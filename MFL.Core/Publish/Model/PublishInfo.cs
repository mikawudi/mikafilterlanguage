using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFL.Core.Publish.Model
{
    public class PublishInfo
    {
        public Dictionary<string, MTypeInfo> PropertiesInfo { get; private set; } = new Dictionary<string, MTypeInfo>();
        public string Name;

        public PublishInfo(string typeName, List<MemberInfo> memberInfo)
        {
            /*
            this.Name = type.Name;
            var peoperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            var infoList = new List<MemberInfo>();
            infoList.AddRange(peoperties);
            infoList.AddRange(fields);
            */
            this.Name = typeName;
            foreach (var info in memberInfo)
            {
                if (GetTypeName(info, out var typename))
                    PropertiesInfo.Add(info.Name, typename);
            }
        }

        private bool GetTypeName(MemberInfo memInfo, out MTypeInfo result)
        {
            result = null;
            string descStr = null;
            var pType = memInfo is PropertyInfo ? ((PropertyInfo)memInfo).PropertyType : ((FieldInfo)memInfo).FieldType;
            var atts = memInfo.GetCustomAttributes(typeof(DescAttribute), false);
            if (atts != null && atts.Length > 0)
                descStr = ((DescAttribute)atts[0]).Desc;
            if (pType.IsGenericType)
            {
                var genType = pType.GetGenericTypeDefinition();
                if (genType == typeof(List<>) || genType == typeof(IList<>))
                {
                    result = new MTypeInfo(true, pType.GetGenericArguments().First().Name, descStr);//$"List<{pType.GetGenericArguments().First().Name}>";
                    return true;
                }
                return false;
            }
            else
            {
                result = new MTypeInfo(false, pType.Name, descStr);//pType.Name;
                return true;
            }
        }
    }
}
