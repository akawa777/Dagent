using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dagent;
using System.Reflection;

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

        public static KeyValuePair<string, object>[] GetKeyValuePairs(Parameter[] parameters)
        {
            if (parameters == null) return new KeyValuePair<string, object>[0];

            KeyValuePair<string, object>[] rtnKeyValueParis = new KeyValuePair<string, object>[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                rtnKeyValueParis[i] = new KeyValuePair<string, object>(parameters[i].Name, parameters[i].Value);
            }

            return rtnKeyValueParis;
        }
    }
}
