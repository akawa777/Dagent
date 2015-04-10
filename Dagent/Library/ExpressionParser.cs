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
        private static class MemberExpressionCache<T>
        {
            static MemberExpressionCache()
            {
                Map = new Dictionary<string, MemberExpression>();
            }

            public static Dictionary<string, MemberExpression> Map { get; set; }
        }
        

        public static Dictionary<string, MemberInfo> GetMemberInfoMap<T>(params Expression<Func<T, object>>[] expressions)
        {
            Dictionary<string, MemberInfo> memberInfoMap = new Dictionary<string, MemberInfo>();

            if (expressions == null) return memberInfoMap;

            for (int i = 0; i < expressions.Length; i++)
            {
                MemberInfo memberInfo = GetMemberInfo<T>(expressions[i]);
                memberInfoMap[memberInfo.Name] = memberInfo;
            }

            return memberInfoMap;
        }

        public static MemberInfo[] GetMemberInfos<T>(params Expression<Func<T, object>>[] expressions)
        {
            MemberInfo[] memberInfos = new MemberInfo[expressions.Length];

            if (expressions == null) return memberInfos;

            for (int i = 0; i < expressions.Length; i++)
            {
                memberInfos[i] = GetMemberInfo<T>(expressions[i]);
            }

            return memberInfos;
        }

        public static MemberInfo GetMemberInfo<T>(Expression<Func<T, object>> expression)
        {           
            
            MemberExpression memberExpression;            

            if (MemberExpressionCache<T>.Map.TryGetValue(expression.Body.ToString().Split('.')[1], out memberExpression))
            {
                return memberExpression.Member;
            }

            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = (MemberExpression)expression.Body;
                MemberExpressionCache<T>.Map[expression.Body.ToString().Split('.')[1]] = memberExpression;
                
            }
            else if (expression.Body.NodeType == ExpressionType.Convert)
            {
                UnaryExpression unaryExpression = (UnaryExpression)expression.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
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

        public static MemberInfo GetMemberInfo<T, P>(Expression<Func<T, P>> expression)
        {
            MemberExpression memberExpression;

            if (MemberExpressionCache<T>.Map.TryGetValue(expression.Body.ToString().Split('.')[1], out memberExpression))
            {
                return memberExpression.Member;
            }

            if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = (MemberExpression)expression.Body;
                MemberExpressionCache<T>.Map[expression.Body.ToString().Split('.')[1]] = memberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.Convert)
            {
                UnaryExpression unaryExpression = (UnaryExpression)expression.Body;
                memberExpression = (MemberExpression)unaryExpression.Operand;
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
    }
}
