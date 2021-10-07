using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Gtk;

namespace YTSubConverter.UI.Linux
{
    internal static class GtkExtensions
    {
        public static void AddRange<T>(this ListStore store, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                store.AppendValues(new[] { item });
            }
        }

        public static IEnumerable<T> AsEnumerable<T>(this ListStore store)
        {
            if (!store.GetIterFirst(out TreeIter iter))
                yield break;

            do
            {
                yield return (T)store.GetValue(iter, 0);
            } while (store.IterNext(ref iter));
        }

        public static void AppendPropertyColumn<T>(this TreeView treeView, string title, Func<T, string> getValue)
        {
            treeView.AppendColumn(
                title,
                new CellRendererText(),
                (TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter) =>
                {
                    T item = (T)model.GetValue(iter, 0);
                    ((CellRendererText)cell).Text = getValue(item);
                }
            );
        }

        public static TreeIter GetSelected(this TreeSelection selection)
        {
            selection.GetSelected(out TreeIter iter);
            return iter;
        }

        public static object GetSelectedModelValue(this TreeSelection selection)
        {
            selection.GetSelected(out ITreeModel model, out TreeIter iter);
            return model?.GetValue(iter, 0);
        }

        public static Color GetColor(this ColorButton button)
        {
            return button.Rgba.ToColor();
        }

        public static void SetColor(this ColorButton button, Color color)
        {
            button.Rgba = color.ToRgba();
            GLib.Signal.Emit(button, "color-set");
        }

        public static Gdk.RGBA ToRgba(this Color color)
        {
            return new Gdk.RGBA
                       {
                           Red = color.R / 255.0,
                           Green = color.G / 255.0,
                           Blue = color.B / 255.0,
                           Alpha = color.A / 255.0
                       };
        }

        public static Color ToColor(this Gdk.RGBA rgba)
        {
            if (rgba.Red == 0 && rgba.Green == 0 && rgba.Blue == 0 && rgba.Alpha == 0)
                return Color.Empty;

            return Color.FromArgb(
                (int)(rgba.Alpha * 255),
                (int)(rgba.Red * 255),
                (int)(rgba.Green * 255),
                (int)(rgba.Blue * 255)
            );
        }

        public static Pango.Rectangle ToPixelsInclusive(this Pango.Rectangle rect)
        {
            Pango.Rectangle dummy = new Pango.Rectangle();
            Pango.Global.ExtentsToPixels(ref rect, ref dummy);
            return rect;
        }

        public static void ApplyCss(this Widget widget, StringBuilder css)
        {
            widget.ApplyCss(css.ToString());
        }

        public static void ApplyCss(this Widget widget, string css)
        {
            CssProvider provider = new CssProvider();
            provider.LoadFromData(css);
            widget.StyleContext.AddProvider(provider, 800);
        }

        public static void Clear(this Container container)
        {
            while (container.Children.Length > 0)
            {
                container.Remove(container.Children[0]);
            }
        }
    }
}
