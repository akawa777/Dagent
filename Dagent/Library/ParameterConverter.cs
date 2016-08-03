using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dagent;
using System.Reflection;
using System.Data.Common;

namespace Dagent.Library
{
    internal static class ParameterConverter
    {
        public static Parameter[] GetParameters(object parameters)
        {
            if (parameters != null)
            {
                PropertyInfo[] properties = parameters.GetType().GetProperties();

                Parameter[] rtnParameters = new Parameter[properties.Length];

                for (int i = 0; i < properties.Length; i++ )
                {
                    rtnParameters[i] = new Parameter(properties[i].Name, properties[i].GetValue(parameters, null));
                }

                return rtnParameters;
            }

            return new Parameter[0];
        }

        public static void SetParamters(DbCommand command, Parameter[] parameters, Func<DbParameter, DbParameter> func)
        {
            if (parameters != null)
            {
                foreach (Parameter parameter in parameters)
                {
                    IDbParamterCreator creator = (parameter as IDbParamterCreator);

                    DbParameter dbParamter = creator.CreateParameter(func);

                    command.Parameters.Add(dbParamter);
                }
            }
        }
    }
}
