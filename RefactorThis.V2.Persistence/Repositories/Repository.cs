using RefactorThis.V2.Persistence.Entities;
using System.Linq.Expressions;

namespace RefactorThis.V2.Persistence.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
    //TODO add Entity framework related code
    public Repository()
    {
    }

    public async Task<TEntity?> Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose() { }

    public async Task<TEntity?> GetById(object id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<TEntity>> GetFilteredAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<TEntity?> Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}