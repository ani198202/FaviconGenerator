using System;
using System.Drawing;
using System.Windows.Forms;

namespace FaviconGenerator
{
    // 輸入對話框
    public class InputDialog : Form
    {
        public string InputText { get; private set; }

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            this.Text = title;
            this.ClientSize = new Size(400, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(10, 10),
                Width = 380
            };

            var txtInput = new TextBox
            {
                Text = defaultValue,
                Location = new Point(10, 40),
                Width = 380
            };

            var btnOk = new Button
            {
                Text = "確定",
                DialogResult = DialogResult.OK,
                Location = new Point(150, 80)
            };

            btnOk.Click += (s, e) =>
            {
                InputText = txtInput.Text;
                this.Close();
            };

            this.Controls.AddRange(new Control[] { lblPrompt, txtInput, btnOk });
            this.AcceptButton = btnOk;
        }
    }
}