using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dagent
{
    public class Parameter
    {
        public Parameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public virtual string Name { get; set; }
        public virtual object Value { get; set; }
    }
}
