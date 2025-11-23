using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
        Task<RefreshToken?> GetByIdAsync(Guid refreshTokenId);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
        Task<RefreshToken> AddAsync(RefreshToken refreshToken);
        Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
        Task<bool> RevokeTokenAsync(Guid refreshTokenId, string? revokedByIP = null);
        Task<bool> RevokeAllUserTokensAsync(Guid userId);
        Task<int> DeleteExpiredTokensAsync();
    }
}

