using System;
using System.Runtime.InteropServices;

namespace Noesis
{

    public partial class BaseComponent
    {
        protected BaseComponent()
        {
            Type type = this.GetType();

            if (Noesis.Extend.NeedsCreateCPtr(type))
            {
                // Instance created from C#, we need to create C++ native object
                bool registerExtend;
                IntPtr cPtr = CreateCPtr(type, out registerExtend);
                Init(cPtr, true, registerExtend);
                Noesis.Extend.Initialize(this);
            }
            else
            {
                // Extended instance created from C++, where native object is already created
                bool registerExtend = true;
                IntPtr cPtr = Noesis.Extend.GetCPtr(this, type);
                Init(cPtr, false, registerExtend);
            }
        }

        private void Init(IntPtr cPtr, bool cMemoryOwn, bool registerExtend)
        {
            swigCPtr = new HandleRef(this, cPtr);

            if (registerExtend)
            {
                // NOTE: Instance added to the Extend Table before AddReference is called, so when
                // instance is grabbed table entry can be transformed into a strong reference
                Noesis.Extend.RegisterExtendInstance(this);
            }
            else
            {
                Noesis.Extend.AddProxy(this);
            }

            if (cPtr != IntPtr.Zero && !cMemoryOwn)
            {
                AddReference(cPtr);
            }

            if (registerExtend)
            {
                Noesis.Extend.RegisterInterfaces(this);
            }
        }

        internal static void ForceRelease(object instance, IntPtr cPtr)
        {
            lock (instance)
            {
                BaseComponent component = instance as BaseComponent;
                if (component != null)
                {
                    component.swigCPtr = new HandleRef(null, IntPtr.Zero);
                    Noesis.Extend.RemoveProxy(cPtr);
                }
                else
                {
                    Noesis.Extend.ForceRemoveExtend(instance, cPtr);
                }

                Release(cPtr);
            }
        }

        private void ReleaseProxy(IntPtr cPtr)
        {
            Noesis.Extend.RemoveProxy(cPtr);
            Noesis.Extend.AddPendingRelease(cPtr);
        }

        protected virtual IntPtr CreateCPtr(Type type, out bool registerExtend)
        {
            return CreateExtendCPtr(type, out registerExtend);
        }

        protected IntPtr CreateExtendCPtr(Type type, out bool registerExtend)
        {
            registerExtend = true;
            return Noesis.Extend.NewCPtr(type, this);
        }

        public bool IsDisposed
        {
            get { return swigCPtr.Handle == IntPtr.Zero; }
        }

        public static bool operator ==(BaseComponent a, BaseComponent b)
        {
            // If both are null, or both are the same instance, return true.
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            // Return true if wrapped c++ objects match:
            return a.swigCPtr.Handle == b.swigCPtr.Handle;
        }

        public static bool operator !=(BaseComponent a, BaseComponent b)
        {
            return !(a == b);
        }

        public override bool Equals(object o)
        {
            return this == o as BaseComponent;
        }

        public override int GetHashCode()
        {
            return swigCPtr.Handle.GetHashCode();
        }
    }

}
