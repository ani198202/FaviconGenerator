using System;
using System.Windows.Forms;

namespace FaviconGenerator
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm()); // 假設您的主視窗類別是 MainForm
        }
    }
}