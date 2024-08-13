using RefactorThis.V2.Persistence.Entities;
using System.Linq.Expressions;

namespace RefactorThis.V2.Persistence.Repositories;

public interface IRepository<TEntity> : IDisposable where TEntity : class, IEntity
{
    Task<TEntity?> GetById(object id, CancellationToken cancellationToken = default);
    Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default);
}