using System;
using System.Runtime.InteropServices;

namespace Common
{
   internal static class AppManagerHelper
   {
      const string DLL_NAME = "AdAppMgrSvcInt.dll";

      static IntPtr dllHandle = IntPtr.Zero;

      // Define the delegates for the library. 
      // TODO: need to add marshal type to support the delegate
      internal delegate bool GetAvailProductUpdateDelegate(string product, string release, string master, string build);

      // Declare delegate instances for every function in DLL
      static internal GetAvailProductUpdateDelegate GetAvailProductUpdate;

      // this static constructor will load the library
      static AppManagerHelper()
      {
         try
         {

            // Initialze the dynamic marshall Infrastructure to call the 32Bit (x86) or the 64Bit (x64) Dll respectively 
            SYSTEM_INFO info = new SYSTEM_INFO();
            GetSystemInfo(ref info);

            // Load the correct DLL according to the processor architecture
            switch (info.wProcessorArchitecture)
            {
               case PROCESSOR_ARCHITECTURE.Intel:
                  string pathx86 = AppDomain.CurrentDomain.BaseDirectory;
                  if (!pathx86.EndsWith("\\"))
                     pathx86 += "\\";
                  pathx86 += @"x86\" + DLL_NAME; // Place your X86 DLL name here

                  dllHandle = LoadLibrary(pathx86);
                  if (dllHandle == IntPtr.Zero)
                     throw new DllNotFoundException("x86 DLL not found");
                  break;

               case PROCESSOR_ARCHITECTURE.Amd64:
                  string pathx64 = AppDomain.CurrentDomain.BaseDirectory;
                  if (!pathx64.EndsWith("\\"))
                     pathx64 += "\\";
                  pathx64 += @"x64\" + DLL_NAME; // Place your X64 DLL name here

                  dllHandle = LoadLibrary(pathx64);
                  if (dllHandle == IntPtr.Zero)
                     throw new DllNotFoundException("x64 DLL not found");
                  break;

               default:
                  throw new NotSupportedException("Platform not Supported");
                  break;
            }

            // init the delegate instances with the function delegates of your libaray
            GetAvailProductUpdate = (GetAvailProductUpdateDelegate)GetDelegate("GetAvailProductUpdate", typeof(GetAvailProductUpdateDelegate));

         }
         catch (Exception e)
         {
            if (dllHandle != IntPtr.Zero)
               FreeLibrary(dllHandle);

            throw e;
         }
      }

      static Delegate GetDelegate(string procName, Type delegateType)
      {
         IntPtr procAdress = GetProcAddress(dllHandle, procName);
         if (procAdress == IntPtr.Zero)
            throw new EntryPointNotFoundException("Function: " + procName);

         return Marshal.GetDelegateForFunctionPointer(procAdress, delegateType);

      }


      #region Marshal data type
      internal enum PROCESSOR_ARCHITECTURE : ushort
      {
         Intel = 0,
         MIPS = 1,
         Alpha = 2,
         PPC = 3,
         SHX = 4,
         ARM = 5,
         IA64 = 6,
         Alpha64 = 7,
         Amd64 = 9,
         Unknown = 0xFFFF
      }


      [StructLayout(LayoutKind.Sequential)]
      internal struct SYSTEM_INFO
      {
         internal PROCESSOR_ARCHITECTURE wProcessorArchitecture;
         internal ushort wReserved;
         internal uint dwPageSize;
         internal IntPtr lpMinimumApplicationAddress;
         internal IntPtr lpMaximumApplicationAddress;
         internal IntPtr dwActiveProcessorMask;
         internal uint dwNumberOfProcessors;
         internal uint dwProcessorType;
         internal uint dwAllocationGranularity;
         internal ushort dwProcessorLevel;
         internal ushort dwProcessorRevision;
      }

      #endregion


      #region kernal32
      [DllImport("kernel32.dll")]
      internal static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

      [DllImport("kernel32.dll")]
      internal static extern IntPtr LoadLibrary(string fileName);

      [DllImport("kernel32.dll", SetLastError = true)]
      static extern bool FreeLibrary(IntPtr hModule);

      [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
      internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
      #endregion
   }
}
