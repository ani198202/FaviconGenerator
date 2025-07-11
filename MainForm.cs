using IconLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FaviconGenerator
{
    public class MainForm : Form
    {
        // 在類別開頭新增欄位
        private bool _isGenerating = false;

        // 控制項宣告
        private TextBox txtSource;
        private TextBox txtDestination;
        private ComboBox cmbSizes;
        private Button btnBrowseSource;
        private Button btnBrowseDest;
        private Button btnGenerate;
        private Button btnExit;
        private Button btnHelp;

        // 預定義尺寸
        private readonly int[] allSizes = { 16, 24, 32, 48, 64, 96, 128, 256 };

        public MainForm()
        {
            // 基本表單設定
            this.Text = "Favicon 生成工具 (.NET)";
            this.ClientSize = new Size(500, 180);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 1. 第一行：來源檔案
            var lblSource = new Label
            {
                Text = "來源檔案:",
                Location = new Point(10, 15),
                Width = 80
            };

            txtSource = new TextBox
            {
                Location = new Point(100, 12),
                Width = 300
            };

            btnBrowseSource = new Button
            {
                Text = "瀏覽...",
                Location = new Point(410, 10),
                Width = 70
            };
            btnBrowseSource.Click += BrowseSource_Click;

            // 2. 第二行：目的地目錄
            var lblDest = new Label
            {
                Text = "輸出目錄:",
                Location = new Point(10, 45),
                Width = 80
            };

            txtDestination = new TextBox
            {
                Location = new Point(100, 42),
                Width = 300
            };

            btnBrowseDest = new Button
            {
                Text = "瀏覽...",
                Location = new Point(410, 40),
                Width = 70
            };
            btnBrowseDest.Click += BrowseDest_Click;

            // 3. 第三行：尺寸選擇
            var lblSize = new Label
            {
                Text = "輸出選項:",
                Location = new Point(10, 75),
                Width = 80
            };

            cmbSizes = new ComboBox
            {
                Location = new Point(100, 72),
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSizes.Items.AddRange(new object[] {
                "全尺寸 ICO (包含所有標準尺寸)",  // 對應 Python 的 'all' 參數
                "全尺寸 ICO + 獨立 PNG",         // 對應 Python 的 'all+' 參數
                "16x16 (標準)",
                "32x32 (標準)",
                "48x48 (標準)",
                "自訂尺寸..."
            });
            cmbSizes.SelectedIndex = 0; // 預設選中全尺寸

            // 4. 第四行：功能按鈕
            btnGenerate = new Button
            {
                Text = "執行生成",
                Location = new Point(100, 110),
                Width = 100,
                BackColor = Color.LightGreen
            };
            btnGenerate.Click += btnGenerate_Click; // 非同步版本

            btnExit = new Button
            {
                Text = "結束",
                Location = new Point(210, 110),
                Width = 100
            };
            btnExit.Click += (s, e) => this.Close();

            btnHelp = new Button
            {
                Text = "幫助",
                Location = new Point(320, 110),
                Width = 100
            };
            btnHelp.Click += ShowHelp;

            // 將所有控制項加入表單
            this.Controls.AddRange(new Control[] {
                lblSource, txtSource, btnBrowseSource,
                lblDest, txtDestination, btnBrowseDest,
                lblSize, cmbSizes,
                btnGenerate, btnExit, btnHelp
            });
        }

        private void BrowseSource_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "PNG 圖片|*.png|所有檔案|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSource.Text = dialog.FileName;
                    if (string.IsNullOrEmpty(txtDestination.Text))
                    {
                        txtDestination.Text = Path.GetDirectoryName(dialog.FileName);
                    }
                }
            }
        }

        private void BrowseDest_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtDestination.Text = dialog.SelectedPath;
                }
            }
        }

        // 修改按鈕事件處理方法
        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            if (_isGenerating) return;

            _isGenerating = true;
            btnGenerate.Enabled = false;
            btnGenerate.Text = "生成中...";
            btnGenerate.BackColor = Color.LightGray;

            try
            {
                await Task.Run(() =>
                {
                    // 將原有同步程式碼移至此處
                    if (string.IsNullOrEmpty(txtSource.Text) || !File.Exists(txtSource.Text))
                    {
                        MessageBox.Show("請選擇有效的來源檔案！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string baseName = Path.GetFileNameWithoutExtension(txtSource.Text);
                    string outputDir = txtDestination.Text;

                    // 根據選擇的選項決定處理方式
                    switch (cmbSizes.SelectedIndex)
                    {
                        case 0: // 全尺寸 ICO
                            GenerateIcoFile(txtSource.Text, outputDir, baseName, allSizes);
                            break;
                        case 1: // 全尺寸 ICO + PNG
                            GenerateIcoFile(txtSource.Text, outputDir, baseName, allSizes);
                            GeneratePngFiles(txtSource.Text, outputDir, baseName, allSizes);
                            break;
                        case 2: // 16x16
                            GenerateIcoFile(txtSource.Text, outputDir, baseName, new[] { 16 });
                            break;
                        case 3: // 32x32
                            GenerateIcoFile(txtSource.Text, outputDir, baseName, new[] { 32 });
                            break;
                        case 4: // 48x48
                            GenerateIcoFile(txtSource.Text, outputDir, baseName, new[] { 48 });
                            break;
                        case 5: // 自訂尺寸
                            HandleCustomSize(txtSource.Text, outputDir, baseName);
                            break;
                    }
                });

                MessageBox.Show("Favicon 生成完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("無法寫入輸出目錄，請檢查權限！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("圖片尺寸過大，請減少尺寸或使用較小的原始圖檔！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Parameter is not valid"))
            {
                MessageBox.Show("不支援的圖片格式，請使用 PNG 或其他標準格式！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("生成失敗: {0}\n{1}", ex.GetType().Name, ex.Message), "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGenerate.Enabled = true;
                btnGenerate.Text = "執行生成";
                btnGenerate.BackColor = Color.LightGreen;
                _isGenerating = false;
            }
        }

        private void GenerateIcoFile(string sourcePath, string outputDir, string baseName, IEnumerable<int> sizes)
        {
            // 確保輸出目錄存在
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string icoPath = Path.Combine(outputDir, string.Format("{0}.ico", baseName));

            using (var original = Image.FromFile(sourcePath))
            using (var multiIcon = new MultiIcon())
            {
                var icon = multiIcon.Add("Favicon"); // 圖標名稱（可自訂）

                foreach (var size in sizes)
                {
                    // 修改這裡：建立支援透明度的 Bitmap
                    using (var resized = new Bitmap(size, size, PixelFormat.Format32bppArgb)) // 新增 PixelFormat
                    using (var g = Graphics.FromImage(resized))
                    {
                        g.Clear(Color.Transparent); // 清除為透明背景
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(original, 0, 0, size, size);
                        icon.Add(resized);
                    }
                }

                // 儲存為 ICO 檔案
                multiIcon.Save(icoPath, MultiIconFormat.ICO);
            }
        }

        private void GeneratePngFiles(string sourcePath, string outputDir, string baseName, IEnumerable<int> sizes)
        {
            using (var original = Image.FromFile(sourcePath))
            {
                foreach (var size in sizes)
                {
                    using (var resized = new Bitmap(original, new Size(size, size)))
                    {
                        string pngPath = Path.Combine(outputDir, string.Format("{0}_{1}x{1}.png", baseName, size));
                        resized.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
        }

        private void HandleCustomSize(string sourcePath, string outputDir, string baseName)
        {
            using (var dialog = new InputDialog("自訂尺寸", "請輸入尺寸 (多個尺寸用逗號分隔):", "16,32,48"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var customSizes = dialog.InputText.Split(',')
                        .Select(s => int.TryParse(s.Trim(), out int size) ? size : 0)
                        .Where(size => size > 0)
                        .ToArray();

                    if (customSizes.Length == 0)
                    {
                        MessageBox.Show("無效的尺寸輸入！", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    GenerateIcoFile(sourcePath, outputDir, baseName, customSizes);
                }
            }
        }

        private void ShowHelp(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Favicon 生成工具使用說明:\n\n" +
                "1. 選擇來源 PNG 圖片\n" +
                "2. 指定輸出目錄\n" +
                "3. 選擇輸出選項:\n" +
                "   - 全尺寸 ICO: 生成包含所有標準尺寸的單一 .ico 文件\n" +
                "   - 全尺寸 ICO + PNG: 同時生成 .ico 和各尺寸的 .png\n" +
                "   - 單一尺寸: 只生成指定尺寸的 .ico\n" +
                "   - 自訂尺寸: 手動輸入多個尺寸\n\n" +
                "標準尺寸包含: 16, 24, 32, 48, 64, 96, 128, 256 像素",
                "幫助",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}