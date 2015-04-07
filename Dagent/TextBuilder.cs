using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Dagent
{
    public class TextBuilder : ITextBuilder
    {
        public TextBuilder()
        {
            
        }

        public TextBuilder(string template)
        {
            Append(template);
        }

        public TextBuilder(string template, object parameters)
        {
            Append(template, parameters);
        }

        public TextBuilder(string template, Dictionary<string, string> parameters)
        {
            Append(template, parameters);
        }

        private string front = "{{";
        private string rear = "}}";

        private StringBuilder sbText = new StringBuilder();

        public ITextBuilder PlaceHolder(string front, string rear)
        {
            this.front = front;
            this.rear = rear;

            return this;
        }

        public ITextBuilder Append(string template)
        {
            sbText.Append(template);

            return this;
        }

        public ITextBuilder Append(string template, object parameters)
        {
            if (parameters == null) return this;

            Dictionary<string, string> parameterMap = new Dictionary<string, string>();

            foreach (System.Reflection.PropertyInfo property in parameters.GetType().GetProperties())
            {
                if (property.PropertyType != typeof(string)) continue;

                parameterMap[property.Name] = property.GetValue(parameters, null) as string;
            }

            return Append(template, parameterMap);
        }

        public ITextBuilder Append(string template, Dictionary<string, string> parameters)
        {
            if (parameters == null) return this;

            string rtnText = template;

            foreach (var keyValue in parameters)
            {
                rtnText = rtnText.Replace(front + keyValue.Key + rear, keyValue.Value);
            }

            sbText.Append(rtnText);

            return this;
        }        

        public string Generate()
        {
            return sbText.ToString();
        }

        public ITextBuilder Clear()
        {
            sbText = new StringBuilder();
            return this;

        }
    }
}
