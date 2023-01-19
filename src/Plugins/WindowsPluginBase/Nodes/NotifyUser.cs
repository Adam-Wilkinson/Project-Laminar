using System.Collections.Generic;
using Laminar.PluginFramework.NodeSystem;
using Windows.UI.Notifications;

namespace WindowsPluginBase.Nodes;

public class NotifyUser : INode
{
    private static ToastNotifier manager = ToastNotificationManager.CreateToastNotifier("LaminarToast");

    //private readonly INodeField _titleField = Constructor.NodeField("Title").WithInput<string>();
    //private readonly INodeField _textField = Constructor.NodeField("Body").WithInput<string>();

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
