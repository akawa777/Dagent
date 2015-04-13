using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Dagent.Library
{
    internal class DynamicMethodBuilder<T>
    {
        //private static readonly Dictionary<string, Action<T, object>> setterCache = new Dictionary<string, Action<T, object>>();
        //public static Action<T, object> CreateSetMethod(PropertyInfo propertyInfo)
        //{
        //    Action<T, object> setterAction;            

        //    if (setterCache.TryGetValue(propertyInfo.Name, out setterAction))
        //    {
        //        return setterAction;
        //    }

        //    /*
        //    * If there's no setter return null
        //    */
        //    MethodInfo setMethod = propertyInfo.GetSetMethod();
        //    if (setMethod == null)
        //        return null;

        //    /*
        //    * Create the dynamic method
        //    */
        //    Type[] arguments = new Type[2];
        //    arguments[0] = arguments[1] = typeof(object);

        //    DynamicMethod setter = new DynamicMethod(
        //        //String.Concat("_Set", propertyInfo.Name, "_"),
        //      "_Set" + propertyInfo.Name + "_",
        //        //typeof(void), arguments, propertyInfo.DeclaringType);
        //      typeof(void), arguments, typeof(T));
        //    ILGenerator generator = setter.GetILGenerator();
        //    generator.Emit(OpCodes.Ldarg_0);
        //    //generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
        //    generator.Emit(OpCodes.Castclass, typeof(T));
        //    generator.Emit(OpCodes.Ldarg_1);

        //    if (propertyInfo.PropertyType.IsClass)
        //        generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
        //    else
        //        generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

        //    generator.EmitCall(OpCodes.Callvirt, setMethod, null);
        //    generator.Emit(OpCodes.Ret);

        //    /*
        //    * Create the delegate and return it
        //    */
        //    setterAction = setter.CreateDelegate(typeof(Action<T, object>)) as Action<T, object>;

        //    setterCache[propertyInfo.Name] = setterAction;

        //    return setterAction;
        //}

        //private static readonly Dictionary<string, Func<T, object>> getterCache = new Dictionary<string, Func<T, object>>();
        //public static Func<T, object> CreateGetMethod(PropertyInfo propertyInfo)
        //{
        //    Func<T, object> getterFunc;

        //    if (getterCache.TryGetValue(propertyInfo.Name, out getterFunc))
        //    {
        //        return getterFunc;
        //    }

        //    /*
        //    * If there's no getter return null
        //    */
        //    MethodInfo getMethod = propertyInfo.GetGetMethod();
        //    if (getMethod == null)
        //        return null;

        //    /*
        //     * Create the dynamic method
        //     */
        //    Type[] arguments = new Type[1];
        //    arguments[0] = typeof(object);

        //    DynamicMethod getter = new DynamicMethod(
        //        //String.Concat("_Get", propertyInfo.Name, "_"),
        //        "_Get" + propertyInfo.Name + "_",
        //        //typeof(object), arguments, propertyInfo.DeclaringType);
        //        typeof(object), arguments, typeof(T));
        //    ILGenerator generator = getter.GetILGenerator();
        //    generator.DeclareLocal(typeof(object));
        //    generator.Emit(OpCodes.Ldarg_0);
        //    //generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
        //    generator.Emit(OpCodes.Castclass, typeof(T));
        //    generator.EmitCall(OpCodes.Callvirt, getMethod, null);

        //    if (!propertyInfo.PropertyType.IsClass)
        //        generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

        //    generator.Emit(OpCodes.Ret);

        //    /*
        //     * Create the delegate and return it
        //     */
        //    getterFunc = getter.CreateDelegate(typeof(Func<T, object>)) as Func<T, object>;

        //    getterCache[propertyInfo.Name] = getterFunc;

        //    return getterFunc;
        //}

        public static Action<T, object> CreateSetMethod(PropertyInfo propertyInfo)
        {
            return DynamicMethodBuilder<T, object>.CreateSetMethod(propertyInfo);
        }
        public static Func<T, object> CreateGetMethod(PropertyInfo propertyInfo)
        {
            return DynamicMethodBuilder<T, object>.CreateGetMethod(propertyInfo);
        }
    }

    internal class DynamicMethodBuilder<T, P>
    {
        private static readonly Dictionary<string, Action<T, P>> setterCache = new Dictionary<string, Action<T, P>>();
        public static Action<T, P> CreateSetMethod(PropertyInfo propertyInfo)
        {
            Action<T, P> setterAction;

            if (setterCache.TryGetValue(propertyInfo.Name, out setterAction))
            {
                return setterAction;
            }

            /*
            * If there's no setter return null
            */
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            if (setMethod == null)
                return null;

            /*
            * Create the dynamic method
            */
            Type[] arguments = new Type[2];
            arguments[0] = typeof(T);
            arguments[1] = typeof(P);

            DynamicMethod setter = new DynamicMethod(
                //String.Concat("_Set", propertyInfo.Name, "_"),
              "_Set" + propertyInfo.Name + "_",
                //typeof(void), arguments, propertyInfo.DeclaringType);
              typeof(void), arguments, typeof(T));
            ILGenerator generator = setter.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            //generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.Emit(OpCodes.Castclass, typeof(T));
            generator.Emit(OpCodes.Ldarg_1);

            if (propertyInfo.PropertyType.IsClass)
                generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            else
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

            generator.EmitCall(OpCodes.Callvirt, setMethod, null);
            generator.Emit(OpCodes.Ret);

            /*
            * Create the delegate and return it
            */
            setterAction = setter.CreateDelegate(typeof(Action<T, P>)) as Action<T, P> ;

            setterCache[propertyInfo.Name] = setterAction;

            return setterAction;
        }

        private static readonly Dictionary<string, Func<T, P>> getterCache = new Dictionary<string, Func<T, P>>();
        public static Func<T, P> CreateGetMethod(PropertyInfo propertyInfo)
        {
            Func<T, P> getterFunc;

            if (getterCache.TryGetValue(propertyInfo.Name, out getterFunc))
            {
                return getterFunc;
            }

            /*
            * If there's no getter return null
            */
            MethodInfo getMethod = propertyInfo.GetGetMethod();
            if (getMethod == null)
                return null;

            /*
             * Create the dynamic method
             */
            Type[] arguments = new Type[1];
            arguments[0] = typeof(T);

            DynamicMethod getter = new DynamicMethod(
                //String.Concat("_Get", propertyInfo.Name, "_"),
                "_Get" + propertyInfo.Name + "_",
                //typeof(object), arguments, propertyInfo.DeclaringType);
                typeof(P), arguments, typeof(T));
            ILGenerator generator = getter.GetILGenerator();
            generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Ldarg_0);
            //generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            generator.Emit(OpCodes.Castclass, typeof(T));
            generator.EmitCall(OpCodes.Callvirt, getMethod, null);

            if (!propertyInfo.PropertyType.IsClass)
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

            generator.Emit(OpCodes.Ret);

            /*
             * Create the delegate and return it
             */
            getterFunc = getter.CreateDelegate(typeof(Func<T, P>)) as Func<T, P>;

            getterCache[propertyInfo.Name] = getterFunc;

            return getterFunc;
        }
    }
}
