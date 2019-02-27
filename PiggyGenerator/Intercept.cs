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
            IParseTree kk = (IParseTree)k;
            IParseTree vv = (IParseTree)v;
            if (OutputEngine.is_ast_node(vv)) throw new Exception();
            if (OutputEngine.is_spec_node(kk)) throw new Exception();
#if DEBUG
            System.Console.WriteLine("----");
            System.Console.WriteLine(kk.GetType());
            System.Console.WriteLine(kk.SourceInterval);
            System.Console.WriteLine(kk.GetText());
            System.Console.WriteLine("----");
            System.Console.WriteLine(vv.GetType());
            System.Console.WriteLine(vv.SourceInterval);
            System.Console.WriteLine(vv.GetText());
            System.Console.WriteLine("----");
            System.Console.WriteLine(b);
#endif
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
