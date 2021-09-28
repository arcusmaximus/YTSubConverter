using System;
using System.Collections.Generic;
using System.Drawing;
using AppKit;

namespace Arc.YTSubConverter.UI.Mac
{
    internal static class CocoaExtensions
    {
        private static readonly HashSet<NSView> DisabledViews = new();

        public static bool IsEnabled(this NSView view)
        {
            return !DisabledViews.Contains(view);
        }

        public static void SetEnabled(this NSView view, bool enabled)
        {
            if (enabled)
                DisabledViews.Remove(view);
            else
                DisabledViews.Add(view);

            if (enabled)
            {
                NSView superView = view.Superview;
                while (superView != null)
                {
                    if (!superView.IsEnabled())
                        return;

                    superView = superView.Superview;
                }
            }

            SetEnabledRecursive(view, enabled);
        }

        private static void SetEnabledRecursive(NSView view, bool enabled)
        {
            if (view is NSTextField label)
                label.TextColor = enabled ? NSColor.ControlText : NSColor.DisabledControlText;
            else if (view is NSControl control)
                control.Enabled = enabled;

            foreach (NSView subView in view.Subviews)
            {
                if (!subView.IsEnabled())
                    continue;

                SetEnabledRecursive(subView, enabled);
            }
        }

        public static bool IsChecked(this NSButton button)
        {
            return button.State == NSCellStateValue.On;
        }

        public static void SetChecked(this NSButton button, bool isChecked)
        {
            if (isChecked == button.IsChecked())
                return;

            button.State = isChecked ? NSCellStateValue.On : NSCellStateValue.Off;
            if (button.Action != null)
                button.SendAction(button.Action, button.Target);
        }

        public static Color GetColor(this NSColorWell well)
        {
            return well.Color.ToColor();
        }

        public static void SetColor(this NSColorWell well, Color color)
        {
            well.Color = color.ToNSColor();
            if (well.Action != null)
                well.SendAction(well.Action, well.Target);
        }

        public static NSColor ToNSColor(this Color color)
        {
            if (color.IsEmpty)
                return NSColor.Clear;

            return NSColor.FromRgba(color.R, color.G, color.B, color.A);
        }

        public static Color ToColor(this NSColor nsColor)
        {
            if (nsColor == null || nsColor == NSColor.Clear)
                return Color.Empty;

            nsColor = nsColor.UsingColorSpace(NSColorSpace.GenericRGBColorSpace);
            nsColor.GetRgba(out nfloat r, out nfloat g, out nfloat b, out nfloat a);
            return Color.FromArgb(
                (int)(a * 255),
                (int)(r * 255),
                (int)(g * 255),
                (int)(b * 255)
            );
        }
    }
}
