using System.Collections.Generic;

namespace PiggyGenerator
{
    public class Intercept<K, V> : MultiMap<K, V>
    {
        public List<V> this[K key]
        {
            get
            {
                var v = base[key];
                return v;
            }
        }

        public void MyAdd(K k, V v)
        {
            TryGetValue(k, out var value);
            var b = value?.Contains(v);
            Add(k, v);
        }
    }
}