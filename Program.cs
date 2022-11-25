// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
/// <summary>
/// A utility class to determine a process parent. Credit https://stackoverflow.com/questions/394816/how-to-get-parent-process-in-net-in-managed-way
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ParentProcessUtilities
{

	// These members must match PROCESS_BASIC_INFORMATION
	internal IntPtr Reserved1;
	internal IntPtr PebBaseAddress;
	internal IntPtr Reserved2_0;
	internal IntPtr Reserved2_1;
	internal IntPtr UniqueProcessId;
	internal IntPtr InheritedFromUniqueProcessId;

	[DllImport("ntdll.dll")]
	private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

	/// <summary>
	/// Gets the parent process of the current process.
	/// </summary>
	/// <returns>An instance of the Process class.</returns>
	public static Process GetParentProcess()
	{
		return GetParentProcess(Process.GetCurrentProcess().Handle);
	}

	/// <summary>
	/// Gets the parent process of specified process.
	/// </summary>
	/// <param name="id">The process id.</param>
	/// <returns>An instance of the Process class.</returns>
	public static Process GetParentProcess(int id)
	{
		Process process = Process.GetProcessById(id);
		return GetParentProcess(process.Handle);
	}

	/// <summary>
	/// Gets the parent process of a specified process.
	/// </summary>
	/// <param name="handle">The process handle.</param>
	/// <returns>An instance of the Process class.</returns>
	public static Process GetParentProcess(IntPtr handle)
	{
		ParentProcessUtilities pbi = new ParentProcessUtilities();
		int returnLength;
		int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
		if (status != 0)
			throw new Win32Exception(status);

		try
		{
			return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
		}
		catch (ArgumentException)
		{
			// not found
			return null;
		}
	}
}
// and performance coutner

internal class Program
{
	private static void Main(string[] args)
	{
		Console.WriteLine("Hello, World!");
		[DllImport("user32.dll")]
		static extern IntPtr SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")]
		static extern IntPtr BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); //ShowWindow needs an IntPtr

		static void FocusProcess(IntPtr handle)
		{
			ShowWindow(handle, 9); //9 = SW_RESTORE
			SetForegroundWindow(handle);
			BringWindowToTop(handle);
		}
		var done = false;
		var continuousMode = false;

		while (
			!done
		)
		{

			// Thread.Sleep(1000);
			/*
			find processes whose parent is GalaxyClient but aren't GalaxyClient or galaxyclient related themselves. then, focus on the first one.
			*/
			//get all processes
			Process[] processes = Process.GetProcesses();
			//Get ones whose parents are galaxyclient
			IEnumerable<Process> galaxyProcesses = processes
			.Where(p =>
			{
				//get parent
				try
				{
					var parent = ParentProcessUtilities.GetParentProcess(p.Id);
					//if parent is null, it's probably a system process, so we don't care about it
					if (parent == null)
					{
						return false;
					}
					//if parent is galaxyclient, we care about it
					if (Regex.Match(parent.ProcessName, "galaxyclient", RegexOptions.IgnoreCase).Success)
					{
						return true;
					}
				}
				catch
				{
					return false;
				}
				return false;
			});
			//Get ones whose parents are galaxyclient and whose names do not match "galaxyclient" or "GalaxyClient"
			var yourGame = galaxyProcesses.Where(p =>
				// p.ProcessName
				!Regex.IsMatch(p.ProcessName, "galaxy|helper|client", RegexOptions.IgnoreCase)
			);
			//Get the first one
			var yourGameProcess = yourGame.FirstOrDefault();
			//Focus on it
			if (
				yourGameProcess?.MainWindowHandle != null
			)
			{
				FocusProcess(yourGameProcess.MainWindowHandle);
			}

			done = true && !continuousMode;
		}
	}
}