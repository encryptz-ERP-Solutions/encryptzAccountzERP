using System;

namespace Business.Core.DTOs
{
    public class CreateUserSubscriptionDto
    {
        public Guid BusinessID { get; set; }
        public int PlanID { get; set; }
        public string Status { get; set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime EndDateUTC { get; set; }
        public DateTime? TrialEndsAtUTC { get; set; }
    }
}
