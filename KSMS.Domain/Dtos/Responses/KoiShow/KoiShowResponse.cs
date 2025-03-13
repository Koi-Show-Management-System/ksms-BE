using KSMS.Domain.Dtos.Responses.CompetitionCategory;

using KSMS.Domain.Dtos.Responses.Ticket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KSMS.Domain.Dtos.Responses.KoiShow
{
    public class KoiShowResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartExhibitionDate { get; set; }

        public DateTime? EndExhibitionDate { get; set; }

        public string? Location { get; set; }

        public string? Description { get; set; }

        public DateOnly? RegistrationDeadline { get; set; }

        public int? MinParticipants { get; set; }

        public int? MaxParticipants { get; set; }

        public bool? HasGrandChampion { get; set; }

        public bool? HasBestInShow { get; set; }

        public string? ImgUrl { get; set; }

        public string? Status { get; set; }

       
       
      

    }
}
