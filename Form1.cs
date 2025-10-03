using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System.IO;

namespace BluetifyApp
{
    public partial class Form1 : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private System.Windows.Forms.Timer minimizeTimer;
        private bool isMinimizing = false;
        private int originalHeight;

        protected override void WndProc(ref Message m)
        {
            const int RESIZE_HANDLE_SIZE = 10;
            const int WM_NCHITTEST = 0x84;
            const int HTCLIENT = 1;
            const int HTLEFT = 10;
            const int HTRIGHT = 11;
            const int HTTOP = 12;
            const int HTTOPLEFT = 13;
            const int HTTOPRIGHT = 14;
            const int HTBOTTOM = 15;
            const int HTBOTTOMLEFT = 16;
            const int HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                var hitPoint = this.PointToClient(new Point(m.LParam.ToInt32()));
                if (hitPoint.X <= RESIZE_HANDLE_SIZE && hitPoint.Y <= RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTTOPLEFT;
                else if (hitPoint.X >= this.ClientSize.Width - RESIZE_HANDLE_SIZE && hitPoint.Y <= RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTTOPRIGHT;
                else if (hitPoint.X <= RESIZE_HANDLE_SIZE && hitPoint.Y >= this.ClientSize.Height - RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTBOTTOMLEFT;
                else if (hitPoint.X >= this.ClientSize.Width - RESIZE_HANDLE_SIZE && hitPoint.Y >= this.ClientSize.Height - RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTBOTTOMRIGHT;
                else if (hitPoint.X <= RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTLEFT;
                else if (hitPoint.X >= this.ClientSize.Width - RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTRIGHT;
                else if (hitPoint.Y <= RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTTOP;
                else if (hitPoint.Y >= this.ClientSize.Height - RESIZE_HANDLE_SIZE)
                    m.Result = (IntPtr)HTBOTTOM;
                else
                    m.Result = (IntPtr)HTCLIENT;
                return;
            }
            base.WndProc(ref m);
        }

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.Width = 1024;
            this.Height = 768;
            this.BackColor = Color.Black;
            this.Padding = new Padding(5);
            this.DoubleBuffered = true;

            Panel titleBar = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            this.Controls.Add(titleBar);

            titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };

            Button minBtn = new Button
            {
                Width = 40,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            minBtn.FlatAppearance.BorderSize = 0;
            minBtn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.White, 2))
                {
                    e.Graphics.DrawLine(pen, 12, 18, 28, 18);
                }
            };
            minBtn.Click += (s, e) =>
            {
                if (!isMinimizing)
                {
                    originalHeight = this.Height;
                    isMinimizing = true;
                    minimizeTimer.Start();
                }
            };
            titleBar.Controls.Add(minBtn);

            Button closeBtn = new Button
            {
                Width = 40,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawLine(pen, 12, 12, 28, 28);
                    e.Graphics.DrawLine(pen, 28, 12, 12, 28);
                }
            };
            closeBtn.Click += (s, e) => this.Close();
            titleBar.Controls.Add(closeBtn);

            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Padding = new Padding(0)
            };
            this.Controls.Add(panel);

            var webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(webView);

            InitializeWebView(webView);

            minimizeTimer = new System.Windows.Forms.Timer();
            minimizeTimer.Interval = 15;
            minimizeTimer.Tick += MinimizeTimer_Tick;
        }

        private void MinimizeTimer_Tick(object? sender, EventArgs e)
        {
            if (this.Opacity > 0.3)
                this.Opacity -= 0.05;

            if (this.Height > 100)
                this.Height -= 40;

            if (this.Opacity <= 0.3 || this.Height <= 100)
            {
                minimizeTimer.Stop();
                this.WindowState = FormWindowState.Minimized;
                this.Opacity = 1;
                this.Height = originalHeight;
                isMinimizing = false;
            }
        }

        private async void InitializeWebView(WebView2 webView)
        {
            try
            {
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Bluetify", "WebView2");
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await webView.EnsureCoreWebView2Async(env);
                webView.CoreWebView2.Navigate("http://bluetify.alwaysdata.net/");
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebView2 Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}