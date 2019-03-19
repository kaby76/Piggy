using System;
using System.Collections.Generic;
using System.Text;

namespace PiggyGenerator
{
    public class SmartSet<T> : HashSet<T>
    {
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(SmartSet<T>)) return false;
            var o = obj as SmartSet<T>;
            if (this.Count != o.Count) return false;
            if (!this.SetEquals(o)) return false;
            return true;
        }
        public override int GetHashCode()
        {
            int hc = this.Count;
            // Get each element hash code, sum
            foreach (var t in this)
            {
                var thc = (t.GetHashCode()) * 8;
                hc += thc;
            }
            return hc;
        }
    }
}
