using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace SourceLog.Plugin.Perforce
{
    // http://www.zorched.net/2009/01/01/register-and-unregister-com-dll-from-net-code/
    public class Registrar : IDisposable
    {
        private IntPtr hLib;

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        internal delegate int PointerToMethodInvoker();

        public Registrar(string filePath)
        {
            hLib = LoadLibrary(filePath);
            if (IntPtr.Zero == hLib)
            {
                int errno = Marshal.GetLastWin32Error();
                throw new Win32Exception(errno, "Failed to load library.");
            }
        }

        public void RegisterComDLL()
        {
            CallPointerMethod("DllRegisterServer");
        }

        public void UnRegisterComDLL()
        {
            CallPointerMethod("DllUnregisterServer");
        }

        private void CallPointerMethod(string methodName)
        {
            IntPtr dllEntryPoint = GetProcAddress(hLib, methodName);
            if (IntPtr.Zero == dllEntryPoint)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            PointerToMethodInvoker drs =
                   (PointerToMethodInvoker)Marshal.GetDelegateForFunctionPointer(dllEntryPoint,
                               typeof(PointerToMethodInvoker));
            drs();
        }

        public void Dispose()
        {
            if (IntPtr.Zero != hLib)
            {
                UnRegisterComDLL();
                FreeLibrary(hLib);
                hLib = IntPtr.Zero;
            }
        }
    }
}
