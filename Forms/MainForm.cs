using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinFfmpeg.Models;
using WinFfmpeg.Services;

namespace WinFfmpeg.Forms
{
    public partial class MainForm : Form
    {
        private string _inputPath;
        private FormatCategory _sourceCategory;
        private ConversionTask _task;
        private FfmpegRunner _runner;
        private FormatInfo _selectedFormat;
        private string _ffmpegPath;

        private FlowLayoutPanel flpPopular;
        private Panel pnlAllFormats;
        private FlowLayoutPanel flpAllFormats;
        private Panel pnlToAudio;
        private FlowLayoutPanel flpToAudio;
        private Panel pnlToImage;
        private FlowLayoutPanel flpToImage;
        private Panel pnlCustom;
        private TextBox txtCustomFormat;
        private TextBox txtCustomParams;
        private TextBox txtCommandPreview;
        private Button btnConvert;
        private Button btnCancel;
        private ProgressBar progressBar;
        private Label lblProgress;

        public MainForm(string filePath)
        {
            _inputPath = filePath;
            _sourceCategory = FileTypeDetector.DetectCategory(filePath);
            _task = new ConversionTask(filePath);
            _runner = new FfmpegRunner();
            _ffmpegPath = FindFfmpegPath();

            InitializeComponent();
            LoadFormats();
            UpdateCommandPreview();
        }

        private void InitializeComponent()
        {
            this.Text = "Конвертировать";
            this.Size = new Size(520, 650);
            this.MinimumSize = new Size(420, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.Font = SystemFonts.MessageBoxFont;

            var mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = SystemColors.Window,
                Padding = new Padding(12),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

            var topPanel = new Panel { Dock = DockStyle.Fill, Height = 48 };

            var lblFileName = new Label
            {
                Text = Path.GetFileName(_inputPath),
                Dock = DockStyle.Top,
                Height = 20,
                Font = new Font(this.Font, FontStyle.Bold),
                AutoEllipsis = true
            };
            topPanel.Controls.Add(lblFileName);

            var txtSearch = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 26
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            topPanel.Controls.Add(txtSearch);

            mainTable.Controls.Add(topPanel, 0, 0);

            var scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };

            var contentFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(6)
            };

            var lblPopular = new Label
            {
                Text = GetCategorySectionTitle(_sourceCategory),
                Width = 460,
                Height = 20,
                Font = new Font(this.Font, FontStyle.Bold)
            };
            contentFlow.Controls.Add(lblPopular);

            flpPopular = new FlowLayoutPanel
            {
                Width = 460,
                Height = 42,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            contentFlow.Controls.Add(flpPopular);

            var llAllFormats = new LinkLabel
            {
                Text = "▶ Все " + GetCategoryNamePlural(_sourceCategory).ToLower() + " форматы",
                Width = 460,
                Height = 20
            };
            contentFlow.Controls.Add(llAllFormats);

            pnlAllFormats = new Panel
            {
                Width = 460,
                Visible = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            flpAllFormats = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            pnlAllFormats.Controls.Add(flpAllFormats);
            contentFlow.Controls.Add(pnlAllFormats);

            llAllFormats.LinkClicked += (s, e) =>
            {
                TogglePanel(pnlAllFormats, llAllFormats,
                    "Все " + GetCategoryNamePlural(_sourceCategory).ToLower() + " форматы",
                    "Все " + GetCategoryNamePlural(_sourceCategory).ToLower() + " форматы");
                if (pnlAllFormats.Visible)
                    PopulateFormatButtons(flpAllFormats, FormatRegistry.GetFormatsByCategory(_sourceCategory));
            };

            var crossCategories = FileTypeDetector.GetAvailableTargetCategories(_inputPath)
                .Where(c => c != _sourceCategory).ToList();

            if (crossCategories.Contains(FormatCategory.Audio))
            {
                var llToAudio = new LinkLabel
                {
                    Text = "▶ Конвертировать в аудио",
                    Width = 460,
                    Height = 20
                };
                contentFlow.Controls.Add(llToAudio);

                pnlToAudio = new Panel
                {
                    Width = 460,
                    Visible = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                flpToAudio = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                pnlToAudio.Controls.Add(flpToAudio);
                contentFlow.Controls.Add(pnlToAudio);

                llToAudio.LinkClicked += (s, e) =>
                {
                    TogglePanel(pnlToAudio, llToAudio, "Конвертировать в аудио", "Конвертировать в аудио");
                    if (pnlToAudio.Visible)
                        PopulateFormatButtons(flpToAudio, FormatRegistry.GetFormatsByCategory(FormatCategory.Audio));
                };
            }

            if (crossCategories.Contains(FormatCategory.Image))
            {
                var llToImage = new LinkLabel
                {
                    Text = "▶ Конвертировать в изображение",
                    Width = 460,
                    Height = 20
                };
                contentFlow.Controls.Add(llToImage);

                pnlToImage = new Panel
                {
                    Width = 460,
                    Visible = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                flpToImage = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                pnlToImage.Controls.Add(flpToImage);
                contentFlow.Controls.Add(pnlToImage);

                llToImage.LinkClicked += (s, e) =>
                {
                    TogglePanel(pnlToImage, llToImage, "Конвертировать в изображение", "Конвертировать в изображение");
                    if (pnlToImage.Visible)
                        PopulateFormatButtons(flpToImage, FormatRegistry.GetFormatsByCategory(FormatCategory.Image));
                };
            }

            var llCustom = new LinkLabel
            {
                Text = "▶ Ручной ввод",
                Width = 460,
                Height = 20
            };
            contentFlow.Controls.Add(llCustom);

            pnlCustom = new Panel
            {
                Width = 460,
                Height = 80,
                Visible = false
            };

            var lblCustFormat = new Label
            {
                Text = "Выходной формат:",
                Location = new Point(4, 6),
                Size = new Size(120, 18)
            };
            pnlCustom.Controls.Add(lblCustFormat);

            txtCustomFormat = new TextBox
            {
                Location = new Point(128, 2),
                Size = new Size(100, 24)
            };
            txtCustomFormat.TextChanged += CustomFieldsChanged;
            pnlCustom.Controls.Add(txtCustomFormat);

            var lblCustParams = new Label
            {
                Text = "Кастомные параметры ffmpeg:",
                Location = new Point(4, 34),
                Size = new Size(440, 18)
            };
            pnlCustom.Controls.Add(lblCustParams);

            txtCustomParams = new TextBox
            {
                Location = new Point(4, 54),
                Size = new Size(440, 24)
            };
            txtCustomParams.TextChanged += CustomFieldsChanged;
            pnlCustom.Controls.Add(txtCustomParams);

            contentFlow.Controls.Add(pnlCustom);

            llCustom.LinkClicked += (s, e) =>
            {
                TogglePanel(pnlCustom, llCustom, "Ручной ввод", "Ручной ввод");
            };

            scrollPanel.Controls.Add(contentFlow);
            mainTable.Controls.Add(scrollPanel, 0, 1);

            txtCommandPreview = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BackColor = SystemColors.Window,
                Font = new Font("Consolas", 8.5f),
                ScrollBars = ScrollBars.Vertical
            };
            mainTable.Controls.Add(txtCommandPreview, 0, 2);

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };
            mainTable.Controls.Add(progressBar, 0, 3);

            lblProgress = new Label
            {
                Dock = DockStyle.Fill,
                Text = "",
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainTable.Controls.Add(lblProgress, 0, 3);

            var bottomPanel = new Panel { Dock = DockStyle.Fill, Height = 36 };

            btnConvert = new Button
            {
                Text = "Конвертировать",
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Right
            };
            btnConvert.Click += BtnConvert_Click;

            btnCancel = new Button
            {
                Text = "Отмена",
                Size = new Size(85, 30),
                Anchor = AnchorStyles.Right,
                Enabled = false
            };
            btnCancel.Click += BtnCancel_Click;

            bottomPanel.Controls.Add(btnCancel);
            bottomPanel.Controls.Add(btnConvert);

            btnCancel.Location = new Point(bottomPanel.Width - 90, 3);
            btnConvert.Location = new Point(bottomPanel.Width - 220, 3);

            bottomPanel.Layout += (s, e) =>
            {
                btnCancel.Location = new Point(bottomPanel.Width - 90, 3);
                btnConvert.Location = new Point(bottomPanel.Width - 220, 3);
            };

            mainTable.Controls.Add(bottomPanel, 0, 4);

            this.Controls.Add(mainTable);
        }

        private string GetCategorySectionTitle(FormatCategory cat)
        {
            switch (cat)
            {
                case FormatCategory.Video: return "Популярные видео форматы";
                case FormatCategory.Audio: return "Популярные аудио форматы";
                case FormatCategory.Image: return "Популярные форматы изображений";
                default: return "Популярные форматы";
            }
        }

        private string GetCategoryNamePlural(FormatCategory cat)
        {
            switch (cat)
            {
                case FormatCategory.Video: return "Видео";
                case FormatCategory.Audio: return "Аудио";
                case FormatCategory.Image: return "Изображения";
                default: return "";
            }
        }

        private void LoadFormats()
        {
            var popular = FormatRegistry.GetPopularByCategory(_sourceCategory);
            PopulateFormatButtons(flpPopular, popular);

            var allSource = FormatRegistry.GetFormatsByCategory(_sourceCategory);
            PopulateFormatButtons(flpAllFormats, allSource);

            if (flpToAudio != null)
            {
                var audioFormats = FormatRegistry.GetPopularByCategory(FormatCategory.Audio);
                PopulateFormatButtons(flpToAudio, audioFormats);
            }

            if (flpToImage != null)
            {
                var imageFormats = FormatRegistry.GetPopularByCategory(FormatCategory.Image);
                PopulateFormatButtons(flpToImage, imageFormats);
            }
        }

        private void PopulateFormatButtons(FlowLayoutPanel panel, List<FormatInfo> formats)
        {
            panel.Controls.Clear();
            foreach (var fmt in formats)
            {
                var btn = new Button
                {
                    Text = fmt.DisplayName,
                    Size = new Size(72, 34),
                    Tag = fmt,
                    FlatStyle = FlatStyle.Standard,
                    Margin = new Padding(2)
                };
                btn.Click += FormatButton_Click;
                panel.Controls.Add(btn);
            }
        }

        private void FormatButton_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var fmt = (FormatInfo)btn.Tag;

            _selectedFormat = fmt;
            _task.TargetFormat = fmt;
            _task.IsCustomFormat = false;

            HighlightSelectedButton(btn);
            UpdateCommandPreview();
        }

        private void HighlightSelectedButton(Button selected)
        {
            foreach (var panel in new[] { flpPopular, flpAllFormats, flpToAudio, flpToImage })
            {
                if (panel == null) continue;
                foreach (Control c in panel.Controls)
                {
                    if (c is Button b)
                    {
                        b.BackColor = b == selected ? SystemColors.Highlight : SystemColors.Control;
                        b.ForeColor = b == selected ? SystemColors.HighlightText : SystemColors.ControlText;
                    }
                }
            }
        }

        private void CustomFieldsChanged(object sender, EventArgs e)
        {
            _task.IsCustomFormat = true;
            _task.CustomFormat = txtCustomFormat.Text;
            _task.CustomParams = txtCustomParams.Text;
            if (!string.IsNullOrWhiteSpace(txtCustomFormat.Text))
            {
                var found = FormatRegistry.GetByExtension(txtCustomFormat.Text);
                if (found != null)
                {
                    _task.TargetFormat = found;
                    _task.IsCustomFormat = false;
                }
            }
            UpdateCommandPreview();
        }

        private void UpdateCommandPreview()
        {
            _task.OutputPath = null;
            txtCommandPreview.Text = FfmpegBuilder.BuildPreviewCommand(_task);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            var query = ((TextBox)sender).Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                pnlAllFormats.Visible = false;
                return;
            }

            pnlAllFormats.Visible = true;
            var results = FormatRegistry.SearchFormats(query);
            PopulateFormatButtons(flpAllFormats, results);
        }

        private void TogglePanel(Panel panel, LinkLabel link, string expandedText, string collapsedText)
        {
            if (panel.Visible)
            {
                panel.Visible = false;
                link.Text = "▶ " + collapsedText;
            }
            else
            {
                panel.Visible = true;
                link.Text = "▼ " + expandedText;
            }
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            if (_task.TargetFormat == null && !_task.IsCustomFormat)
            {
                MessageBox.Show("Выберите формат для конвертации.", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_ffmpegPath == null)
            {
                MessageBox.Show("ffmpeg.exe не найден. Убедитесь, что ffmpeg установлен и доступен.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _task.OutputPath = _task.GenerateOutputPath();
            var arguments = FfmpegBuilder.BuildArguments(_task);

            btnConvert.Enabled = false;
            btnCancel.Enabled = true;
            progressBar.Value = 0;
            lblProgress.Text = "Подготовка...";

            _runner.ProgressChanged += Runner_ProgressChanged;
            _runner.Completed += Runner_Completed;
            _runner.ErrorOccurred += Runner_ErrorOccurred;
            _runner.Run(_ffmpegPath, arguments);
        }

        private void Runner_ProgressChanged(object sender, FfmpegProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Runner_ProgressChanged(sender, e)));
                return;
            }

            progressBar.Value = Math.Min(100, (int)e.Percent);
            var speedText = string.IsNullOrEmpty(e.Speed) ? "" : $" | Скорость: {e.Speed}";
            lblProgress.Text = $"{e.Percent:F1}%{speedText}";
        }

        private void Runner_Completed(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Runner_Completed(sender, e)));
                return;
            }

            progressBar.Value = 100;
            lblProgress.Text = "Готово!";
            btnConvert.Enabled = true;
            btnCancel.Enabled = false;

            MessageBox.Show($"Файл успешно конвертирован:\n{_task.OutputPath}",
                "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }

        private void Runner_ErrorOccurred(object sender, string error)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Runner_ErrorOccurred(sender, error)));
                return;
            }

            lblProgress.Text = "Ошибка!";
            btnConvert.Enabled = true;
            btnCancel.Enabled = false;

            MessageBox.Show($"Ошибка конвертации:\n{error}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            _runner.Cancel();
            btnConvert.Enabled = true;
            btnCancel.Enabled = false;
            lblProgress.Text = "Отменено";
        }

        private string FindFfmpegPath()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var localPath = Path.Combine(appDir, "ffmpeg.exe");
            if (File.Exists(localPath))
                return localPath;

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "ffmpeg.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                using (var proc = System.Diagnostics.Process.Start(psi))
                {
                    var output = proc.StandardOutput.ReadLine();
                    proc.WaitForExit();
                    if (proc.ExitCode == 0 && !string.IsNullOrEmpty(output) && File.Exists(output))
                        return output;
                }
            }
            catch { }

            return null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_runner.IsRunning)
                _runner.Cancel();
            base.OnFormClosed(e);
        }
    }
}
