
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.TicketType
{
    public class CreateTicketTypeRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; } = null!;
        
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 99999999.99, ErrorMessage = "Price must be between 0.01 and 99,999,999.99.")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "AvailableQuantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AvailableQuantity must be greater than 0.")]
        public int AvailableQuantity { get; set; }
    }
}
