using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace WindowsPluginBase.Nodes
{
    public class NotifyUser : IActionNode
    {
        private static ToastNotifier manager = ToastNotificationManager.CreateToastNotifier("LaminarToast");

        private readonly INodeField _titleField = Constructor.NodeField("Title").WithInput<string>();
        private readonly INodeField _textField = Constructor.NodeField("Body").WithInput<string>();

        public IEnumerable<INodeComponent> Fields { get { yield return _titleField; yield return _textField; } }

        public string NodeName { get; } = "Push Notification";

        public void Evaluate()
        {
            // var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            //var textNodes = template.GetElementsByTagName("text").ToList();
            //foreach (var textNode in textNodes)
            //{
            //    textNode.AppendChild(template.CreateTextNode(_titleField.GetInput<string>()));
            //}

            // var toast = new ToastNotification(template) { Tag = "Laminar App", Group = "C#", ExpirationTime = DateTimeOffset.Now.AddSeconds(3), };
            // manager.Show(toast);
        }
    }
}
