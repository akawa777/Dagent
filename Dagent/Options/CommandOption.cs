using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Dagent.Rows;

namespace Dagent.Options
{
    internal class CommandOption<T> where T : new()
    {
        public CommandOption()
        {
            PrimaryKeys = new Dictionary<string, Func<T, object>>();
            AutoMapping = true;
            MapAction = (row, model) => { };            
        }

        public Dictionary<string, Func<T, object>> PrimaryKeys { get; set; }        
        public virtual bool AutoMapping { get; set; }
        public virtual Action<IUpdateRow, T> MapAction { get; set; }        
    }
}
