using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFL.Core.ExpTreeCompiler
{

    public class MContext
    {
        private const string ROOT = "root";
        public Type Root { get; private set; }
        private List<Tuple<string, Type>> Stack = new List<Tuple<string, Type>>();

        public MContext(Type root)
        {
            this.Root = root;
            Stack.Add(new Tuple<string, Type>(ROOT, root));
        }

        public Tuple<string, Type> Current
        {
            get { return Stack[Stack.Count - 1]; }
        }

        public Tuple<string, Type> Get()
        {
            if (Stack.Count == 0)
                throw new Exception();
            var result = Stack[Stack.Count - 1];
            Stack.RemoveAt(Stack.Count - 1);
            return result;
        }

        public void Set(Type t, string name)
        {
            Stack.Add(new Tuple<string, Type>(name, t));
        }
    }
}
