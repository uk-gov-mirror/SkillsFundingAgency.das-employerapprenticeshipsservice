﻿using System;
using SFA.DAS.EAS.Portal.Client.Types;

namespace SFA.DAS.EAS.Portal.Client.Services.DasRecruit.Models
{
    internal class VacancySummary
    {
        public string Title { get; set; }
        public long? VacancyReference { get; set; }
        public string Status { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string TrainingTitle { get; set; }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
        public string RaaManageVacancyUrl { get; set; }
        public ApplicationMethod ApplicationMethod { get; set; }
    }
}
