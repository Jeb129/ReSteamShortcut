using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;

namespace ReSteamShortcut
{
    public class Program
    {
        static string SteamPath = @"C:\Program Files (x86)\Steam\steam.exe";
        static readonly string Desktop = @"C:\Users\" + Environment.UserName + @"\Desktop\";
        static void Main()
        {
            UserInteraction();
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
        static void UserInteraction()
        {
            List<SteamLinkDetails> links = GetSteamLinks();
            if (links.Count == 0)
            {
                Console.WriteLine("Before using this app, you have to create Steam shortcuts in your desktop folder");
                return;
            }
            SetupSteamPath();
            foreach (SteamLinkDetails link in links)
            {
                Console.WriteLine($"Creating shortcut for {link.Name}...");
                System.IO.File.Delete(link.FilePath);
                CreateShortcut(link);
            }
            SHChangeNotify(0x8000000, 0x1000, IntPtr.Zero, IntPtr.Zero);

            Console.WriteLine();
            Console.WriteLine(
                $"Done. Created {links.Count} {(links.Count == 1 ? "shortcut" : "shortcuts")}\n"

                + "\nCreated by Jeb129\n"
                + "GitHub: https://github.com/Jeb129"
            );
        }
        static void SetupSteamPath()
        {
            while (!System.IO.File.Exists(SteamPath))
            {
                Console.WriteLine(
                    $"Could't find \"steam.exe\" in {SteamPath.Replace(@"\steam.exe", "")}" +
                    "\nWrite path to Steam installation folder:");

                SteamPath = Console.ReadLine() + @"\steam.exe";
                Console.WriteLine();
            }
        }
        static SteamLinkDetails? GetAppArgs(string path)
        {
            string[] fileDetails = System.IO.File.ReadLines(path).ToArray();
            if (path.Split('.')[1] != "url" 
                || fileDetails.Length < 7 && !Regex.IsMatch(fileDetails[5], "steam:")) 
                return null;

            return new SteamLinkDetails 
            { 
                FilePath = path, 
                IconPath = fileDetails[6].Replace("IconFile=", ""),
                Name = path.Replace(Desktop, "").Replace(".url", ""),
                GameURL = fileDetails[5].Replace("URL=", "")
            };
        }
        static List<SteamLinkDetails> GetSteamLinks()
        {
            string[] filePaths = Directory.GetFiles(Desktop);
            List<SteamLinkDetails> list = new List<SteamLinkDetails>();
            foreach (string filepath in filePaths)
            {
                SteamLinkDetails? linkDetails = GetAppArgs(filepath);
                if (linkDetails != null)
                    list.Add((SteamLinkDetails)linkDetails);
            }
            return list;
        }
        static void CreateShortcut(SteamLinkDetails args)
        {
            IWshShortcut sc = (IWshShortcut)new WshShell().CreateShortcut(Desktop + args.Name + ".lnk");
            sc.TargetPath = SteamPath;
            sc.WorkingDirectory = SteamPath.Replace(@"\steam.exe", "");
            sc.Arguments = args.GameURL;
            sc.IconLocation = args.IconPath == "" ? SteamPath: args.IconPath;
            sc.Save();
        }

        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);
    }
    public struct SteamLinkDetails
    {
        public string FilePath;
        public string IconPath;
        public string Name;
        public string GameURL;
    }
}
