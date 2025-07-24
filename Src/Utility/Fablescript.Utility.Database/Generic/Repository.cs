using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using Fablescript.Utility.Base.Exceptions;
using Fablescript.Utility.Base.Persistence;
using Fablescript.Utility.Base.UnitOfWork;

namespace Fablescript.Utility.Database.Generic
{
  public class Repository<TEntity, TId, TUoWContext, TDataContext>
    : IRepository<TEntity, TId>
    where TEntity : class, IEntity<TId>
    where TId : notnull
    where TUoWContext : IUnitOfWorkContext
    where TDataContext : DbContext, IDataBaseContext
  {
    #region Dependencies

    readonly IUnitOfWorkProvider<TUoWContext> UnitOfWorkProvider;

    #endregion


    public Repository(IUnitOfWorkProvider<TUoWContext> unitOfWorkProvider)
    {
      UnitOfWorkProvider = unitOfWorkProvider;
    }

    protected TDataContext DataContext => UnitOfWorkProvider?.Current?.Payload<TDataContext>()
        ?? throw new ArgumentNullException("No unit of work provided. Maybe a call to IUnitOfWorkProvider.Start() is missing?");

    public virtual void Add(TEntity entity)
    {
      DataContext.Add(entity);
    }


    public virtual Task AddAsync(TEntity entity)
    {
      return DataContext.AddAsync(entity).AsTask();
    }


    public virtual TEntity Get(TId id)
    {
      var entity = DataContext.Find<TEntity>(id);
      if (entity == null)
        throw new EntityNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");
      return entity;
    }


    public virtual async Task<TEntity> GetAsync(TId id)
    {
      var entity = await DataContext.FindAsync<TEntity>(id);
      if (entity == null)
        throw new EntityNotFoundException($"{typeof(TEntity).Name} with id {id} not found.");
      return entity;
    }


    public virtual bool TryGet(TId id, [MaybeNullWhen(false)] out TEntity entity)
    {
      var e = DataContext.Find<TEntity>(id);
      if (e != null)
      {
        entity = e;
        return true;
      }
      else
      {
        entity = default;
        return false;
      }
    }


    public virtual async Task<(bool, TEntity?)> TryGetAsync(TId id)
    {
      var e = await DataContext.FindAsync<TEntity>(id);
      if (e != null)
        return (true, e);
      else
        return (false, default);
    }


    public virtual IReadOnlyList<TEntity> GetAll()
    {
      return DataContext.Set<TEntity>().ToList().AsReadOnly();
    }


    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
      return (await DataContext.Set<TEntity>().ToListAsync()).AsReadOnly();
    }


    public virtual void Remove(TId id)
    {
      var entity = DataContext.Find<TEntity>(id);
      if (entity != null)
        DataContext.Remove(entity);
    }


    public virtual async Task RemoveAsync(TId id)
    {
      var entity = await DataContext.FindAsync<TEntity>(id);
      if (entity != null)
        DataContext.Remove(entity);
    }
  }
}
