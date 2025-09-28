using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

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

            Panel titleBar = new Panel
            {
                Height = 25,
                Dock = DockStyle.Top,
                BackColor = Color.Black
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

            Button closeBtn = new Button
            {
                Text = "X",
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Width = 50,
                Dock = DockStyle.Right
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => this.Close();
            titleBar.Controls.Add(closeBtn);

            Button minBtn = new Button
            {
                Text = "_",
                ForeColor = Color.White,
                BackColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Width = 50,
                Dock = DockStyle.Right
            };
            minBtn.FlatAppearance.BorderSize = 0;
            minBtn.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            titleBar.Controls.Add(minBtn);

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
        }

        private async void InitializeWebView(WebView2 webView)
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate("https://bluetify.alwaysdata.net");
        }
    }
}