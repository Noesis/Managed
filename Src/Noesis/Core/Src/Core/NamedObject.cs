using System;

namespace Noesis
{
    internal class NamedObject : BaseComponent
    {
        public NamedObject(string name, IntPtr cPtr) : base(cPtr, false)
        {
            _name = name;
        }
        public override string ToString()
        {
            return _name;
        }

        private string _name;
    }
}

