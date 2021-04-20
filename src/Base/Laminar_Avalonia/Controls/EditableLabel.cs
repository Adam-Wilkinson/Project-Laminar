using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Avalonia.Controls
{
    public class EditableLabel : TemplatedControl
    {
        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<EditableLabel, string>(nameof(Text));
        public static readonly StyledProperty<bool> IsEditingProperty = AvaloniaProperty.Register<EditableLabel, bool>(nameof(IsEditing), false);

        private TextBox _enterBox;

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool IsEditing
        {
            get => GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            e.NameScope.Find<TextBlock>("PART_Display").DoubleTapped += delegate { StartEditingName(); };
            _enterBox = e.NameScope.Find<TextBox>("PART_Editor");
            _enterBox.LostFocus += delegate { StopEditingName(); };
            if (IsEditing)
            {
                StartEditingName();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            NodeDisplay_KeyDown(this, e);
        }

        private void NodeDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter)
            {
                StopEditingName();
            };
        }

        private void StopEditingName()
        {
            IsEditing = false;
        }

        public void StartEditingName()
        {
            IsEditing = true;
            _enterBox?.SelectAll();
            _enterBox?.Focus();
        }
    }
}
