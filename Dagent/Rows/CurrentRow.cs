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
        public CurrentRow(Row row, bool canSetValue)
            : base(row, canSetValue)
        {

        }

        public CurrentRow(IDataReader dataReader)
            : base(dataReader)
        {

        }
    }
}
