using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ATF.Scripts.Storage.Utils.Nito.Collections
{
    internal static class CollectionHelpers
    {
        public static IReadOnlyCollection<T> ReifyCollection<T>(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source is IReadOnlyCollection<T>)
            {
                var result = (IReadOnlyCollection<T>) source;
                return result;
            }

            if (source is ICollection<T>)
            {
                var collection = (ICollection<T>) source;
                return new CollectionWrapper<T>(collection);
            }

            if (source is ICollection)
            {
                var nonGenericCollection = (ICollection) source;
                return new NonGenericCollectionWrapper<T>(nonGenericCollection);
            }
            
            return new List<T>(source);
        }

        private sealed class NonGenericCollectionWrapper<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection _collection;

            public NonGenericCollectionWrapper(ICollection collection)
            {
                if (collection != null)
                {
                    _collection = collection;
                }
                else
                {
                    throw new ArgumentNullException(nameof(collection));
                }
            }

            public int Count => _collection.Count;

            public IEnumerator<T> GetEnumerator()
            {
                return _collection.Cast<T>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }
        }

        private sealed class CollectionWrapper<T> : IReadOnlyCollection<T>
        {
            private readonly ICollection<T> _collection;

            public CollectionWrapper(ICollection<T> collection)
            {
                if (collection != null)
                {
                    _collection = collection;
                }
                else
                {
                    throw new ArgumentNullException(nameof(collection));    
                }
            }

            public int Count => _collection.Count;

            public IEnumerator<T> GetEnumerator()
            {
                return _collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _collection.GetEnumerator();
            }
        }
    }
}
