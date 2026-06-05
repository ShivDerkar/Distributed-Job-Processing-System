using DistributedJob.Domain.Entities;

namespace DistributedJob.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}