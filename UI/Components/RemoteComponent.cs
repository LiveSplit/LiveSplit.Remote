using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class RemoteComponent : LogicComponent
    {
        protected HttpListener HttpListener { get; set; }
        protected IDictionary<string, string> HostedFiles { get; set; }

        public override string ComponentName
        {
            get { return "LiveSplit Remote"; }
        }

        public RemoteComponent(LiveSplitState state)
        {
            SetUpHttpListener();
            SetUpHostedFiles();
        }

        private void SetUpHostedFiles()
        {
            HostedFiles = new Dictionary<string, string>();

            HostedFiles.Add("/", WebFiles.index_html);
            HostedFiles.Add("/index.html", WebFiles.index_html);
            HostedFiles.Add("/js/timer.js", WebFiles.timer_js);
            HostedFiles.Add("/js/fastclick-min.js", WebFiles.fastclick_min_js);
            HostedFiles.Add("/js/FileSaver.min.js", WebFiles.FileSaver_min_js);
            HostedFiles.Add("/js/slideout.min.js", WebFiles.slideout_min_js);
            HostedFiles.Add("/css/timer.css", WebFiles.timer_css);
        }

        private void SetUpHttpListener()
        {
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add("http://localhost:58321/");
            HttpListener.Start();
            HttpListener.BeginGetContext(GotContext, null);
        }

        private string ResolveTontentType(string url)
        {
            if (url.StartsWith("/js/"))
            {
                return "application/javascript";
            }
            else if (url.StartsWith("/css/"))
            {
                return "text/css";
            }
            else
            {
                return "text/html";
            }
        }

        protected void GotContext(IAsyncResult result)
        {
            if (result.IsCompleted && HttpListener != null)
            {
                var context = HttpListener.EndGetContext(result);

                var url = context.Request.RawUrl;

                string response = null;

                if (HostedFiles.ContainsKey(url))
                {
                    response = HostedFiles[url];
                }
                else
                {
                    response = "404";
                }

                var contentType = ResolveTontentType(url);

                context.Response.ContentType = contentType;

                var stream = context.Response.OutputStream;

                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(response);
                }

                HttpListener.BeginGetContext(GotContext, null);
            }
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("settings");
        }

        public override void SetSettings(XmlNode settings)
        {
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
        }

        public override void Dispose()
        {
            HttpListener.Close();
            HttpListener = null;
        }
    }
}
