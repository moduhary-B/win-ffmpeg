using System;

namespace WinFfmpeg.Models
{
    public class ConversionTask
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public FormatInfo TargetFormat { get; set; }
        public string CustomParams { get; set; }
        public string CustomFormat { get; set; }
        public bool IsCustomFormat { get; set; }

        public ConversionTask(string inputPath, FormatInfo targetFormat = null)
        {
            InputPath = inputPath;
            TargetFormat = targetFormat;
            CustomParams = "";
            CustomFormat = "";
            IsCustomFormat = false;
        }

        public string GetOutputExtension()
        {
            if (IsCustomFormat && !string.IsNullOrWhiteSpace(CustomFormat))
                return CustomFormat.TrimStart('.');
            return TargetFormat?.Extension ?? "mp4";
        }

        public string GenerateOutputPath()
        {
            var dir = System.IO.Path.GetDirectoryName(InputPath);
            var nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(InputPath);
            var ext = "." + GetOutputExtension();
            var outputPath = System.IO.Path.Combine(dir, nameWithoutExt + ext);
            var counter = 1;
            while (System.IO.File.Exists(outputPath))
            {
                outputPath = System.IO.Path.Combine(dir, nameWithoutExt + "_" + counter + ext);
                counter++;
            }
            return outputPath;
        }
    }
}
