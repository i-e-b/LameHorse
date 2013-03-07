using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Interfaces
{
	public class AddLocalPathForLinuxLibrarySearch
	{
		/// <summary>
		/// You MUST call this if the shared lib is not in the normal execution path.
		/// Returns true if running in a Posix environment (Linux or Mac)
		/// </summary>
		public static bool Setup()
		{
			var p = (int) Environment.OSVersion.Platform;
			var posix = (p == 4) || (p == 6) || (p == 128);

			var pathElements = new HashSet<string>();
			var ldElements = new HashSet<string>();

			Action<string> add = s => { pathElements.Add(s); ldElements.Add(s); };

			foreach (var item in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(':')) { pathElements.Add(item); }
			foreach (var item in (Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? "").Split(':')) { ldElements.Add(item); }

			if (posix)
			{
				add("/usr/lib"); // for debian.
				add(".");
			}

			add(ExecutingAssemblyPath());
			add(CallingAssemblyPath());
			add(EntryAssemblyPath());

			try
			{
				var newPath = string.Join(":", pathElements.Where(e => e != "").Select(e => "\"" + e + "\""));
				//Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
				Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Process);

				var newLD = string.Join(":", ldElements.Where(e => e != "").Select(e => "\"" + e + "\""));
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", newLD, EnvironmentVariableTarget.User);
				Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", newLD, EnvironmentVariableTarget.Process);
			}
			catch
			{
				Console.WriteLine("Failed to set PATH variable: libraries may not be found");
			}
			return posix;
		}

		static string EntryAssemblyPath()
		{
			try
			{
				return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			}
			catch
			{
				Console.WriteLine("Adding Executing assembly path failed");
			}
			return "";
		}

		static string CallingAssemblyPath()
		{
			try
			{
				return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			}
			catch
			{
				Console.WriteLine("Adding Calling assembly path failed");
			}
			return "";
		}

		static string ExecutingAssemblyPath()
		{
			try
			{
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			}
			catch
			{
				Console.WriteLine("Adding Executing assembly path failed");
			}
			return "";
		}
	}
}
