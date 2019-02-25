using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFL.Core.Publish.Model
{
    public class MTypeInfo
    {
        public MTypeInfo(bool isList, string typeName, string desc)
        {
            this.TypeName = typeName;
            this.IsList = isList;
            this.Desc = desc;
        }
        public string TypeName { get; private set; }
        public bool IsList { get; private set; }
        public string Desc { get; private set; }

        public override string ToString()
        {
            return !this.IsList ? this.TypeName : $"List<{this.TypeName}>";
        }
    }
}
