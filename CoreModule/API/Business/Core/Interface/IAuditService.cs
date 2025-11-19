using System;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IAuditService
    {
        Task LogAsync(Guid? userId, string action, string objectType, string objectId, string notes = null);
    }
}

