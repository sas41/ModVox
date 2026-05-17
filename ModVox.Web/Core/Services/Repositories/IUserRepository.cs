using ModVox.Web.Domain;

namespace ModVox.Web.Repositories;

public interface IUserRepository
{
    Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, UserAccount>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(UserAccount user, CancellationToken cancellationToken);
    Task UpdateAsync(UserAccount user, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken);
}
