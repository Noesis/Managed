using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public class Memory
    {
        /// <summary>
        /// Gets current allocated native memory
        /// </summary>
        public static uint Current { get { return Noesis_GetAllocatedMemory(); } }

        /// <summary>
        /// Gets accumulated allocated memory
        /// </summary>
        public static uint Accumulated { get { return Noesis_GetAllocatedMemoryAccum(); } }

        /// <summary>
        /// Gets the total number of memory allocations
        /// </summary>
        public static uint Allocs { get { return Noesis_GetAllocationsCount(); } }


        [DllImport(Library.Name)]
        private static extern uint Noesis_GetAllocatedMemory();

        [DllImport(Library.Name)]
        private static extern uint Noesis_GetAllocatedMemoryAccum();

        [DllImport(Library.Name)]
        private static extern uint Noesis_GetAllocationsCount();
    }
}

