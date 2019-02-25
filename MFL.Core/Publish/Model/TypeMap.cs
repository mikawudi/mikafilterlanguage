using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFL.Core.Publish.Model
{
    public class TypeMap
    {
        public string Root { get; private set; }
        public Dictionary<string, PublishInfo> Mapper { get; private set; }
        public TypeMap(string root, Dictionary<string, PublishInfo> mapper)
        {
            this.Root = root;
            this.Mapper = mapper;
        }
    }
}
