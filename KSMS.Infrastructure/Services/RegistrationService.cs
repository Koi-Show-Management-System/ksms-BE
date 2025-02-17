using KSMS.Application.GoogleServices;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using PaymentType = KSMS.Domain.Entities.PaymentType;

namespace KSMS.Infrastructure.Services;

public class RegistrationService : BaseService<RegistrationService>, IRegistrationService
{
    private readonly PayOS _payOs;
    private readonly IMediaService _mediaService;
    private readonly IFirebaseService _firebaseService;
    public RegistrationService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RegistrationService> logger, IHttpContextAccessor httpContextAccessor, PayOS payOs, IMediaService mediaService, IFirebaseService firebaseService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _payOs = payOs;
        _mediaService = mediaService;
        _firebaseService = firebaseService;
    }

    public async Task<object> CreateRegistration(CreateRegistrationRequest createRegistrationRequest)
    {
        var accountId = GetIdFromJwt();
        var koiShow = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiShowId);
        var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiProfileId);
        if (koiShow is null)
        {
            throw new NotFoundException("Show is not existed");
        }

        if (koiProfile is null)
        {
            throw new NotFoundException("Koi is not existed");
        }

        var registrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(predicate: x => x.KoiShowId == koiShow.Id);
        if (registrations.Count > koiShow.MaxParticipants)
        {
            throw new NotFoundException("");
        }
        var registration = createRegistrationRequest.Adapt<Registration>();
        registration.KoiAge = koiProfile.Age;
        registration.KoiSize = koiProfile.Size;
        registration.RegistrationFee = koiShow.RegistrationFee;
        registration.AccountId = accountId;
        registration.Status = RegistrationStatus.Pending.ToString().ToLower();
        await _unitOfWork.GetRepository<Registration>().InsertAsync(registration);
        await _unitOfWork.CommitAsync();
        if (createRegistrationRequest.RegistrationImages is not [])
        {
            await _mediaService.UploadRegistrationImage(createRegistrationRequest.RegistrationImages, registration.Id);
        }
        if (createRegistrationRequest.RegistrationVideos is not [])
        {
            await _mediaService.UploadRegistrationVideo(createRegistrationRequest.RegistrationVideos, registration.Id);
        }

        var registrationDb = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registration.Id,
                include: query => query.Include(r => r.KoiShow)
                    .Include(r => r.Account)
                    .Include(r => r.KoiMedia)
                                                            .Include(r => r.KoiProfile)
                                                            .ThenInclude(r => r.Variety));
        var sendMail = MailUtil.SendEmail(registrationDb.Account.Email,
            MailUtil.ContentMailUtil.Title_ThankingForRegisterSh,
            MailUtil.ContentMailUtil.ConfirmingRegistration(registrationDb), "");
        if (!sendMail)
        {
            throw new BadRequestException("Error sending confirmation email.");
        }
        return new
        {
            Message = "Register successfully"
        };
    }

    public async Task UpdateRegistrationPaymentStatusForPayOs(Guid registrationPaymentId, RegistrationPaymentStatus status)
    {
        var registrationPayment = await _unitOfWork.GetRepository<RegistrationPayment>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationPaymentId,
                include: query => query
                    .Include(r => r.Registration).ThenInclude(r => r.Account)
                    .Include(r => r.Registration).ThenInclude(r => r.CompetitionCategory)
                    .ThenInclude(r => r.KoiShow));
        registrationPayment.Status = status switch
        {
            RegistrationPaymentStatus.Cancelled => RegistrationPaymentStatus.Cancelled.ToString().ToLower(),
            RegistrationPaymentStatus.Paid => RegistrationPaymentStatus.Paid.ToString().ToLower(),
            _ => registrationPayment.Status
        };

        if (registrationPayment.Status == RegistrationPaymentStatus.Paid.ToString().ToLower())
        {
            registrationPayment.QrcodeData = await _firebaseService.UploadImageAsync(
                    FileUtils.ConvertBase64ToFile(QrcodeUtil.GenerateQrCode(registrationPayment.Id)), "qrCode/");
            registrationPayment.Registration.Status = RegistrationStatus.Paid.ToString().ToLower();
            _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registrationPayment);
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registrationPayment.Registration);
            await _unitOfWork.CommitAsync();
            var sendMail = MailUtil.SendEmail(registrationPayment.Registration.Account.Email,
                MailUtil.ContentMailUtil.Title_CheckOutSuccessfully,
                MailUtil.ContentMailUtil.CheckOutSuccess(registrationPayment), "");
            if (!sendMail)
            {
                throw new BadRequestException("Error sending confirmation email.");
            }
        }
    }
    public async Task UpdateStatusForRegistration(Guid registrationId, RegistrationStatus status)
    {
         
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.Id == registrationId,
                include: query => query
                    .Include(r => r.Account)
                    .Include(r => r.KoiMedia)
                    .Include(r => r.KoiProfile)
                    .ThenInclude(kp => kp.Variety)
                    .ThenInclude(v => v.CategoryVarieties)
                    .ThenInclude(cv => cv.CompetitionCategory)
                    .Include(r => r.KoiShow));
        if (registration is null)
        {
            throw new NotFoundException("Registration is not existed");
        }
        var accountId = GetIdFromJwt();
        var showStaff = await _unitOfWork.GetRepository<ShowStaff>()
            .SingleOrDefaultAsync(predicate: s => s.AccountId == accountId && s.KoiShowId == registration.KoiShowId);
        if (showStaff is null)
        {
            throw new ForbiddenMethodException("You are not staff for this show!!!!");
        }
        registration.Status = status switch
        {
            RegistrationStatus.Pending => RegistrationStatus.Pending.ToString().ToLower(),
            RegistrationStatus.Confirm => RegistrationStatus.Confirm.ToString().ToLower(),
            RegistrationStatus.Reject => RegistrationStatus.Reject.ToString().ToLower(),
            //RegistrationStatus.NotEnoughQuota => RegistrationStatus.NotEnoughQuota.ToString().ToLower(),
            //RegistrationStatus.Cancelled => RegistrationStatus.Cancelled.ToString().ToLower(),
            _ => registration.Status
        };
        if (registration.Status == RegistrationStatus.Reject.ToString().ToLower())
        {
            
        }
        if (registration.Status == RegistrationStatus.Confirm.ToString().ToLower())
        {
            var eligibleCategories = registration.KoiProfile.Variety.CategoryVarieties
                .Select(cv => cv.CompetitionCategory)
                .Where(cc => 
                    registration.KoiSize >= cc.SizeMin && 
                    registration.KoiSize <= cc.SizeMax &&
                    cc.KoiShowId == registration.KoiShowId)
                .ToList();

            if (!eligibleCategories.Any())
                throw new BadRequestException("Không tìm thấy hạng mục phù hợp cho cá Koi này");
            var bestCategory = eligibleCategories.MinBy(c => c.SizeMax - c.SizeMin);
            if (bestCategory != null)
            {
                var registrationCount =
                    await _unitOfWork.GetRepository<Registration>().GetListAsync(predicate: x => x.CompetitionCategoryId == bestCategory.Id);
                if (registrationCount.Count > bestCategory.MaxEntries)
                {
                    throw new NotFoundException("The number of participants in the category exceeds the limit");
                }
                registration.CompetitionCategoryId = bestCategory.Id;
                registration.ApprovedAt = DateTime.Now;
                _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
                await _unitOfWork.CommitAsync();
                var sendMail = MailUtil.SendEmail(registration.Account.Email,
                    MailUtil.ContentMailUtil.Title_ApproveForRegisterSh,
                    MailUtil.ContentMailUtil.ConfirmCategoryAssignment(registration, bestCategory), "");
                if (!sendMail)
                {
                    throw new BadRequestException("Error sending confirmation email.");
                }
            }
        }
        
    }

    public async Task<CheckOutRegistrationResponse> CheckOut(Guid registrationId)
    {
        var accountId = GetIdFromJwt();
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationId,
                include: query => query.Include(r => r.KoiProfile)
                    .Include(r => r.RegistrationPayment));
        if (registration is null)
        {
            throw new NotFoundException("Registration is not found");
        }
        
        if (registration.AccountId != accountId)
        {
            throw new ForbiddenMethodException("This registration is not yours!!!!");
        }

        if (registration.Status == RegistrationStatus.Pending.ToString().ToLower())
        {
            throw new BadRequestException("This registration is not confirm!!!");
        }
        if (registration.Status == RegistrationStatus.Paid.ToString().ToLower())
        {
            throw new BadRequestException("This registration is already paid!!!");
        }
        RegistrationPayment registrationPayment;
        if (registration.RegistrationPayment != null)
        {
            registration.RegistrationPayment.PaymentDate = DateTime.Now;
            registration.RegistrationPayment.Status = RegistrationPaymentStatus.Pending.ToString().ToLower(); 
            _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registration.RegistrationPayment);
            registrationPayment = registration.RegistrationPayment;
        }
        else
        {
            registrationPayment = new RegistrationPayment
            {
                RegistrationId = registration.Id,
                Status = RegistrationPaymentStatus.Pending.ToString().ToLower(),
                PaymentMethod = PaymentMethod.PayOs.ToString(),
                PaymentTypeId = (await _unitOfWork.GetRepository<PaymentType>()
                    .SingleOrDefaultAsync(predicate: x => x.Name == PaymentTypes.Registration.ToString())).Id,
                PaidAmount = registration.RegistrationFee,
                PaymentDate = DateTime.Now
            };
            await _unitOfWork.GetRepository<RegistrationPayment>().InsertAsync(registrationPayment);
            
        }
        await _unitOfWork.CommitAsync();
        var registrationCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        var items = new List<ItemData>();
        var item = new ItemData("Registration for " + registration.KoiProfile.Name, 1, (int)registration.RegistrationFee);
        items.Add(item);
        const string baseUrl = "https://localhost:7042/api/Registration" + "/success";
        var url = $"{baseUrl}?registrationPaymentId={registrationPayment.Id}";
        var paymentData = new PaymentData(registrationCode, (int)registration.RegistrationFee, "Registration", items,
            url, url);
        var createPayment = await _payOs.createPaymentLink(paymentData);
        return new CheckOutRegistrationResponse()
        {
            Message = "Checkout Successfully",
            Url = createPayment.checkoutUrl
        };
    }
}