using System.Text;
using WinFfmpeg.Models;

namespace WinFfmpeg.Services
{
    public static class FfmpegBuilder
    {
        public static string BuildArguments(ConversionTask task)
        {
            var sb = new StringBuilder();

            sb.Append($"-i \"{task.InputPath}\"");

            if (task.IsCustomFormat && !string.IsNullOrWhiteSpace(task.CustomParams))
            {
                sb.Append($" {task.CustomParams.Trim()}");
            }
            else if (task.TargetFormat != null)
            {
                var format = task.TargetFormat;

                if (format.Category == FormatCategory.Audio)
                {
                    if (format.DefaultAudioCodec != null)
                        sb.Append($" -c:a {format.DefaultAudioCodec}");
                    if (!string.IsNullOrWhiteSpace(format.ExtraParams))
                        sb.Append($" {format.ExtraParams}");
                    sb.Append(" -vn");
                }
                else if (format.Category == FormatCategory.Image)
                {
                    if (format.DefaultCodec != null)
                        sb.Append($" -c:v {format.DefaultCodec}");
                    if (!string.IsNullOrWhiteSpace(format.ExtraParams))
                        sb.Append($" {format.ExtraParams}");
                    if (format.Extension != "gif")
                        sb.Append(" -frames:v 1");
                    sb.Append(" -an");
                }
                else
                {
                    if (format.DefaultCodec != null)
                        sb.Append($" -c:v {format.DefaultCodec}");
                    if (format.DefaultAudioCodec != null)
                        sb.Append($" -c:a {format.DefaultAudioCodec}");
                    if (!string.IsNullOrWhiteSpace(format.ExtraParams))
                        sb.Append($" {format.ExtraParams}");
                }

                if (!string.IsNullOrWhiteSpace(task.CustomParams))
                    sb.Append($" {task.CustomParams.Trim()}");
            }

            sb.Append(" -y");

            var outputPath = task.OutputPath ?? task.GenerateOutputPath();
            sb.Append($" \"{outputPath}\"");

            return sb.ToString();
        }

        public static string BuildPreviewCommand(ConversionTask task)
        {
            return "ffmpeg " + BuildArguments(task);
        }
    }
}
