using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Ticket
{
    public class CreateTicketRequest
    {
         

        [Required(ErrorMessage = "ShowId is required.")]
        public Guid ShowId { get; set; }

        [Required(ErrorMessage = "TicketType is required.")]
        [StringLength(100, ErrorMessage = "TicketType must not exceed 100 characters.")]
        public string TicketType { get; set; } = null!;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "AvailableQuantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AvailableQuantity must be at least 1.")]
        public int AvailableQuantity { get; set; }
    }
}
