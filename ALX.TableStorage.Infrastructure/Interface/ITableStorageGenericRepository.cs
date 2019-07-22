using System.Collections.Generic;
using System.Threading.Tasks;

namespace ALX.TableStorage.Infrastructure.Interface
{
    public interface ITableStorageGenericRepository<T> where T : TableStorageBaseEntity, new()
    {
        Task<List<T>> All(int? size);
        Task<List<T>> GetListByPartitionKey(string partitionKey);
        Task<T> Get(string partitionKey, string rowKey);
        Task InsertOrUpdate(T item);
        Task Delete(T item);
        Task Delete(string partitionKey, string rowKey);
    }
}