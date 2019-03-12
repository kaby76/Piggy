namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;
    using System;

    public class Intercept<K, V> : MultiMap<K, V>
    {
        public void MyAdd(K k, V v)
        {
            this.TryGetValue(k, out List<V> value);
            var b = value?.Contains(v);
            this.Add(k, v);
        }

        public List<V> this[K key]
        {
            get
            {
                List<V> v = base[key];
                return v;
            }
        }
    }
}
