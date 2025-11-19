using System;
using System.Threading.Tasks;

namespace Repository.Core.Interface
{
    public interface IAuditRepository
    {
        Task LogAsync(Guid? userId, string action, string objectType, string objectId, string notes = null);
    }
}

