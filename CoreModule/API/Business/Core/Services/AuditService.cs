using System;
using System.Threading.Tasks;
using BusinessLogic.Core.Interface;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task LogAsync(Guid? userId, string action, string objectType, string objectId, string notes = null)
        {
            await _auditRepository.LogAsync(userId, action, objectType, objectId, notes);
        }
    }
}

