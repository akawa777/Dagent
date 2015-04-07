using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Dagent
{
    public interface ITextBuilder
    {
        ITextBuilder PlaceHolder(string front, string rear);
        ITextBuilder Append(string template);
        ITextBuilder Append(string template, object parameters);
        ITextBuilder Append(string template, Dictionary<string, string> parameters);
        string Generate();
        ITextBuilder Clear();
    }
}
