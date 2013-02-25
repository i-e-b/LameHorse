using System;
using System.IO;
using System.Reflection;

namespace LameHorse.LAME.Interop
{
	public class AddLocalPathForLinuxLibrarySearch
	{
		/// <summary>
		/// You MUST call this if the shared lib is not in the normal execution path.
		/// Returns true if running in a Posix environment (Linux or Mac)
		/// </summary>
		public static bool Setup()
		{
			var p = (int)Environment.OSVersion.Platform;
			var posix = (p == 4) || (p == 6) || (p == 128);
			if (!posix) return false;

			var path = "";
			path += "/usr/lib"; // for debian.

			try
			{
				path += Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				path += ";";
			}
			catch
			{
				Console.WriteLine("Adding Executing assembly path failed");
			}

			try
			{
				path += Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
				path += ";";
			}
			catch
			{
				Console.WriteLine("Adding Calling assembly path failed");
			}

			try
			{
				path += Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			}
			catch
			{
				Console.WriteLine("Adding Executing assembly path failed");
			}

			try
			{
				var oldPath = Environment.GetEnvironmentVariable("PATH");
				if (!string.IsNullOrEmpty(oldPath)) oldPath += ";";
				oldPath += path;
				Environment.SetEnvironmentVariable("PATH", oldPath, EnvironmentVariableTarget.Process);


				var oldLD = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
				if (!string.IsNullOrEmpty(oldLD)) oldLD += ";";
				oldLD += path;
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", oldLD, EnvironmentVariableTarget.Process);
			}
			catch
			{
				Console.WriteLine("Failed to set PATH variable: LAME library may not be found");
			}
			return true;
		}
	}
}
