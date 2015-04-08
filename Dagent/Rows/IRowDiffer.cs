using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Dagent.Library;
using System.Data.Common;

namespace Dagent.Rows
{
    public interface IRowCompare
    {
        bool Compare(IRow dagentRow, params string[] columnNames);
        //bool Compare(object[] values, params int[] indexes);        
    }
}
