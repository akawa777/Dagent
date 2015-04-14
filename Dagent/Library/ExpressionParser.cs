using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Dagent.Library
{
    internal static class ExpressionParser
    {
        public static MemberInfo GetMemberInfo<T, P>(Expression<Func<T, P>> expression)
        {   
            MemberExpression memberExpression = null;            

            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression.Body as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.Convert)
            {
                UnaryExpression unaryExpression = expression.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression != null)
            {
                return memberExpression.Member;
            }
            else
            {
                return null;
            }
        }

        public static PropertyInfo GetPropertyInfo<T, P>(Expression<Func<T, P>> expression)
        {
            MemberInfo member = GetMemberInfo(expression);

            if (member == null)
            {
                return null;
            }
            else
            {
                return member as PropertyInfo;
            }
        }
    }
}
