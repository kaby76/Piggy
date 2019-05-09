using System.Collections.Generic;

namespace PiggyGenerator
{
    // SmartSet is a HashSet with a hash value itself, so it can be used in Dictionary<>.
    public class SmartSet<T> : HashSet<T>
    {
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(SmartSet<T>)) return false;
            var o = obj as SmartSet<T>;
            if (Count != o.Count) return false;
            if (!SetEquals(o)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            var hc = Count;
            // Get each element hash code, sum
            foreach (var t in this)
            {
                var thc = t.GetHashCode() * 8;
                hc += thc;
            }

            return hc;
        }
    }
}