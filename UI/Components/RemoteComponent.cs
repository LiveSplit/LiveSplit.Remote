using LiveSplit.Model;
using LiveSplit.Model.Input;
using LiveSplit.TimeFormatters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace LiveSplit.UI.Components
{
    public class RemoteComponent : LogicComponent
    {
        protected HttpListener HttpListener { get; set; }
        protected WebSocketServer WebSocketServer { get; set; }
        protected IDictionary<string, byte[]> HostedFiles { get; set; }

        public override string ComponentName
        {
            get { return "LiveSplit Remote"; }
        }

        public RemoteComponent(LiveSplitState state)
        {
            SetUpHttpListener();
            SetUpWebSocketServer(state);
            SetUpHostedFiles();
        }

        private void SetUpHostedFiles()
        {
            HostedFiles = new Dictionary<string, byte[]>();

            HostedFiles.Add("/", Encoding.UTF8.GetBytes(WebFiles.index_html));
            HostedFiles.Add("/index.html", Encoding.UTF8.GetBytes(WebFiles.index_html));
            HostedFiles.Add("/webstorage.manifest", Encoding.UTF8.GetBytes(WebFiles.webstorage_manifest));
            HostedFiles.Add("/js/timer.js", Encoding.UTF8.GetBytes(WebFiles.timer_js));
            HostedFiles.Add("/js/fastclick-min.js", Encoding.UTF8.GetBytes(WebFiles.fastclick_min_js));
            HostedFiles.Add("/js/FileSaver.min.js", Encoding.UTF8.GetBytes(WebFiles.FileSaver_min_js));
            HostedFiles.Add("/js/slideout.min.js", Encoding.UTF8.GetBytes(WebFiles.slideout_min_js));
            HostedFiles.Add("/css/timer.css", Encoding.UTF8.GetBytes(WebFiles.timer_css));


            using (var memoryStream = new MemoryStream())
            {
                WebFiles.Icon.Save(memoryStream);
                HostedFiles.Add("/favicon.ico", memoryStream.GetBuffer());
            }
        }

        private void SetUpHttpListener()
        {
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add("http://localhost:58321/");
            HttpListener.Start();
            HttpListener.BeginGetContext(GotContext, null);
        }

        private void SetUpWebSocketServer(LiveSplitState state)
        {
            WebSocketServer = new WebSocketServer(58322);
            WebSocketServer.AddWebSocketService<Behavior>("/websocket", () => new Behavior(state));
            WebSocketServer.Start();
        }

        private string ResolveTontentType(string url)
        {
            if (url.EndsWith(".js"))
            {
                return "application/javascript";
            }
            else if (url.EndsWith(".css"))
            {
                return "text/css";
            }
            else if (url.EndsWith(".ico"))
            {
                return "image/x-icon";
            }
            else if (url.EndsWith(".manifest"))
            {
                return "text/cache-manifest";
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

                byte[] response = null;

                if (HostedFiles.ContainsKey(url))
                {
                    response = HostedFiles[url];
                }
                else
                {
                    response = Encoding.UTF8.GetBytes("404");
                }

                var contentType = ResolveTontentType(url);
                context.Response.ContentType = contentType;

                using (var stream = context.Response.OutputStream)
                {
                    stream.Write(response, 0, response.Length);
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
            if (HttpListener != null)
            {
                HttpListener.Close();
                HttpListener = null;
            }
            if (WebSocketServer != null)
            {
                WebSocketServer.Stop();
                WebSocketServer = null;
            }
            //TODO Dispose Behavior?
        }

        public class Behavior : WebSocketBehavior, IDisposable
        {
            private LiveSplitState state;

            public Behavior(LiveSplitState state)
            {
                this.state = state;
                state.OnPause += state_OnPause;
                state.OnReset += state_OnReset;
                state.OnResume += state_OnResume;
                state.OnSkipSplit += state_OnSkipSplit;
                state.OnSplit += state_OnSplit;
                state.OnStart += state_OnStart;
                state.OnUndoSplit += state_OnUndoSplit;
            }

            void state_OnUndoSplit(object sender, EventArgs e)
            {
                Send("undosplit");
            }

            void state_OnStart(object sender, EventArgs e)
            {
                Send("start");
            }

            void state_OnSplit(object sender, EventArgs e)
            {
                Send("split");
            }

            void state_OnSkipSplit(object sender, EventArgs e)
            {
                Send("skipsplit");
            }

            void state_OnResume(object sender, EventArgs e)
            {
                Send("resume");
            }

            void state_OnReset(object sender, TimerPhase value)
            {
                Send("reset");
            }

            void state_OnPause(object sender, EventArgs e)
            {
                Send("paused");
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                var timeFormatter = new ShortTimeFormatter();
                Send(timeFormatter.Format(state.CurrentTime.RealTime));
            }

            public void Dispose()
            {
                //TODO Remove events
            }
        }
    }
}
