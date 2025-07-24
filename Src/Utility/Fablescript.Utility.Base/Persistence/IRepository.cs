using System.Diagnostics.CodeAnalysis;

namespace Fablescript.Utility.Base.Persistence
{
  public interface IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : notnull
  {
    void Add(TEntity entity);
    Task AddAsync(TEntity entity);
    TEntity Get(TId id);
    Task<TEntity> GetAsync(TId id);
    bool TryGet(TId id, [MaybeNullWhen(false)] out TEntity entity);
    Task<(bool Success, TEntity? Entity)> TryGetAsync(TId id);
    IReadOnlyList<TEntity> GetAll();
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    void Remove(TId id);
    Task RemoveAsync(TId id);
  }
}
