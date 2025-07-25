﻿using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Dtos.Responses.Variety;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Domain.Dtos.Responses.Registration
{
    public class CheckQRRegistrationResoponse
    {
        public string? RegistrationNumber { get; set; }

        public string RegisterName { get; set; } = null!;

        public decimal KoiSize { get; set; }

        public int KoiAge { get; set; }

        public KoiProfileCheckinResponse? KoiProfile { get; set; }

        public virtual ICollection<GetKoiMediaResponse> KoiMedia { get; set; } = new List<GetKoiMediaResponse>();

    }
}
