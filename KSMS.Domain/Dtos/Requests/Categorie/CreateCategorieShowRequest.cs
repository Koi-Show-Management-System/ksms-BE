
using KSMS.Domain.Dtos.Requests.Round;
using KSMS.Domain.Dtos.Responses.Award;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Categorie
{ 
    public class CreateCategorieShowRequest
    {
        [Required(ErrorMessage = "ShowId is required.")]
        public Guid ShowId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = null!;

        [Range(0, 1000, ErrorMessage = "SizeMin must be between 0 and 1000.")]
        public decimal? SizeMin { get; set; }

        [Range(0, 1000, ErrorMessage = "SizeMax must be between 0 and 1000.")]
        public decimal? SizeMax { get; set; }

        public Guid? VarietyId { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(1, 100, ErrorMessage = "MaxEntries must be between 1 and 100.")]
        public int? MaxEntries { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(50, ErrorMessage = "Status must not exceed 50 characters.")]
        public string? Status { get; set; }

        public virtual ICollection<CreateRoundRequest> Rounds { get; set; } = new List<CreateRoundRequest>();
        public virtual ICollection<CreateAwardCateShowRequest> Awards { get; set; } = new List<CreateAwardCateShowRequest>();
    }

}
