using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using EnvDTE;

namespace Jcl.Tools.WindowsForms.ToolbarImagePicker
{
	internal class ResourceFile
	{
		public ResourceFile(string displayName, string path, string resourceObjectName)
		{
			DisplayName = displayName;
			Path = path;
			ResourceObjectName = resourceObjectName;
		}

		public string DisplayName { get; }
		public string Path { get; }
		public string ResourceObjectName { get; }

		public override string ToString()
		{
			return DisplayName;
		}
	}

	internal static class DteProjectResourceHelper
	{
		private static string GetProjectDirectory(Project project)
		{
			if (string.IsNullOrEmpty(project?.FullName))
				return "";
			Debug.Assert(project.FullName != null, "project.FullName != null");
			var path = Path.GetDirectoryName(project.FullName);
			if (path == null)
				throw new InvalidOperationException($"Path not found: {project.FullName}");
			return Path.GetFullPath(path);
		}

		private static string EnsureBackslash(string filePath)
		{
			if (!string.IsNullOrEmpty(filePath) && !filePath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
				return filePath + Path.DirectorySeparatorChar;
			return filePath;
		}

		private static string GetRelativeDirectory(string fileName, string rootDirectory)
		{
			if (string.IsNullOrEmpty(fileName))
				return string.Empty;
			if (string.IsNullOrEmpty(rootDirectory))
				return fileName;
			rootDirectory = EnsureBackslash(rootDirectory);

			return fileName.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase) ? fileName.Substring(rootDirectory.Length) : fileName;
		}

		public static IEnumerable<ResourceFile> GetResourceFiles(Project project)
		{
			try
			{
				var ls = new List<ResourceFile>();

				// Find all resources in project (including linked)
				foreach (var pi in project.ProjectItems.Cast<ProjectItem>())
				{
					if (pi.FileCount == 0) continue;
					var filename = pi.FileNames[0];
					if (!filename.EndsWith(".resx")) continue;

					var resxname = Path.GetFileNameWithoutExtension(pi.FileNames[0]);
					var pdir = GetProjectDirectory(project);

					if (filename.StartsWith(GetProjectDirectory(project)))
						ls.Add(new ResourceFile(GetRelativeDirectory(filename, pdir), filename, resxname));
				}
				return ls;
			}
			catch (Exception e)
			{
				Trace.TraceError("Error: {0}", e.Message);
				return Enumerable.Empty<ResourceFile>();
			}
		}
	}
}