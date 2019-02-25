using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFL.Core.Publish.Model
{
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, AllowMultiple = false)]
    public class DescAttribute : Attribute
    {
        public string Desc { get; set; }

        public DescAttribute(string desc)
        {
            this.Desc = desc;
        }
    }
}
