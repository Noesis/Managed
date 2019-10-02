using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    public partial class Texture
    {
        private class ManagedTexture
        {
            public object Texture;
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified D3D11 texture native handle
        /// <param name="nativePointer">ID3D11Texture2D native pointer</param>
        /// </summary>
        public static Texture WrapD3D11Texture(object texture, IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted)
        {
            Texture tex = WrapD3D11(nativePointer, width, height, numMipMaps, isInverted);

            if (texture != null)
            {
                tex.SetPrivateData(new ManagedTexture { Texture = texture });
            }

            return tex;
        }

        /// <summary>
        /// Creates a Texture wrapper for the specified GL texture native handle
        /// <param name="nativePointer">GLuint texture name</param>
        /// </summary>
        public static Texture WrapGLTexture(object texture, IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted)
        {
            Texture tex = WrapGL(nativePointer, width, height, numMipMaps, isInverted);

            if (texture != null)
            {
                tex.SetPrivateData(new ManagedTexture { Texture = texture });
            }

            return tex;
        }

        #region Imports

        #if !UNITY_5_3_OR_NEWER
        static Texture()
        {
            Noesis.GUI.Init();
        }
        #endif

        private static Texture WrapD3D11(IntPtr nativePointer, int width, int height,
            int numMipMaps, bool isInverted)
        {
            IntPtr texPtr = Noesis_WrapD3D11Texture(nativePointer, width, height, numMipMaps, isInverted);
            return new Texture(texPtr, true);
        }

        private static Texture WrapGL(IntPtr nativePointer, int width, int height, int numMipMaps,
            bool isInverted)
        {
            IntPtr texPtr = Noesis_WrapGLTexture(nativePointer, width, height, numMipMaps, isInverted);
            return new Texture(texPtr, true);
        }

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_WrapD3D11Texture(IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted);

        [DllImport(Library.Name)]
        private static extern IntPtr Noesis_WrapGLTexture(IntPtr nativePointer,
            int width, int height, int numMipMaps, bool isInverted);

        #endregion
    }

}
