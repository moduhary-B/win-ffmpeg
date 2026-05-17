using System;
using System.IO;
using System.Windows.Forms;
using WinFfmpeg.Forms;
using WinFfmpeg.Services;

namespace WinFfmpeg
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0 && args[0] == "/register")
            {
                var ok = ShellRegistrar.Register();
                MessageBox.Show(ok
                    ? "Готово!\n\nКонвертировать медиафайлы:\n  Правый клик → Открыть с помощью → WinFfmpeg"
                    : "Ошибка регистрации. Запустите от имени администратора.",
                    "WinFfmpeg", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                return;
            }

            if (args.Length > 0 && args[0] == "/unregister")
            {
                var ok = ShellRegistrar.Unregister();
                MessageBox.Show(ok ? "Удалено из «Открыть с помощью»." : "Ошибка. Запустите от имени администратора.",
                    "WinFfmpeg", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                return;
            }

            string filePath = null;
            if (args.Length > 0)
            {
                filePath = string.Join(" ", args).Trim('"', ' ');
            }

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath) && !FileTypeDetector.IsMediaFile(filePath))
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Выберите медиафайл для конвертации";
                    ofd.Filter = "Медиа файлы|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm;*.m4v;*.3gp;*.mp3;*.wav;*.aac;*.flac;*.ogg;*.wma;*.m4a;*.opus;*.gif;*.png;*.jpg;*.jpeg;*.bmp;*.webp;*.tiff|Все файлы|*.*";
                    if (ofd.ShowDialog() != DialogResult.OK)
                        return;
                    filePath = ofd.FileName;
                }
            }
            else if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Title = "Выберите файл для конвертации";
                    ofd.Filter = "Медиа файлы|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm;*.m4v;*.3gp;*.mp3;*.wav;*.aac;*.flac;*.ogg;*.wma;*.m4a;*.opus;*.gif;*.png;*.jpg;*.jpeg;*.bmp;*.webp;*.tiff|Все файлы|*.*";
                    if (ofd.ShowDialog() != DialogResult.OK)
                        return;
                    filePath = ofd.FileName;
                }
            }

            Application.Run(new MainForm(filePath));
        }
    }
}
