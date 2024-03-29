﻿namespace System.Web.Script.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Resources;

    internal static class ObjectConverter
    {
        private static Type _dictionaryGenericType = typeof(Dictionary<,>);
        private static Type _enumerableGenericType = typeof(IEnumerable<>);
        private static Type _idictionaryGenericType = typeof(IDictionary<,>);
        private static Type _listGenericType = typeof(List<>);
        private static readonly Type[] s_emptyTypeArray = new Type[0];

        private static bool AddItemToList(IList oldList, IList newList, Type elementType, JavaScriptSerializer serializer, bool throwOnError)
        {
            IEnumerator enumerator = oldList.GetEnumerator();
            {
                while (enumerator.MoveNext())
                {
                    object obj2;
                    if (!ConvertObjectToTypeMain(enumerator.Current, elementType, serializer, throwOnError, out obj2))
                    {
                        return false;
                    }
                    newList.Add(obj2);
                }
            }
            return true;
        }

        private static bool AssignToPropertyOrField(object propertyValue, object o, string memberName, JavaScriptSerializer serializer, bool throwOnError)
        {
            IDictionary dictionary = o as IDictionary;
            if (dictionary != null)
            {
                if (!ConvertObjectToTypeMain(propertyValue, null, serializer, throwOnError, out propertyValue))
                {
                    return false;
                }
                dictionary[memberName] = propertyValue;
                return true;
            }
            Type type = o.GetType();
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                MethodInfo setMethod = property.GetSetMethod();
                if (setMethod != null)
                {
                    if (!ConvertObjectToTypeMain(propertyValue, property.PropertyType, serializer, throwOnError, out propertyValue))
                    {
                        return false;
                    }
                    try
                    {
                        object[] parameters = new object[] { propertyValue };
                        setMethod.Invoke(o, parameters);
                        return true;
                    }
                    catch
                    {
                        if (throwOnError)
                        {
                            throw;
                        }
                        return false;
                    }
                }
            }
            FieldInfo field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (field != null)
            {
                if (!ConvertObjectToTypeMain(propertyValue, field.FieldType, serializer, throwOnError, out propertyValue))
                {
                    return false;
                }
                try
                {
                    field.SetValue(o, propertyValue);
                    return true;
                }
                catch
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    return false;
                }
            }
            return true;
        }

        private static Dictionary<string, Dictionary<string, string>> _fieldMapCache = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> GetFieldMap(Type type)
        {
            string fullName = type.FullName;
            Dictionary<string, string> result;
            if (_fieldMapCache.TryGetValue(fullName, out  result))
            {
                return result;
            }
            Type attrType = typeof(FieldMapAttribute);
            result = new Dictionary<string, string>();
            foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                CheckFieldMapAttribute(result, info, attrType);
            }
            foreach (PropertyInfo info in type.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance))
            {
                CheckFieldMapAttribute(result, info, attrType);
            }
            _fieldMapCache[fullName] = result;
            return result;
        }

        private static void CheckFieldMapAttribute(Dictionary<string, string> result, MemberInfo info, Type attrType)
        {
            if (!info.IsDefined(attrType)) return;
            FieldMapAttribute attr = info.GetCustomAttribute<FieldMapAttribute>();
            if (attr == null || string.IsNullOrEmpty(attr.As)) return;
            result[attr.As] = info.Name;
        }
        private static bool ConvertDictionaryToObject(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer, bool throwOnError, out object convertedObject)
        {
            Type t = type;
            string id = null;
            object o = dictionary;
            if ((id != null) || IsClientInstantiatableType(t, serializer))
            {
                o = Activator.CreateInstance(t);
            }
            List<string> list = new List<string>(dictionary.Keys);
            if (IsGenericDictionary(type))
            {
                Type type3 = type.GetGenericArguments()[0];
                if ((type3 != typeof(string)) && (type3 != typeof(object)))
                {
                    if (throwOnError)
                    {
                        object[] args = new object[] { type.FullName };
                        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DictionaryTypeNotSupported, args));
                    }
                    convertedObject = null;
                    return false;
                }
                Type type4 = type.GetGenericArguments()[1];
                IDictionary dictionary2 = null;
                if (IsClientInstantiatableType(type, serializer))
                {
                    dictionary2 = (IDictionary) Activator.CreateInstance(type);
                }
                else
                {
                    Type[] typeArguments = new Type[] { type3, type4 };
                    dictionary2 = (IDictionary) Activator.CreateInstance(_dictionaryGenericType.MakeGenericType(typeArguments));
                }
                if (dictionary2 != null)
                {
                    foreach (string str2 in list)
                    {
                        object obj4;
                        if (!ConvertObjectToTypeMain(dictionary[str2], type4, serializer, throwOnError, out obj4))
                        {
                            convertedObject = null;
                            return false;
                        }
                        dictionary2[str2] = obj4;
                    }
                    convertedObject = dictionary2;
                    return true;
                }
            }
            if ((type != null) && !type.IsAssignableFrom(o.GetType()))
            {
                if (!throwOnError)
                {
                    convertedObject = null;
                    return false;
                }
                if (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, s_emptyTypeArray, null) == null)
                {
                    object[] objArray2 = new object[] { type.FullName };
                    throw new MissingMethodException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_NoConstructor, objArray2));
                }
                object[] objArray3 = new object[] { type.FullName };
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_DeserializerTypeMismatch, objArray3));
            }
            Dictionary<string, string> maps = new Dictionary<string, string>();
            if (t != null) maps = GetFieldMap(t);
            foreach (string str3 in list)
            {
                string key = str3;
                string str4;
                if (t != null && maps.TryGetValue(str3, out  str4))
                {
                    key = str4;
                }
                if (!AssignToPropertyOrField(dictionary[str3], o, key, serializer, throwOnError))
                {
                    convertedObject = null;
                    return false;
                }
            }
            convertedObject = o;
            return true;
        }


        private static bool ConvertListToObject(IList list, Type type, JavaScriptSerializer serializer, bool throwOnError, out IList convertedList)
        {
            if (((type == null) || (type == typeof(object))) || IsArrayListCompatible(type))
            {
                Type elementType = typeof(object);
                if ((type != null) && (type != typeof(object)))
                {
                    elementType = type.GetElementType();
                }
                ArrayList newList = new ArrayList();
                if (!AddItemToList(list, newList, elementType, serializer, throwOnError))
                {
                    convertedList = null;
                    return false;
                }
                if (((type == typeof(ArrayList)) || (type == typeof(IEnumerable))) || ((type == typeof(IList)) || (type == typeof(ICollection))))
                {
                    convertedList = newList;
                    return true;
                }
                convertedList = newList.ToArray(elementType);
                return true;
            }
            if (type.IsGenericType && (type.GetGenericArguments().Length == 1))
            {
                Type type3 = type.GetGenericArguments()[0];
                Type[] typeArguments = new Type[] { type3 };
                if (_enumerableGenericType.MakeGenericType(typeArguments).IsAssignableFrom(type))
                {
                    Type[] typeArray2 = new Type[] { type3 };
                    Type type4 = _listGenericType.MakeGenericType(typeArray2);
                    IList list3 = null;
                    if (IsClientInstantiatableType(type, serializer) && typeof(IList).IsAssignableFrom(type))
                    {
                        list3 = (IList) Activator.CreateInstance(type);
                    }
                    else
                    {
                        if (type4.IsAssignableFrom(type))
                        {
                            if (throwOnError)
                            {
                                object[] args = new object[] { type.FullName };
                                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_CannotCreateListType, args));
                            }
                            convertedList = null;
                            return false;
                        }
                        list3 = (IList) Activator.CreateInstance(type4);
                    }
                    if (!AddItemToList(list, list3, type3, serializer, throwOnError))
                    {
                        convertedList = null;
                        return false;
                    }
                    convertedList = list3;
                    return true;
                }
            }
            else if (IsClientInstantiatableType(type, serializer) && typeof(IList).IsAssignableFrom(type))
            {
                IList list4 = (IList) Activator.CreateInstance(type);
                if (!AddItemToList(list, list4, null, serializer, throwOnError))
                {
                    convertedList = null;
                    return false;
                }
                convertedList = list4;
                return true;
            }
            if (throwOnError)
            {
                object[] objArray2 = new object[] { type.FullName };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_ArrayTypeNotSupported, objArray2));
            }
            convertedList = null;
            return false;
        }

        internal static object ConvertObjectToType(object o, Type type, JavaScriptSerializer serializer)
        {
            object obj2;
            ConvertObjectToTypeMain(o, type, serializer, true, out obj2);
            return obj2;
        }

        private static bool ConvertObjectToTypeInternal(object o, Type type, JavaScriptSerializer serializer, bool throwOnError, out object convertedObject)
        {
            IDictionary<string, object> dictionary = o as IDictionary<string, object>;
            if (dictionary != null)
            {
                return ConvertDictionaryToObject(dictionary, type, serializer, throwOnError, out convertedObject);
            }
            IList list = o as IList;
            if (list != null)
            {
                IList list2;
                if (ConvertListToObject(list, type, serializer, throwOnError, out list2))
                {
                    convertedObject = list2;
                    return true;
                }
                convertedObject = null;
                return false;
            }
            if ((type == null) || (o.GetType() == type))
            {
                convertedObject = o;
                return true;
            }
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(o.GetType()))
            {
                try
                {
                    convertedObject = converter.ConvertFrom(null, CultureInfo.InvariantCulture, o);
                    return true;
                }
                catch
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    convertedObject = null;
                    return false;
                }
            }
            if (converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    string str;
                    if (o is DateTime)
                    {
                        DateTime time = (DateTime) o;
                        str = time.ToUniversalTime().ToString("u", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        str = TypeDescriptor.GetConverter(o).ConvertToInvariantString(o);
                    }
                    convertedObject = converter.ConvertFromInvariantString(str);
                    return true;
                }
                catch
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                    convertedObject = null;
                    return false;
                }
            }
            if (type.IsAssignableFrom(o.GetType()))
            {
                convertedObject = o;
                return true;
            }
            if (throwOnError)
            {
                object[] args = new object[] { o.GetType(), type };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, AtlasWeb.JSON_CannotConvertObjectToType, args));
            }
            convertedObject = null;
            return false;
        }

        private static bool ConvertObjectToTypeMain(object o, Type type, JavaScriptSerializer serializer, bool throwOnError, out object convertedObject)
        {
            if (o == null)
            {
                if (type == typeof(char))
                {
                    convertedObject = '\0';
                    return true;
                }
                if (IsNonNullableValueType(type))
                {
                    if (throwOnError)
                    {
                        throw new InvalidOperationException(AtlasWeb.JSON_ValueTypeCannotBeNull);
                    }
                    convertedObject = null;
                    return false;
                }
                convertedObject = null;
                return true;
            }
            if (o.GetType() == type)
            {
                convertedObject = o;
                return true;
            }
            return ConvertObjectToTypeInternal(o, type, serializer, throwOnError, out convertedObject);
        }

        private static bool IsArrayListCompatible(Type type)
        {
            if ((!type.IsArray && !(type == typeof(ArrayList))) && (!(type == typeof(IEnumerable)) && !(type == typeof(IList))))
            {
                return (type == typeof(ICollection));
            }
            return true;
        }

        internal static bool IsClientInstantiatableType(Type t, JavaScriptSerializer serializer)
        {
            if (((t == null) || t.IsAbstract) || (t.IsInterface || t.IsArray))
            {
                return false;
            }
            if (t == typeof(object))
            {
                return false;
            }

            if (t.IsValueType)
            {
                return true;
            }
            if (t.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, s_emptyTypeArray, null) == null)
            {
                return false;
            }
            return true;
        }

        private static bool IsGenericDictionary(Type type)
        {
            if (((type == null) || !type.IsGenericType) || (!typeof(IDictionary).IsAssignableFrom(type) && !(type.GetGenericTypeDefinition() == _idictionaryGenericType)))
            {
                return false;
            }
            return (type.GetGenericArguments().Length == 2);
        }

        private static bool IsNonNullableValueType(Type type)
        {
            if ((type == null) || !type.IsValueType)
            {
                return false;
            }
            if (type.IsGenericType)
            {
                return !(type.GetGenericTypeDefinition() == typeof(Nullable<>));
            }
            return true;
        }

        internal static bool TryConvertObjectToType(object o, Type type, JavaScriptSerializer serializer, out object convertedObject) => 
            ConvertObjectToTypeMain(o, type, serializer, false, out convertedObject);
    }
}

