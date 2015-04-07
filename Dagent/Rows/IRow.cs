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
    public interface IRow
    {
        int ColumnCount { get; }

        Type GetColumnType(int i);

        string GetColumnName(int i);

        int GetOrdinal(string columnName);

        bool ContainsColumn(string columnName);

        T Get<T>(string columnName);        

        object this[string columnName] { get; set; }        

        object this[int i] { get; set; }

        object[] Values { get; }

        string[] ColumnNames { get; }
    }
}
