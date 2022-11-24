// See https://aka.ms/new-console-template for more information
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

internal class Program
{
	private static void Main(string[] args)
	{
		Console.WriteLine("Hello, World!");
		[DllImport("user32.dll")]
		static extern IntPtr SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); //ShowWindow needs an IntPtr

		static void FocusProcess()
		{
			IntPtr hWnd; //change this to IntPtr
			Process[] processRunning = Process.GetProcesses();
			foreach (Process pr in processRunning)
			{
				if (pr.ProcessName == "notepad")
				{
					hWnd = pr.MainWindowHandle; //use it as IntPtr not int
					ShowWindow(hWnd, 3);
					SetForegroundWindow(hWnd); //set to topmost
				}
			}
		}
		var done = false;
		var continuousMode = false;

		while (
			!done
		)
		{

			Thread.Sleep(1000);
			/*
			find processes whose parent is GalaxyClient but aren't GalaxyClient or galaxyclient related themselves. then, focus on the first one.
			*/
			//get all processes
			Process[] processes = Process.GetProcesses();
			//Get ones whose parents are galaxyclient
			IEnumerable<Process> galaxyProcesses = processes.Where(p => p.ProcessName == "GalaxyClient");
			//Get ones whose parents are galaxyclient and whose names do not match "galaxyclient" or "GalaxyClient"
			var yourGame = galaxyProcesses.Where(p =>
				// p.ProcessName
				!Regex.IsMatch(p.ProcessName, "^(?!GalaxyClient|galaxyclient).*$")
			);
			//Get the first one
			var yourGameProcess = yourGame.FirstOrDefault();
			//Focus on it
			if (
				yourGameProcess?.MainWindowHandle != null
			)
			{
				SetForegroundWindow(yourGameProcess.MainWindowHandle);
				ShowWindow(yourGameProcess.MainWindowHandle, 3);
			}

			done = true && !continuousMode;
		}
	}
}