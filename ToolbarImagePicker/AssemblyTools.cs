using System;
using System.Runtime.InteropServices;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	internal static class AssemblyTools
	{
		public static string GetAssemblyPath(string name)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			var finalName = name;
			var aInfo = new AssemblyInfo {cchBuf = 1024};
			aInfo.currentAssemblyPath = new string('\0', aInfo.cchBuf);

			IAssemblyCache ac;
			var hr = CreateAssemblyCache(out ac, 0);
			if (hr < 0) return aInfo.currentAssemblyPath;
			hr = ac.QueryAssemblyInfo(0, finalName, ref aInfo);
			return hr < 0 ? null : aInfo.currentAssemblyPath;
		}

		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
		private interface IAssemblyCache
		{
			void Reserved0();

			[PreserveSig]
			int QueryAssemblyInfo(int flags, [MarshalAs(UnmanagedType.LPWStr)] string assemblyName, ref AssemblyInfo assemblyInfo);
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct AssemblyInfo
		{
			public readonly int cbAssemblyInfo;
			public readonly int assemblyFlags;
			public readonly long assemblySizeInKB;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string currentAssemblyPath;
			public int cchBuf; // size of path buf.
		}

		[DllImport("fusion.dll")]
		private static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, int reserved);
	}
}
