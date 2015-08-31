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
    public partial class Form1 : Form
    {
        public List<Tuple<int,SessionEventArgs>> connections = new List<Tuple<int, SessionEventArgs>>();
        internal int i = 0;
        internal bool flag = false;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                ProxyServer.Stop();
            }
            catch
            {

            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

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
        private void button1_Click(object sender, EventArgs e)
        {

            if (WebRequest.DefaultWebProxy.GetProxy(new Uri("http://www.google.com/")).Host.Equals("localhost"))
            {
                stopProxyServer();
                MessageBox.Show(this, "Proxy server successfully stopped.");
                button1.Text = "Start Proxy Server";
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
                button1.Text = "Stop Proxy Server";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

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
