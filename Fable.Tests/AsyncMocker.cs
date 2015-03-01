using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Moq;

namespace Fable.Tests
{
    internal static class AsyncMocker
    {
        /// <summary>
        /// Creates a mock DbSet with the correct setups for async calls.
        /// </summary>
        internal static Mock<DbSet<TData>> WrapAsAsyncCompatible<TData,TKey>(IQueryable<TData> data, Func<TData, TKey> keySelector) where TData : class 
        {
            var mockSet = new Mock<DbSet<TData>>(MockBehavior.Strict);
            mockSet.As<IDbAsyncEnumerable<TData>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<TData>(data.GetEnumerator()));

            mockSet.As<IQueryable<TData>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<TData>(data.Provider));

            mockSet.As<IQueryable<TData>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<TData>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<TData>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.Setup(m => m.Find(It.IsAny<object[]>()))
                .Returns((object[] keys) => data.FirstOrDefault(item => keySelector(item).Equals((TKey)keys[0])));
            // ReturnsAsync doesn't let you use the value seen by It.IsAny in Setup.
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
                .Returns((object[] keys) =>
                {
                    var tcs = new TaskCompletionSource<TData>();
                    var val = data.FirstOrDefault(item => keySelector(item).Equals((TKey) keys[0]));
                    tcs.SetResult(val);
                    return tcs.Task;
                });

            return mockSet;
        }
    }
}
