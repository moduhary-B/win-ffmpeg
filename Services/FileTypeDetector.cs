using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinFfmpeg.Models;

namespace WinFfmpeg.Services
{
    public static class FileTypeDetector
    {
        private static readonly HashSet<string> VideoExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mp4", "avi", "mkv", "mov", "wmv", "flv", "webm", "m4v", "3gp", "ts",
            "mpeg", "mpg", "ogv", "vob", "asf", "mts", "m2ts", "dv", "hevc", "y4m"
        };

        private static readonly HashSet<string> AudioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mp3", "wav", "aac", "flac", "ogg", "wma", "m4a", "opus", "ac3",
            "aiff", "alac", "dts", "ape", "pcm", "raw", "amr", "awb"
        };

        private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "gif", "png", "jpg", "jpeg", "bmp", "webp", "tiff", "tif", "ico",
            "svg", "psd", "raw", "cr2", "nef", "heic", "heif", "avif"
        };

        public static FormatCategory DetectCategory(string filePath)
        {
            var ext = Path.GetExtension(filePath)?.TrimStart('.');
            if (string.IsNullOrEmpty(ext))
                return FormatCategory.Video;

            if (VideoExtensions.Contains(ext))
                return FormatCategory.Video;
            if (AudioExtensions.Contains(ext))
                return FormatCategory.Audio;
            if (ImageExtensions.Contains(ext))
                return FormatCategory.Image;

            return FormatCategory.Video;
        }

        public static bool IsMediaFile(string filePath)
        {
            var ext = Path.GetExtension(filePath)?.TrimStart('.');
            if (string.IsNullOrEmpty(ext))
                return false;
            return VideoExtensions.Contains(ext) || AudioExtensions.Contains(ext) || ImageExtensions.Contains(ext);
        }

        public static List<FormatCategory> GetAvailableTargetCategories(string filePath)
        {
            var source = DetectCategory(filePath);
            var result = new List<FormatCategory> { source };

            if (source == FormatCategory.Video)
            {
                result.Add(FormatCategory.Audio);
                result.Add(FormatCategory.Image);
            }
            else if (source == FormatCategory.Audio)
            {
                result.Add(FormatCategory.Video);
            }
            else if (source == FormatCategory.Image)
            {
                result.Add(FormatCategory.Video);
            }

            return result;
        }
    }
}
