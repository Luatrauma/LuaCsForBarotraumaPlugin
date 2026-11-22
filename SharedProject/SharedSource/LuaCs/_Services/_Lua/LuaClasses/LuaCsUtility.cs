using Barotrauma.Items.Components;
using Barotrauma.Networking;
using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using Barotrauma.LuaCs;

namespace Barotrauma
{
    partial class LuaCsFile
    {
        private static string GetFullPath(string path) => System.IO.Path.GetFullPath(path).CleanUpPath().Replace('\\', '/');

        private static bool IsInDirectory(string fullPath, string directory, bool includeSelf = true)
        {
            string dir = GetFullPath(directory).TrimEnd('/');
            return (includeSelf && fullPath.Equals(dir, StringComparison.OrdinalIgnoreCase))
                || fullPath.StartsWith(dir + "/", StringComparison.OrdinalIgnoreCase);
        }

        private static string SaveFolder => SaveUtil.DefaultSaveFolder;

        public static bool CanReadFromPath(string path)
        {
            path = GetFullPath(path);

            if (IsInDirectory(path, SaveFolder))
                return true;

            if (IsInDirectory(path, ContentPackage.LocalModsDir))
                return true;

            if (IsInDirectory(path, ContentPackage.WorkshopModsDir))
                return true;

#if CLIENT
            if (IsInDirectory(path, ModReceiver.DownloadFolder))
                return true;
#endif

            if (IsInDirectory(path, "."))
                return true;

            return false;
        }

        private static readonly string[] ProhibitedWriteExtensions =
        {
            ".dll", ".cs",
            ".exe", ".com", ".scr", ".msi", ".bat", ".cmd", ".ps1", ".vbs", ".js", ".hta", ".lnk", ".url",
            ".sh", ".so", ".dylib",
        };

        public static bool CanWriteToPath(string path)
        {
            path = GetFullPath(path);

            if (path.Length > 2 && path.IndexOf(':', 2) >= 0)
            {
                return false;
            }

            string extension = System.IO.Path.GetExtension(path.TrimEnd('.', ' '));
            if (ProhibitedWriteExtensions.Any(e => extension.Equals(e, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (IsInDirectory(path, LuaCsSetup.GetLuaCsPackage().Dir))
            {
                return false;
            }

            if (IsInDirectory(path, SaveFolder, includeSelf: false))
                return true;

            if (IsInDirectory(path, ContentPackage.LocalModsDir, includeSelf: false))
                return true;

            if (IsInDirectory(path, ContentPackage.WorkshopModsDir, includeSelf: false))
                return true;

#if CLIENT
            if (IsInDirectory(path, ModReceiver.DownloadFolder, includeSelf: false))
                return true;
#endif

            return false;
        }

        public static bool IsPathAllowedException(string path, bool write = true, LuaCsMessageOrigin origin = LuaCsMessageOrigin.Unknown)
        {
            if (write)
            {
                if (CanWriteToPath(path))
                {
                    return true;
                }
                else
                {
                    throw new Exception("File access to \"" + path + "\" not allowed.");
                }
            }
            else
            {
                if (CanReadFromPath(path))
                {
                    return true;
                }
                else
                {
                    throw new Exception("File access to \"" + path + "\" not allowed.");
                }
            }
        }

        public static bool IsPathAllowedLuaException(string path, bool write = true) =>
            IsPathAllowedException(path, write, LuaCsMessageOrigin.LuaMod);
        public static bool IsPathAllowedCsException(string path, bool write = true) =>
            IsPathAllowedException(path, write, LuaCsMessageOrigin.CSharpMod);

        public static string Read(string path)
        {
            if (!IsPathAllowedException(path, false))
                return "";

            return File.ReadAllText(path);
        }

        public static void Write(string path, string text)
        {
            if (!IsPathAllowedException(path))
                return;

            File.WriteAllText(path, text);
        }

        public static void Delete(string path)
        {
            if (!IsPathAllowedException(path))
                return;

            File.Delete(path);
        }

        public static void DeleteDirectory(string path)
        {
            if (!IsPathAllowedException(path))
                return;

            Directory.Delete(path, true);
        }

        public static void Move(string path, string destination)
        {
            if (!IsPathAllowedException(path))
                return;

            if (!IsPathAllowedException(destination))
                return;

            File.Move(path, destination, true);
        }

        public static FileStream OpenRead(string path)
        {
            if (!IsPathAllowedException(path, false))
                return null;

            return File.Open(path, FileMode.Open, FileAccess.Read);
        }
        public static FileStream OpenWrite(string path)
        {
            if (!IsPathAllowedException(path))
                return null;

            if (File.Exists(path)) return File.Open(path, FileMode.Truncate, FileAccess.Write);
            else return File.Open(path, FileMode.Create, FileAccess.Write);
        }

        public static bool Exists(string path)
        {
            if (!IsPathAllowedException(path, false))
                return false;

            return File.Exists(path);
        }

        public static bool CreateDirectory(string path)
        {
            if (!IsPathAllowedException(path))
                return false;

            Directory.CreateDirectory(path);

            return true;
        }

        public static bool DirectoryExists(string path)
        {
            if (!IsPathAllowedException(path, false))
                return false;

            return Directory.Exists(path);
        }

        public static string[] GetFiles(string path)
        {
            if (!IsPathAllowedException(path, false))
                return null;

            return Directory.GetFiles(path);
        }

        public static string[] GetDirectories(string path)
        {
            if (!IsPathAllowedException(path, false))
                return new string[] { };

            return Directory.GetDirectories(path);
        }

        public static string[] DirSearch(string sDir)
        {
            if (!IsPathAllowedException(sDir, false))
                return new string[] { };

            List<string> files = new List<string>();

            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }

                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        files.Add(f);
                    }
                    DirSearch(d);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }

            return files.ToArray();
        }
    }
}
