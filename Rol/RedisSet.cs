using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Rol
{
    public interface IRedisSet<T> : IEnumerable<T>
    {
        RedisKey Id { get; }
        bool Add(T value);
        Task<bool> AddAsync(T value);
        Task<long> AddAsync(params T[] values);
        int Count { get; }
        Task<int> CountAsync { get; }
        bool Contains(T value);
        Task<bool> ContainsAsync(T value);
        void Remove(T value);
        Task RemoveAsync(T value);
    }

    class RedisSet<T> : IRedisSet<T>
    {
        public RedisKey _id;
        public RedisKey Id
        {
            get
            {
                return _id;
            }
        }

        public readonly Store Store;

        public RedisSet(RedisKey id, Store store)
        {
            _id = id;
            Store = store;
        }

        public RedisSet()
        {
            
        }

        public IEnumerator<T> GetEnumerator()
        {
            return
                Store.Connection.GetDatabase()
                    .SetMembers(Id)
                    .Select(o => FromRedisValue<T>.Impl.Value(o, Store))
                    .GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public bool Add(T value)
        {
            return Store.Connection.GetDatabase().SetAdd(Id, ToRedisValue<T>.Impl.Value(value));
        }
        public Task<bool> AddAsync(T value)
        {
            return Store.Connection.GetDatabase().SetAddAsync(Id, ToRedisValue<T>.Impl.Value(value));
        }

        public Task<long> AddAsync(params T[] values)
        {
            return Store.Connection.GetDatabase().SetAddAsync(Id, values.Select(ToRedisValue<T>.Impl.Value).ToArray());
        }

        public int Count => (int)Store.Connection.GetDatabase().SetLength(Id);
        public Task<int> CountAsync => (Store.Connection.GetDatabase().SetLengthAsync(Id).ContinueWith(t => (int) t.Result));
        public bool Contains(T value)
        {
            return Store.Connection.GetDatabase().SetContains(Id, ToRedisValue<T>.Impl.Value(value));
        }
        public Task<bool> ContainsAsync(T value)
        {
            return Store.Connection.GetDatabase().SetContainsAsync(Id, ToRedisValue<T>.Impl.Value(value));
        }
        public void Remove(T value)
        {
            Store.Connection.GetDatabase().SetRemove(Id, ToRedisValue<T>.Impl.Value(value));
        }
        public Task RemoveAsync(T value)
        {
            return Store.Connection.GetDatabase().SetRemoveAsync(Id, ToRedisValue<T>.Impl.Value(value));
        }
    }
}