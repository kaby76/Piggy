namespace PiggyGenerator
{
    using Antlr4.Runtime.Tree;
    using System.Collections.Generic;
    using System;

    public class Intercept<K, V> : MultiMap<K, V>
    {
        public void MyAdd(K k, V v)
        {
            this.Add(k, v);
            IParseTree kk = (IParseTree)k;
            IParseTree vv = (IParseTree)v;
            if (OutputEngine.is_ast_node(vv)) throw new Exception();
            if (OutputEngine.is_spec_node(kk)) throw new Exception();
        }

        public HashSet<V> this[K key]
        {
            get
            {
                HashSet<V> v = base[key];
                return v;
            }
        }
    }
}
