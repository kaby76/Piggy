namespace PiggyGenerator
{
    using System.Collections.Generic;

    public class MultiMap<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
    {
        public MultiMap() : base() { }

        public MultiMap(MultiMap<TKey, TValue> other) : base(other) { }

        public MultiMap(int capacity) : base(capacity) { }

        public MultiMap(IEqualityComparer<TKey> comparer) : base(comparer) { }

        public void Add(TKey key, TValue value)
        {
            HashSet<TValue> valueList;
            if (TryGetValue(key, out valueList))
            {
                valueList.Add(value);
            }
            else
            {
                valueList = new HashSet<TValue>();
                valueList.Add(value);
                Add(key, valueList);
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            HashSet<TValue> valueList;

            if (TryGetValue(key, out valueList))
            {
                if (valueList.Remove(value))
                {
                    if (valueList.Count == 0)
                    {
                        Remove(key);
                    }
                    return true;
                }
            }
            return false;
        }

        public int RemoveAll(TKey key, TValue value)
        {
            HashSet<TValue> valueList;
            int n = 0;
            if (TryGetValue(key, out valueList))
            {
                while (valueList.Remove(value))
                {
                    n++;
                }
                if (valueList.Count == 0)
                {
                    Remove(key);
                }
            }
            return n;
        }

        public int CountAll
        {
            get
            {
                int n = 0;

                foreach (HashSet<TValue> valueList in Values)
                {
                    n += valueList.Count;
                }
                return n;
            }
        }

        public bool Contains(TKey key, TValue value)
        {
            HashSet<TValue> valueList;
            if (TryGetValue(key, out valueList))
            {
                return valueList.Contains(value);
            }
            return false;
        }

        public bool Contains(TValue value)
        {
            foreach (HashSet<TValue> valueList in Values)
            {
                if (valueList.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
