using System.Text;
using System.Xml.Linq;
using YTSubConverter.Shared;
using YTSubConverter.Shared.Formats.Ass;
using Gtk;

namespace YTSubConverter.UI.Linux
{
    internal class GtkStylePreviewGenerator : StylePreviewGenerator
    {
        public static void Generate(Box container, AssStyle style, AssStyleOptions options, AssStyle defaultStyle)
        {
            container.Clear();

            Box backgroundBox = CreateBackgroundBox();
            if (style != null && options != null)
            {
                string markup;
                if (options.IsKaraoke)
                    markup = GetKaraokePreviewMarkup(style, options, defaultStyle);
                else
                    markup = GetRegularPreviewMarkup(style, options, defaultStyle);

                MultiStyleLabel label =
                    new MultiStyleLabel(backgroundBox)
                    {
                        Markup = markup,
                        Visible = true
                    };
                container.PackStart(label, true, true, 0);
            }
            else
            {
                container.PackStart(backgroundBox, true, true, 0);
            }
        }

        private static Box CreateBackgroundBox()
        {
            Box backgroundBox = new Box(Orientation.Horizontal, 0)
                                {
                                    Name = "background",
                                    Visible = true
                                };

            StringBuilder css = new StringBuilder();
            GenerateBackgroundImageCss(css, "#background", null, false);
            backgroundBox.ApplyCss(css);
            return backgroundBox;
        }

        private static string GetRegularPreviewMarkup(AssStyle style, AssStyleOptions options, AssStyle defaultStyle)
        {
            StringBuilder css = new StringBuilder();
            GenerateBackgroundCss(css, null, style, defaultStyle, 1);
            GenerateForegroundCss(css, null, style, style.PrimaryColor, style.OutlineColor, style.ShadowColor, options.ShadowTypes);
            return GetSpanMarkup(css, Resources.PreviewSampleRegular);
        }

        private static string GetKaraokePreviewMarkup(AssStyle style, AssStyleOptions options, AssStyle defaultStyle)
        {
            StringBuilder markup = new StringBuilder();

            StringBuilder css = new StringBuilder();
            GenerateBackgroundCss(css, null, style, defaultStyle, 1);
            GenerateForegroundCss(css, null, style, style.PrimaryColor, style.OutlineColor, style.ShadowColor, options.ShadowTypes);
            markup.Append(GetSpanMarkup(css, Resources.PreviewSampleKaraoke1));

            css.Clear();
            GenerateBackgroundCss(css, null, style, defaultStyle, 1);
            GenerateForegroundCss(
                css,
                null,
                style,
                !options.CurrentWordTextColor.IsEmpty ? options.CurrentWordTextColor : style.PrimaryColor,
                !options.CurrentWordOutlineColor.IsEmpty ? options.CurrentWordOutlineColor : style.OutlineColor,
                !options.CurrentWordShadowColor.IsEmpty ? options.CurrentWordShadowColor : style.ShadowColor,
                options.ShadowTypes
            );
            markup.Append(GetSpanMarkup(css, Resources.PreviewSampleKaraoke2));

            css.Clear();
            GenerateBackgroundCss(css, null, style, defaultStyle, 1);
            GenerateForegroundCss(css, null, style, style.SecondaryColor, style.OutlineColor, style.ShadowColor, options.ShadowTypes);
            markup.Append(GetSpanMarkup(css, Resources.PreviewSampleKaraoke3));

            return markup.ToString();
        }

        private static string GetSpanMarkup(StringBuilder css, string text)
        {
            return new XElement(
                "span",
                new XAttribute("css", css.Replace("\r\n", "")),
                text
            ).ToString();
        }
    }
}
