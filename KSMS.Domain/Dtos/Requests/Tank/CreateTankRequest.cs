using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Tank
{
    public class CreateTankRequest
    {
        public Guid KoiShowId { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a positive number.")]
        public int Capacity { get; set; }

        [StringLength(50, ErrorMessage = "WaterType cannot exceed 50 characters.")]
        public string? WaterType { get; set; }

        [Range(0, 99999.99, ErrorMessage = "Temperature must be within decimal(5,2) range.")]
        public decimal? Temperature { get; set; }

        [Range(0, 14, ErrorMessage = "PHLevel must be between 0 and 14.")]
        public decimal? Phlevel { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "Size must be within decimal(10,2) range.")]
        public decimal? Size { get; set; }

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string? Location { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string? Status { get; set; }
    }
}
