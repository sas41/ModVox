using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IUserRepository
{
    Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task AddAsync(UserAccount user, CancellationToken cancellationToken);
    Task UpdateAsync(UserAccount user, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken);
}
