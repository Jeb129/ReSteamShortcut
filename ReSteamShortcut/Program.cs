using IWshRuntimeLibrary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace ReSteamShortcut
{
    internal class Program
    {
        
        static string SteamDir = "";
        static List<string[]> Apps = new List<string[]>();
        static string Desktop = @"C:\Users\" + Environment.UserName + @"\Desktop";
        static void Main(string[] args)
        {
            Console.WriteLine(" You have create steam's shortcuts to desktop before using this!");
            SteamDir = GetSteamLoc();
            GetApps();
            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
            foreach (string[] App in Apps)
                ShortcutCreate(App);
            Console.WriteLine($" Done. Created {Apps.Count} shortcuts");
            Console.WriteLine("\n Created by Jeb129\n GitHub: https://github.com/Jeb129/SteamShortCutRe");
            Console.WriteLine("\n Press any key to close...");
            Console.ReadKey();
        }
        static string GetSteamLoc()
        {
            do
            {
                Console.WriteLine("\n Steam.exe directory:");
                string a = Console.ReadLine();
                if (System.IO.File.Exists(a + @"\steam.exe"))
                    return a;
                else
                    Console.WriteLine(" Steam.exe not found!");
            }
            while (true);
        }
        static string[] GetAppArgs(string path)
        {
            string[] Source = System.IO.File.ReadLines(path).ToArray();
            if (Source.Length < 7 && !Regex.IsMatch(Source[5],"steam:"))
                return null;
            string Name = path.Replace(Desktop, "").Replace(".url", "");
            string URL = Source[5].Replace("URL=","");
            string IconSource = Source[6].Replace("IconFile=","");
            return new string[] {Name, URL, IconSource};
        }
        static void GetApps()
        {
            string[] Files = Directory.GetFiles(Desktop);
            List<string> URLs = new List<string>();
            foreach (string file in Files)
            {
                if (file.Split('.')[1] != "url") 
                    continue;
                string[] LnkInfo = GetAppArgs(file);
                if (LnkInfo == null)
                    continue;
                Apps.Add(LnkInfo);
                System.IO.File.Delete(file);
            }
        }
        static void ShortcutCreate(string[] args)
        {
            WshShell shell = new WshShell();
            IWshShortcut sc = (IWshShortcut)shell.CreateShortcut(Desktop + args[0] + ".lnk");
            sc.TargetPath = SteamDir + @"\steam.exe";
            sc.WorkingDirectory = SteamDir;
            sc.Arguments = args[1];
            sc.IconLocation = args[2].Length == 0 ? SteamDir + @"\steam.exe": args[2];
            sc.Save();
        }
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);
    }
}
