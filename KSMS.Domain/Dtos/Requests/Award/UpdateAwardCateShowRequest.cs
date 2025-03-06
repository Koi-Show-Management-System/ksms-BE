using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Requests.Award
{
    public class UpdateAwardCateShowRequest
    {

        public Guid Id {  get; set; }
  //      public Guid CompetitionCategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? AwardType { get; set; }
        [Range(0, 9999999999999999.99, ErrorMessage = "PrizeValue must be between 0 and 9,999,999,999,999,999.99.")]
        public decimal? PrizeValue { get; set; }
        public string? Description { get; set; }
    }
}
