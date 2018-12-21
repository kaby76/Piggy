using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Utilities;

namespace Piggy
{
    public class TemplatesBase
    {
        private List<Action> _actions = new List<Action>();

        public TemplatesBase() { }

        public virtual int AddInitialization(Action a)
        {
            var result = _actions.Count();
            _actions.Add(a);
            return result;
        }

        public virtual void ExecuteInitializations()
        {
            foreach (var a in _actions) a();
        }
    }
}
