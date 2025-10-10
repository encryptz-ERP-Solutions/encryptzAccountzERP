using System;

namespace Business.Core.DTOs
{
    public class UpdateUserSubscriptionDto
    {
        public string Status { get; set; }
        public DateTime EndDateUTC { get; set; }
        public DateTime? TrialEndsAtUTC { get; set; }
    }
}
