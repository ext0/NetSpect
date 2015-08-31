using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Titanium.Web.Proxy.Models;
using RequestBuilder;
using System.Net;
using Titanium.Web.Proxy;
using System.Threading;

namespace RequestBuilder
{
    public partial class MainForm : Form
    {
        public List<Tuple<int,SessionEventArgs>> connections = new List<Tuple<int, SessionEventArgs>>();
        internal int i = 0;
        internal bool flag = false;
        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ProxyServer.Stop();
            }
            catch
            {

            }
        }
        public bool startProxyServer(bool SSL, bool sysProxy)
        {
            ProxyServer.BeforeRequest += OnRequest;
            ProxyServer.BeforeResponse += OnResponse;

            ProxyServer.EnableSSL = SSL;
            ProxyServer.SetAsSystemProxy = sysProxy;
            return ProxyServer.Start();
        }
        public void stopProxyServer()
        {
            ProxyServer.BeforeRequest -= OnRequest;
            ProxyServer.BeforeResponse -= OnResponse;
            ProxyServer.Stop();
        }
        public void OnRequest(object sender, SessionEventArgs e)
        {
            flag = true;
            addConnection(i, e);
            if (connections.Count > 100000)
            {
                connections.RemoveAt(0);
            }
            connections.Add(new Tuple<int, SessionEventArgs>(i,e));
            i++;
            flag = false;
        }
        public void OnResponse(object sender, SessionEventArgs e)
        {
            foreach (Tuple<int, SessionEventArgs> request in connections)
            {
                if (e.ProxyRequest.Equals(request.Item2.ProxyRequest))
                {
                    modifyFrom(request.Item1, e);
                    break;
                }
            }
            if (e.ServerResponse.StatusCode == HttpStatusCode.OK)
            {
                if (e.ServerResponse.ContentType.Trim().ToLower().Contains("text/html"))
                {
                    if (e.GetResponseHtmlBody() != null)
                    {
                        try
                        {
                            e.Ok("<!-- Processed by RequestBuilder -->\n" + e.GetResponseHtmlBody());
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
        private void startButton_Click(object sender, EventArgs e)
        {

            if (WebRequest.DefaultWebProxy.GetProxy(new Uri("http://www.google.com/")).Host.Equals("localhost"))
            {
                stopProxyServer();
                MessageBox.Show(this, "Proxy server successfully stopped.");
                startButton.Text = "Start Proxy Server";
            }
            else
            {
                if (startProxyServer(true, true))
                {
                    MessageBox.Show(this, "Proxy server successfully started.");
                }
                else
                {
                    MessageBox.Show(this, "Proxy server failed to start!");
                }
                startButton.Text = "Stop Proxy Server";
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            setConnection(connections[e.RowIndex].Item2);
        }
    }
    class ConnectionView : ListViewItem
    {
        public ConnectionView(SessionEventArgs e)
        {
            this.Name = e.RequestHostname;
            this.SubItems.Add(e.RequestHostname);
        }
    }
}
