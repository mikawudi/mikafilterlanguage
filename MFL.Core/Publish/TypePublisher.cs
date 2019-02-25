using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MFL.Core.Publish.Model;

namespace MFL.Core.Publish
{
    public class TypePublisher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="needDesc">过滤不包含描述属性的字段和属性</param>
        /// <returns></returns>
        public static TypeMap PublishTypeInfo<T>(bool needDesc = false)
            where T : class
        {
            Stack<Type> source = new Stack<Type>();
            Dictionary<string, PublishInfo> result = new Dictionary<string, PublishInfo>();

            source.Push(typeof(T));
            while (source.Count != 0)
            {
                var nType = source.Pop();
                if (result.ContainsKey(nType.Name))
                    continue;
                List<MemberInfo> infoList = GetPropertyAndField(nType, needDesc);
                var pubInfo = new PublishInfo(nType.Name, infoList);
                result.Add(nType.Name, pubInfo);
                foreach (var property in infoList)
                {
                    var dType = property is PropertyInfo ? ((PropertyInfo)property).PropertyType : ((FieldInfo)property).FieldType;
                    if (dType.IsGenericType)
                    {
                        var genType = dType.GetGenericTypeDefinition();
                        if (genType == typeof(List<>) || genType == typeof(IList<>))
                        {
                            dType = dType.GetGenericArguments().First();
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (!dType.IsValueType && !(dType == typeof(string)))
                    {
                        source.Push(dType);
                    }
                }
            }
            return new TypeMap(typeof(T).Name, result);
        }

        private static List<MemberInfo> GetPropertyAndField(Type nType, bool needDesc)
        {
            var properties = nType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = nType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            List<MemberInfo> infoList = new List<MemberInfo>();
            infoList.AddRange(properties);
            infoList.AddRange(fields);
            if (needDesc)
            {
                infoList = infoList.Where(x =>
                {
                    var atts = x.GetCustomAttributes(typeof(DescAttribute), false);
                    return atts != null && atts.Length > 0;
                }).ToList();
            }

            return infoList;
        }
    }
}
