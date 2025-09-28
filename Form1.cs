using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace BluetifyApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            var webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(webView);

            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate("http://cedkemc.alwaysdata.net/music/");
        }
    }
}