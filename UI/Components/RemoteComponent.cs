using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class RemoteComponent : LogicComponent
    {
        public override string ComponentName
        {
            get { return "LiveSplit Remote"; }
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
        }
    }
}
