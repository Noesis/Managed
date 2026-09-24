using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace Noesis
{
    public partial class BaseComponent
    {
        [DynamicDependency("Extend")]
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

        public void Dispose()
        {
            if (!IsDisposed)
            {
                ForceRelease(this, swigCPtr.Handle);
                GC.SuppressFinalize(this);
            }
        }

        internal static void ForceRelease(object instance, IntPtr cPtr)
        {
            BaseComponent component = instance as BaseComponent;
            if (component != null)
            {
                component.swigCPtr = new HandleRef(null, IntPtr.Zero);
                Noesis.Extend.RemoveProxy(cPtr);
            }

            Release(cPtr);
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
            return Noesis.Extend.NewCPtr(type);
        }

        public static IntPtr GetPtr(object instance)
        {
            return Noesis.Extend.GetInstanceHandle(instance).Handle;
        }

        public static object GetProxy(IntPtr ptr)
        {
            return Noesis.Extend.GetProxy(ptr, false);
        }

        public static object GetProxy(IntPtr ptr, bool ownMemory)
        {
            return Noesis.Extend.GetProxy(ptr, ownMemory);
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

            if ((object)a != null)
            {
                if ((object)b != null)
                {
                    if (a.swigCPtr.Handle == b.swigCPtr.Handle)
                    {
                        // will be true if both proxies point to the same native pointer
                        return true;
                    }
                    else
                    {
                        // last check, we do the comparison using the native Equals
                        return EqualsHelper(a.swigCPtr.Handle, b.swigCPtr.Handle);
                    }
                }
                else
                {
                    // will be true if A is a disposed proxy
                    return a.swigCPtr.Handle == IntPtr.Zero;
                }
            }
            else
            {
                if ((object)b != null)
                {
                    // will be true if B is a disposed proxy
                    return b.swigCPtr.Handle == IntPtr.Zero;
                }
                else
                {
                    // both are null
                    return true;
                }
            }
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
