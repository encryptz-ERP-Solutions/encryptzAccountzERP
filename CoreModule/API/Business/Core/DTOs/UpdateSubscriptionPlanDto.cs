namespace Business.Core.DTOs
{
    public class UpdateSubscriptionPlanDto
    {
        public string PlanName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int MaxUsers { get; set; }
        public int MaxBusinesses { get; set; }
        public bool IsPubliclyVisible { get; set; }
        public bool IsActive { get; set; }
    }
}
