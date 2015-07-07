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
    internal interface IRow : IBaseRow
    {
        int ColumnCount { get; }

        Type GetColumnType(int i);

        string GetColumnName(int i);

        int GetOrdinal(string columnName);        

        object this[int i] { get; set; }

        IRow PrevRow { get; set; }

        void SetValue(object[] values);

        bool TryGetValue(string columnName, out object value);
    }
}
