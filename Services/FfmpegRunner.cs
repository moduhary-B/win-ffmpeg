using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WinFfmpeg.Services
{
    public class FfmpegProgressEventArgs : EventArgs
    {
        public double Percent { get; set; }
        public string TimeProcessed { get; set; }
        public string Speed { get; set; }
        public string Bitrate { get; set; }
        public string RawLine { get; set; }
    }

    public class FfmpegRunner
    {
        private Process _process;
        private double _totalDurationSeconds;
        private bool _isRunning;
        private string _ffmpegPath;

        public event EventHandler<FfmpegProgressEventArgs> ProgressChanged;
        public event EventHandler<string> ErrorOccurred;
        public event EventHandler Completed;
        public event EventHandler Cancelled;

        public bool IsRunning => _isRunning;

        public void Run(string ffmpegPath, string arguments)
        {
            _isRunning = true;
            _totalDurationSeconds = 0;
            _ffmpegPath = ffmpegPath;

            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _process.ErrorDataReceived += OnErrorDataReceived;
            _process.Exited += OnProcessExited;

            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
        }

        public void Cancel()
        {
            if (_process != null && !_process.HasExited)
            {
                try
                {
                    _process.Kill();
                    _isRunning = false;
                    Cancelled?.Invoke(this, EventArgs.Empty);
                }
                catch { }
            }
        }

        public void SetTotalDuration(double seconds)
        {
            _totalDurationSeconds = seconds;
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            var line = e.Data;

            var durationMatch = Regex.Match(line, @"Duration:\s*(\d+):(\d+):(\d+)\.(\d+)");
            if (durationMatch.Success && _totalDurationSeconds <= 0)
            {
                var h = double.Parse(durationMatch.Groups[1].Value);
                var m = double.Parse(durationMatch.Groups[2].Value);
                var s = double.Parse(durationMatch.Groups[3].Value);
                var ms = double.Parse(durationMatch.Groups[4].Value) / 100.0;
                _totalDurationSeconds = h * 3600 + m * 60 + s + ms;
                return;
            }

            var timeMatch = Regex.Match(line, @"time=(\d+):(\d+):(\d+)\.(\d+)");
            if (timeMatch.Success && _totalDurationSeconds > 0)
            {
                var h = double.Parse(timeMatch.Groups[1].Value);
                var m = double.Parse(timeMatch.Groups[2].Value);
                var s = double.Parse(timeMatch.Groups[3].Value);
                var ms = double.Parse(timeMatch.Groups[4].Value) / 100.0;
                var currentSeconds = h * 3600 + m * 60 + s + ms;

                var percent = Math.Min(100.0, (currentSeconds / _totalDurationSeconds) * 100.0);

                var speedMatch = Regex.Match(line, @"speed=\s*([\d.]+)x");
                var bitrateMatch = Regex.Match(line, @"bitrate=\s*([\d.]+\w+)");

                ProgressChanged?.Invoke(this, new FfmpegProgressEventArgs
                {
                    Percent = percent,
                    TimeProcessed = $"{timeMatch.Groups[1].Value}:{timeMatch.Groups[2].Value}:{timeMatch.Groups[3].Value}",
                    Speed = speedMatch.Success ? speedMatch.Groups[1].Value + "x" : "",
                    Bitrate = bitrateMatch.Success ? bitrateMatch.Groups[1].Value : "",
                    RawLine = line
                });
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            _isRunning = false;
            if (_process?.ExitCode == 0)
                Completed?.Invoke(this, EventArgs.Empty);
            else
                ErrorOccurred?.Invoke(this, $"ffmpeg exited with code {_process?.ExitCode ?? -1}");
        }
    }
}
