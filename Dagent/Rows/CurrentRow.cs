using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Dagent.Library;
using System.Data.Common;
using System.Data;

namespace Dagent.Rows
{
    internal class CurrentRow : Row, ICurrentRow
    {
        public CurrentRow(Row row)
            : base(row)
        {

        }

        public CurrentRow(Type[] columnTypes, string[] columnNames, object[] values, params string[] uniqueKeys)
            : base(columnTypes, columnNames, values, uniqueKeys)
        {
            
        }

        public CurrentRow(IDataReader dataReader, params string[] uniqueKeys)
            : base(dataReader, uniqueKeys)
        {

        }
    }
}
