using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace Dagent
{
    internal class TempDbParamter : DbParameter
    {
        public override DbType DbType { get; set; }

        public override ParameterDirection Direction { get; set; }

        public override bool IsNullable { get; set; }

        public override string ParameterName { get; set; }

        public override void ResetDbType()
        {
            DbType = DbType.String;
        }

        public override int Size { get; set; }

        public override string SourceColumn { get; set; }

        public override bool SourceColumnNullMapping { get; set; }

        public override DataRowVersion SourceVersion { get; set; }

        public override object Value { get; set; }
    }

    public interface IDbParamterCreator
    {
        DbParameter CreateParameter(Func<DbParameter, DbParameter> func);        
    }

    public class Parameter : IDbParamterCreator
    {
        public Parameter(string name, object value)
        {
            TempDbParamter parameter = new TempDbParamter();
            parameter.ParameterName = name;
            parameter.Value = value;
            this.parameter = parameter;
            this.parameterForValue = parameter;
        }

        private DbParameter parameter;
        private DbParameter parameterForValue;
        private string prefixName = string.Empty;

        public virtual string Name 
        { 
            get
            {
                return parameter.ParameterName;
            }
            set
            {
                parameter.ParameterName = value;
            }
        }        

        public virtual object Value 
        {
            get
            {
                return parameterForValue.Value;
            }
            set
            {
                parameterForValue.Value = value;
            }
        }

        public virtual ParameterDirection Direction 
        { 
            get
            {
                return parameter.Direction;
            }
            set
            {
                parameter.Direction = value;
            }
        }

        DbParameter IDbParamterCreator.CreateParameter(Func<DbParameter, DbParameter> func)
        {
            DbParameter parameter = func(this.parameter);
            parameterForValue = parameter;

            return parameter;
        }
    }
}
