﻿using System;

namespace SFA.DAS.EAS.Portal.Events.Reservations
{
    public class ReservationCreatedEvent
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public string CourseName { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}