using KSMS.Domain.Dtos.Responses.Award;
using KSMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Categorie
{
    public class CategorieResponse
    {
        public Guid ShowId { get; set; }

        public string Name { get; set; } = null!;

        public decimal? SizeMin { get; set; }

        public decimal? SizeMax { get; set; }

        public Guid? VarietyId { get; set; }

        public string? Description { get; set; }

        public int? MaxEntries { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? Status { get; set; }

        public virtual ICollection<AwardResponse> Awards { get; set; } = new List<AwardResponse>();
    }
}
