using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly:ComponentFactory(typeof(RemoteFactory))]

namespace LiveSplit.UI.Components
{
    public class RemoteFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "LiveSplit Remote"; }
        }

        public string Description
        {
            get { return "LiveSplit Remote allows you to control LiveSplit from a web based timer."; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new RemoteComponent(state);
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string UpdateURL
        {
            get { return "http://livesplit.org/update/"; }
        }

        public Version Version
        {
            get { return Version.Parse("1.0.0"); }
        }

        public string XMLURL
        {
            get { return "http://livesplit.org/update/Components/LiveSplit.Remote.xml"; }
        }
    }
}
