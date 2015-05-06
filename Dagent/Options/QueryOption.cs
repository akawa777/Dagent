using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using Dagent.Models;
using Dagent.Rows;

namespace Dagent.Options
{
    //internal class QueryOption<T> where T : new()
    //{
    //    public QueryOption() 
    //    {
    //        Parameters = new Parameter[0];
    //        AutoMapping = true;
    //        MapAction = (model, row) => { };            
    //        UniqueColumnNames = new string[0];
    //    }

    //    public virtual Parameter[] Parameters { get; set; }
    //    public virtual string[] UniqueColumnNames { get; set; }
    //    public virtual string PrefixColumnName { get; set; }
    //    public virtual bool AutoMapping { get; set; }
    //    public virtual Action<T, ICurrentRow> MapAction { get; set; }
    //}
}
