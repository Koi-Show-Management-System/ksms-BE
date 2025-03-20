using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.RegistrationPayment;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Services
{
    public class RegistrationPaymentService : IRegistrationPaymentService
    {
        private readonly IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;

        public RegistrationPaymentService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RegistrationPaymentResponse> GetRegistrationPaymentByIdAsync(Guid id)
        {
            var paymentRepository = _unitOfWork.GetRepository<RegistrationPayment>();

            var payment = await paymentRepository.SingleOrDefaultAsync(
                predicate: p => p.Id == id , include: p =>
                    p.Include(r => r.Registration)
                        .ThenInclude(r => r.KoiProfile)
                        .ThenInclude(r => r.Variety)
                        .Include(r => r.Registration)
                        .ThenInclude(r => r.KoiMedia)
                        .Include(r => r.Registration)
                        .ThenInclude(r => r.CompetitionCategory)
                        .ThenInclude(r => r.KoiShow)
            );
            if (payment.Registration.IsCheckedIn == true)
            {
                throw new BadRequestException("Mã QR đã được check-in.");
            }
            if (payment == null)
            {
                throw new NotFoundException("Mã QR không hợp lệ");
            }

            return payment.Adapt<RegistrationPaymentResponse>();
        }
    }
}