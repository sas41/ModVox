using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Infrastructure.Persistence.Repositories;

public sealed class EfTagRepository : ITagRepository
{
    private readonly ModVoxDbContext _db;
    public EfTagRepository(ModVoxDbContext db) => _db = db;

    public async Task AddAsync(TagRecord tag, CancellationToken cancellationToken)
    {
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<TagRecord?> GetByIdAsync(Guid tagId, CancellationToken cancellationToken)
        => await _db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tagId, cancellationToken);

    public async Task<TagRecord?> GetByLabelAsync(string label, CancellationToken cancellationToken)
        => await _db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Label == label, cancellationToken);

    public async Task<IReadOnlyList<TagRecord>> ListAsync(CancellationToken cancellationToken)
        => await _db.Tags.AsNoTracking().OrderBy(t => t.Label).ToListAsync(cancellationToken);

    public async Task UpdateAsync(TagRecord tag, CancellationToken cancellationToken)
    {
        _db.Tags.Update(tag);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid tagId, CancellationToken cancellationToken)
    {
        await _db.Tags.Where(t => t.Id == tagId).ExecuteDeleteAsync(cancellationToken);
    }
}
