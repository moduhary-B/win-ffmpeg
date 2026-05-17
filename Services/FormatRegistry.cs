using System.Collections.Generic;
using System.Linq;
using WinFfmpeg.Models;

namespace WinFfmpeg.Services
{
    public static class FormatRegistry
    {
        private static readonly List<FormatInfo> _formats = new List<FormatInfo>
        {
            new FormatInfo("mp4", "MP4", FormatCategory.Video, true, "libx264", "aac", "-movflags +faststart", "MPEG-4 Part 14"),
            new FormatInfo("avi", "AVI", FormatCategory.Video, true, "libx264", "mp3", "", "Audio Video Interleave"),
            new FormatInfo("mkv", "MKV", FormatCategory.Video, true, "libx264", "aac", "", "Matroska Video"),
            new FormatInfo("mov", "MOV", FormatCategory.Video, true, "libx264", "aac", "-movflags +faststart", "QuickTime Movie"),
            new FormatInfo("webm", "WEBM", FormatCategory.Video, true, "libvpx-vp9", "libvorbis", "-b:v 1M", "WebM Video"),
            new FormatInfo("wmv", "WMV", FormatCategory.Video, true, "wmv2", "wmav2", "", "Windows Media Video"),
            new FormatInfo("flv", "FLV", FormatCategory.Video, false, "flv1", "mp3", "", "Flash Video"),
            new FormatInfo("m4v", "M4V", FormatCategory.Video, false, "libx264", "aac", "-movflags +faststart", "iTunes Video"),
            new FormatInfo("3gp", "3GP", FormatCategory.Video, false, "libx264", "aac", "-s 320x240", "3GPP Multimedia"),
            new FormatInfo("ts", "TS", FormatCategory.Video, false, "libx264", "aac", "", "MPEG Transport Stream"),
            new FormatInfo("mpeg", "MPEG", FormatCategory.Video, false, "mpeg2video", "mp2", "-q:v 5", "MPEG-1 Video"),
            new FormatInfo("mpg", "MPG", FormatCategory.Video, false, "mpeg2video", "mp2", "-q:v 5", "MPEG-2 Video"),
            new FormatInfo("ogv", "OGV", FormatCategory.Video, false, "libtheora", "libvorbis", "", "Ogg Video"),
            new FormatInfo("vob", "VOB", FormatCategory.Video, false, "mpeg2video", "mp2", "-q:v 5", "DVD Video Object"),
            new FormatInfo("asf", "ASF", FormatCategory.Video, false, "wmv2", "wmav2", "", "Advanced Systems Format"),

            new FormatInfo("mp3", "MP3", FormatCategory.Audio, true, null, "libmp3lame", "-q:a 2", "MPEG-1 Audio Layer III"),
            new FormatInfo("wav", "WAV", FormatCategory.Audio, true, null, "pcm_s16le", "", "Waveform Audio"),
            new FormatInfo("aac", "AAC", FormatCategory.Audio, true, null, "aac", "-b:a 192k", "Advanced Audio Coding"),
            new FormatInfo("flac", "FLAC", FormatCategory.Audio, true, null, "flac", "", "Free Lossless Audio Codec"),
            new FormatInfo("ogg", "OGG", FormatCategory.Audio, true, null, "libvorbis", "-q:a 5", "Ogg Vorbis"),
            new FormatInfo("wma", "WMA", FormatCategory.Audio, false, null, "wmav2", "-b:a 192k", "Windows Media Audio"),
            new FormatInfo("m4a", "M4A", FormatCategory.Audio, false, null, "aac", "-b:a 192k", "MPEG-4 Audio"),
            new FormatInfo("opus", "OPUS", FormatCategory.Audio, false, null, "libopus", "-b:a 128k", "Opus Audio"),
            new FormatInfo("ac3", "AC3", FormatCategory.Audio, false, null, "ac3", "-b:a 192k", "Dolby Digital"),
            new FormatInfo("aiff", "AIFF", FormatCategory.Audio, false, null, "pcm_s16be", "", "Audio Interchange File Format"),
            new FormatInfo("alac", "ALAC", FormatCategory.Audio, false, null, "alac", "", "Apple Lossless Audio"),

            new FormatInfo("gif", "GIF", FormatCategory.Image, true, "gif", null, "-vf \"fps=15,scale=640:-1:flags=lanczos\"", "Graphics Interchange Format"),
            new FormatInfo("png", "PNG", FormatCategory.Image, true, "png", null, "", "Portable Network Graphics"),
            new FormatInfo("jpg", "JPG", FormatCategory.Image, true, "mjpeg", null, "-q:v 2", "JPEG Image"),
            new FormatInfo("bmp", "BMP", FormatCategory.Image, false, "bmp", null, "", "Bitmap Image"),
            new FormatInfo("webp", "WEBP", FormatCategory.Image, false, "libwebp", null, "-q:v 75", "WebP Image"),
            new FormatInfo("tiff", "TIFF", FormatCategory.Image, false, "tiff", null, "", "Tagged Image File Format"),
            new FormatInfo("ico", "ICO", FormatCategory.Image, false, "png", null, "-vf \"scale=256:256\"", "Icon File"),
        };

        public static List<FormatInfo> GetFormatsByCategory(FormatCategory category)
        {
            return _formats.Where(f => f.Category == category).ToList();
        }

        public static List<FormatInfo> GetPopularByCategory(FormatCategory category)
        {
            return _formats.Where(f => f.Category == category && f.IsPopular).ToList();
        }

        public static List<FormatInfo> GetNonPopularByCategory(FormatCategory category)
        {
            return _formats.Where(f => f.Category == category && !f.IsPopular).ToList();
        }

        public static FormatInfo GetByExtension(string extension)
        {
            var ext = extension.TrimStart('.').ToLowerInvariant();
            return _formats.FirstOrDefault(f => f.Extension == ext);
        }

        public static List<FormatInfo> SearchFormats(string query, FormatCategory? category = null)
        {
            var q = query.ToLowerInvariant().TrimStart('.');
            var results = _formats.Where(f =>
                f.Extension.Contains(q) ||
                f.DisplayName.ToLowerInvariant().Contains(q) ||
                (f.Description?.ToLowerInvariant().Contains(q) ?? false));

            if (category.HasValue)
                results = results.Where(f => f.Category == category.Value);

            return results.ToList();
        }

        public static List<FormatInfo> GetAllFormats()
        {
            return _formats.ToList();
        }
    }
}
