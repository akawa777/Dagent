using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dagent.Exceptions
{
    internal static class ExceptionMessges
    {
        private static string dagentError = "[ Dagent Error] ";

        public static string NotExistColumnName(string columnName)
        {
            return dagentError + string.Format("There does not exist that the \"{0}\" Column Name", columnName);
        }
        public static string NotExistProperty(Type type, string columnName)
        {
            return dagentError + string.Format("Property of \"{0}\" Class does not exist that the \"{1}\" Column Name", type.FullName, columnName);
        }

    }
}
