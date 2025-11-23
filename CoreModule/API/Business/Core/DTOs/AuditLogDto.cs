using System;

namespace BusinessLogic.Core.DTOs
{
    public class AuditLogDto
    {
        public long AuditLogId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Guid? ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
        public DateTime ChangedAtUtc { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class AuditLogFilterDto
    {
        public string? TableName { get; set; }
        public string? Action { get; set; }
        public DateTime? StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public int Limit { get; set; } = 100;
    }
}

