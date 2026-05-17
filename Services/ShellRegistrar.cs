using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WinFfmpeg.Services
{
    public static class ShellRegistrar
    {
        private const string AppName = "WinFfmpeg.exe";
        private const string AppKey = "WinFfmpeg.exe";

        private static readonly string[] VideoExtensions =
        {
            "mp4", "avi", "mkv", "mov", "wmv", "flv", "webm", "m4v", "3gp", "ts",
            "mpeg", "mpg", "ogv", "vob", "asf", "mts", "m2ts", "dv"
        };

        private static readonly string[] AudioExtensions =
        {
            "mp3", "wav", "aac", "flac", "ogg", "wma", "m4a", "opus", "ac3",
            "aiff", "amr", "ape"
        };

        private static readonly string[] ImageExtensions =
        {
            "gif", "png", "jpg", "jpeg", "bmp", "webp", "tiff", "tif", "ico",
            "heic", "heif", "avif"
        };

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

        private static string GetAppPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppName);
        }

        private static IEnumerable<string> GetAllExtensions()
        {
            foreach (var ext in VideoExtensions) yield return ext;
            foreach (var ext in AudioExtensions) yield return ext;
            foreach (var ext in ImageExtensions) yield return ext;
        }

        public static bool Register()
        {
            var appPath = GetAppPath();
            if (!File.Exists(appPath))
                return false;

            try
            {
                RegisterApplication(appPath);

                foreach (var ext in GetAllExtensions())
                {
                    RegisterOpenWith(ext);
                }

                RefreshShell();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Unregister()
        {
            try
            {
                UnregisterApplication();

                foreach (var ext in GetAllExtensions())
                {
                    UnregisterOpenWith(ext);
                }

                RefreshShell();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void RegisterApplication(string appPath)
        {
            using (var key = Registry.ClassesRoot.CreateSubKey($@"Applications\{AppKey}\shell\open\command"))
            {
                key.SetValue(null, $"\"{appPath}\" \"%1\"");
            }

            using (var key = Registry.ClassesRoot.CreateSubKey($@"Applications\{AppKey}\shell\open"))
            {
                key.SetValue("FriendlyAppName", "Конвертировать");
            }

            using (var key = Registry.ClassesRoot.CreateSubKey($@"Applications\{AppKey}\SupportedTypes"))
            {
                foreach (var ext in GetAllExtensions())
                {
                    key.SetValue($".{ext}", "");
                }
            }
        }

        private static void UnregisterApplication()
        {
            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree($@"Applications\{AppKey}");
            }
            catch { }
        }

        private static void RegisterOpenWith(string extension)
        {
            using (var key = Registry.ClassesRoot.CreateSubKey($@"SystemFileAssociations\.{extension}\OpenWithList"))
            {
                key.SetValue(AppKey, "");
            }
        }

        private static void UnregisterOpenWith(string extension)
        {
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey(
                    $@"SystemFileAssociations\.{extension}\OpenWithList", true))
                {
                    key?.DeleteValue(AppKey, false);
                }
            }
            catch { }
        }

        private static void RefreshShell()
        {
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool IsRegistered()
        {
            try
            {
                using (var key = Registry.ClassesRoot.OpenSubKey($@"Applications\{AppKey}"))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
