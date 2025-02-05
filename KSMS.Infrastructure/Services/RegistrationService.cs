using System.Security.Claims;
using KSMS.Application.Extensions;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using PaymentType = KSMS.Domain.Entities.PaymentType;

namespace KSMS.Infrastructure.Services;

public class RegistrationService : BaseService<RegistrationService>, IRegistrationService
{
    private readonly PayOS _payOs;
    private readonly IFirebaseService _firebaseService;
    public RegistrationService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RegistrationService> logger, IFirebaseService firebaseService, PayOS payOs) : base(unitOfWork, logger)
    {
        _firebaseService = firebaseService;
        _payOs = payOs;
    }

    public async Task<RegistrationResponse> CreateRegistrationWithPayOs(ClaimsPrincipal claims, CreateRegistrationRequest createRegistrationRequest)
    {
        var accountId = claims.GetAccountId();
        
        var variety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(predicate:v => v.Id == createRegistrationRequest.VarietyId);
        var category = await _unitOfWork.GetRepository<Category>().SingleOrDefaultAsync(
            predicate: c => c.Id == createRegistrationRequest.CategoryId,
            include: query => query.Include(c => c.Show));
        var registrationCount =
            await _unitOfWork.GetRepository<Registration>().GetListAsync(predicate: x => x.CategoryId == category.Id);
        if (registrationCount.Count > category.MaxEntries)
        {
            throw new NotFoundException("");
        }
        if (variety is null)
        {
            throw new NotFoundException("Variety is not found");
        }
        if (category is null)
        {
            throw new NotFoundException("Category is not found");
        }
        if (category.VarietyId is not null)
        {
            if (variety.Id != category.VarietyId)
            {
                throw new BadRequestException(
                    "Variety of Koi for your registration is not match with Variety of this category");
            }
        }
        if (createRegistrationRequest.Size < category.SizeMin || createRegistrationRequest.Size > category.SizeMax)
        {
            throw new BadRequestException("Your size of koi is not in range of category");
        }
        var registration = createRegistrationRequest.Adapt<Registration>();
        registration.RegistrationFee = category.Show.RegistrationFee;
        registration.ImgUrl = await _firebaseService.UploadImageAsync(createRegistrationRequest.Img, "koi/");
        registration.VideoUrl = await _firebaseService.UploadImageAsync(createRegistrationRequest.Video, "koi/");
        registration.AccountId = accountId;
        registration.Status = RegistrationStatus.Pending.ToString().ToLower();
        await _unitOfWork.GetRepository<Registration>().InsertAsync(registration);
        await _unitOfWork.CommitAsync();
        var registrationPayment = new RegistrationPayment
        {
            RegistrationId = registration.Id,
            Status = RegistrationPaymentStatus.Pending.ToString().ToLower(),
            PaymentMethod = PaymentMethod.PayOs.ToString(),
            PaymentTypeId = (await _unitOfWork.GetRepository<PaymentType>()
                .SingleOrDefaultAsync(predicate: x => x.Name == PaymentTypes.Registration.ToString())).Id,
            PaidAmount = category.Show.RegistrationFee,
            PaymentDate = DateTime.Now
        };
        await _unitOfWork.GetRepository<RegistrationPayment>().InsertAsync(registrationPayment);
        await _unitOfWork.CommitAsync();
        var registrationCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
        var items = new List<ItemData>();
        var item = new ItemData("Registration for " + registration.Name, 1, (int)category.Show.RegistrationFee);
        items.Add(item);
        const string baseUrl = "https://localhost:7042/api/Registration" + "/success";
        var url = $"{baseUrl}?registrationPaymentId={registrationPayment.Id}";
        var paymentData = new PaymentData(registrationCode, (int)category.Show.RegistrationFee, "Registration", items,
            url, url);
        var createPayment = await _payOs.createPaymentLink(paymentData);
        return new RegistrationResponse()
        {
            Message = "Register Successfully",
            Url = createPayment.checkoutUrl
        };
    }

    public async Task UpdateRegistrationPaymentStatusForPayOs(Guid registrationPaymentId, RegistrationPaymentStatus status)
    {
        var registrationPayment = await _unitOfWork.GetRepository<RegistrationPayment>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationPaymentId,
                include: query => query
                    .Include(r => r.Registration).ThenInclude(r => r.Account)
                    .Include(r => r.Registration).ThenInclude(r => r.Variety)
                    .Include(r => r.Registration).ThenInclude(r => r.Category)
                    .ThenInclude(r => r.Show));
        registrationPayment.Status = status switch
        {
            RegistrationPaymentStatus.Cancelled => RegistrationPaymentStatus.Cancelled.ToString().ToLower(),
            RegistrationPaymentStatus.Paid => RegistrationPaymentStatus.Paid.ToString().ToLower(),
            _ => registrationPayment.Status
        };
        _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registrationPayment);
        await _unitOfWork.CommitAsync();
        if (registrationPayment.Status == RegistrationPaymentStatus.Paid.ToString().ToLower())
        {
            var sendMail = MailUtil.SendEmail(registrationPayment.Registration.Account.Email,
                MailUtil.ContentMailUtil.Title_ThankingForRegisterSh,
                MailUtil.ContentMailUtil.ConfirmingRegistration(registrationPayment), "");
            if (!sendMail)
            {
                throw new BadRequestException("Error sending confirmation email.");
            }
        }
    }
    public async Task UpdateStatusForRegistration(Guid registrationId, RegistrationStatus status)
    {
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationId,
                include: query => query.Include(r => r.RegistrationPayment)
                    .Include(r => r.Category).ThenInclude(r => r.Show)
                    .Include(r => r.Account));
        registration.Status = status switch
        {
            RegistrationStatus.Pending => RegistrationStatus.Pending.ToString().ToLower(),
            RegistrationStatus.Confirm => RegistrationStatus.Confirm.ToString().ToLower(),
            //RegistrationStatus.NotEnoughQuota => RegistrationStatus.NotEnoughQuota.ToString().ToLower(),
            //RegistrationStatus.Cancelled => RegistrationStatus.Cancelled.ToString().ToLower(),
            _ => registration.Status
        };
        _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
        
        if (registration.Status == RegistrationStatus.Confirm.ToString().ToLower())
        {
            var qrCode = new Qrcode()
            {
                RegistrationPaymentId = registration.RegistrationPayment.Id,
                ExpiryDate = registration.Category.Show.StartDate?.AddMinutes(30) ?? DateTime.Now.AddMinutes(30),
                IsActive = true
            };
            await _unitOfWork.GetRepository<Qrcode>().InsertAsync(qrCode);
            await _unitOfWork.CommitAsync();
            qrCode.QrcodeData = await _firebaseService.UploadImageAsync(
                FileUtils.ConvertBase64ToFile(QrcodeUtil.GenerateQrCode(qrCode.Id)), "qrCode/");
            _unitOfWork.GetRepository<Qrcode>().UpdateAsync(qrCode);
            await _unitOfWork.CommitAsync();
            var sendMail = MailUtil.SendEmail(registration.Account.Email,
                MailUtil.ContentMailUtil.Title_ApproveForRegisterSh,
                MailUtil.ContentMailUtil.SendApprovalEmail(registration, qrCode.QrcodeData, qrCode.ExpiryDate), "");
            if (!sendMail)
            {
                throw new BadRequestException("Error sending confirmation email.");
            }
        }
        
    }
}