using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

namespace OurCraft.Graphics.OpenGL_Objects
{
    //contains function pointers for supporting bindless textures in OpenGL
    public static class Bindless
    {
        //delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate ulong GetTextureHandleDelegate(uint texture);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MakeTextureHandleResidentDelegate(ulong handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MakeTextureHandleNonResidentDelegate(ulong handle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UniformHandleui64Delegate(int location, ulong value);

        //function pointers
        public static GetTextureHandleDelegate GetTextureHandle;
        public static MakeTextureHandleResidentDelegate MakeTextureHandleResident;
        public static MakeTextureHandleNonResidentDelegate MakeTextureHandleNonResident;
        public static UniformHandleui64Delegate UniformHandleui64;

        //try to load all bindless texture function pointers
        static Bindless()
        {
            //get pointers
            IntPtr ptr1 = GLFW.GetProcAddress("glGetTextureHandleARB");
            IntPtr ptr2 = GLFW.GetProcAddress("glMakeTextureHandleResidentARB");
            IntPtr ptr3 = GLFW.GetProcAddress("glMakeTextureHandleNonResidentARB");
            IntPtr ptr4 = GLFW.GetProcAddress("glUniformHandleui64ARB");

            //validate if pointers exist
            if (ptr1 == IntPtr.Zero) throw new Exception("glGetTextureHandleARB not supported");
            if (ptr2 == IntPtr.Zero) throw new Exception("glMakeTextureHandleResidentARB not supported");
            if (ptr3 == IntPtr.Zero) throw new Exception("glMakeTextureHandleNonResidentARB not supported");
            if (ptr4 == IntPtr.Zero) throw new Exception("glUniformHandleui64ARB not supported");

            //convert to c# delegate
            GetTextureHandle = Marshal.GetDelegateForFunctionPointer<GetTextureHandleDelegate>(ptr1);
            MakeTextureHandleResident = Marshal.GetDelegateForFunctionPointer<MakeTextureHandleResidentDelegate>(ptr2);
            MakeTextureHandleNonResident = Marshal.GetDelegateForFunctionPointer<MakeTextureHandleNonResidentDelegate>(ptr3);
            UniformHandleui64 = Marshal.GetDelegateForFunctionPointer<UniformHandleui64Delegate>(ptr4);
        }
    }
}
