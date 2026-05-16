using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfUserRepository : IUserRepository
{
    private readonly ModVoxDbContext _db;
    public EfUserRepository(ModVoxDbContext db) => _db = db;

    public async Task<UserAccount?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
        => await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        => await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task AddAsync(UserAccount user, CancellationToken cancellationToken)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserAccount user, CancellationToken cancellationToken)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken)
        => await _db.Users.OrderBy(u => u.Username).ToListAsync(cancellationToken);
}
