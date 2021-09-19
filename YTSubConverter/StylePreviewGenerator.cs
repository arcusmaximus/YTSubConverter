using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Arc.YTSubConverter.Formats.Ass;
using Arc.YTSubConverter.Util;

namespace Arc.YTSubConverter
{
    public static class StylePreviewGenerator
    {
        private static readonly string[][] FontLists =
            {
                new[] { "Courier New", "Courier", "Nimbus Mono L", "Cutive Mono", "monospace" },
                new[] { "Times New Roman", "Times", "Georgia", "Cambria", "PT Serif Caption", "serif" },
                new[] { "Lucida Console", "Deja Vu Sans Mono", "DejaVu Sans Mono", "Monaco", "Consolas", "PT Mono", "monospace" },
                new[] { "Comic Sans MS", "Impact", "Handlee", "fantasy" },
                new[] { "Monotype Corsiva", "URW Chancery L", "Apple Chancery", "Dancing Script", "cursive" },
                new[] { "Carrois Gothic SC", "sans-serif-smallcaps" },
                new[] { "Roboto", "YouTube Noto", "Arial Unicode Ms", "Arial", "Helvetica", "Verdana", "PT Sans Caption", "sans-serif" }
            };

        private static readonly Dictionary<string, string> ExtensionToMimeType =
            new Dictionary<string, string>
            {
                { ".bmp", "image/bmp" },
                { ".gif", "image/gif" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".tif", "image/tiff" },
                { ".tiff", "image/tiff" }
            };

        public static string GenerateHtml(AssStyle style, AssStyleOptions options, AssStyle defaultStyle, float windowsScaleFactor)
        {
            StringBuilder html = new StringBuilder();
            html.Append($@"
                  <!DOCTYPE html>
                  <html>
                  <head>
                      <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                      <style>
                          html, body
                          {{
                              width: 100%;
                              height: 100%;
                              padding: 0;
                              margin: 0;
                              cursor: default;
                          }}
                          body
                          {{
                              display: table;
                              background-image: url({GetBackgroundImageUrl(options)});
                              background-position: {GetBackgroundImagePosition(options)};
                              background-repeat: {GetBackgroundImageRepeat(options)};
                              -ms-user-select: none;
                          }}
                          #wrapper
                          {{
                              display: table-cell;
                              height: 100%;
                              padding: 10px;
                              text-align: {GetTextAlign(style, options)};
                              vertical-align: {GetVerticalAlign(style, options)};
                          }}
            ");

            if (options != null)
            {
                GenerateBackgroundCss(html, "#background", style, defaultStyle, windowsScaleFactor);
                GenerateForegroundCss(html, "#regular", style, style.PrimaryColor, style.OutlineColor, style.ShadowColor, options.ShadowTypes);
                if (options.IsKaraoke)
                {
                    GenerateForegroundCss(
                        html,
                        "#singing",
                        style,
                        !options.CurrentWordTextColor.IsEmpty ? options.CurrentWordTextColor : style.PrimaryColor,
                        !options.CurrentWordOutlineColor.IsEmpty ? options.CurrentWordOutlineColor : style.OutlineColor,
                        !options.CurrentWordShadowColor.IsEmpty ? options.CurrentWordShadowColor : style.ShadowColor,
                        options.ShadowTypes
                    );
                    GenerateForegroundCss(html, "#unsung", style, style.SecondaryColor, style.OutlineColor, style.ShadowColor, options.ShadowTypes);
                }
            }

            html.Append(@"
                      </style>
                  </head>
                  <body>
                      <div id=""wrapper"">
            ");

            if (options != null)
            {
                html.Append(@"<span id=""background"">");

                if (options.IsKaraoke)
                {
                    html.Append($@"<span id=""regular"">{Resources.PreviewSampleKaraoke1}</span>");
                    html.Append($@"<span id=""singing"">{Resources.PreviewSampleKaraoke2}</span>");
                    html.Append($@"<span id=""unsung"">{Resources.PreviewSampleKaraoke3}</span>");
                }
                else
                {
                    html.Append($@"<span id=""regular"">{Resources.PreviewSampleRegular}</span>");
                }

                html.Append(@"</span>");
            }

            html.Append(@"
                      </div>
                  </body>
                  </html>
            ");
            return html.ToString();
        }

        private static void GenerateBackgroundCss(StringBuilder html, string selector, AssStyle style, AssStyle defaultStyle, float windowsScaleFactor)
        {
            html.Append($@"
                {selector}
                {{
                    padding: 1px 8px;
                    font-size: {(int)(Math.Max(32 * style.LineHeight / defaultStyle.LineHeight, 24) * windowsScaleFactor)}px;
            ");

            if (style.HasOutline && style.OutlineIsBox)
                html.Append($"background-color: {ToRgba(style.OutlineColor)};");

            html.Append(@"
                }
            ");
        }

        private static void GenerateForegroundCss(StringBuilder html, string selector, AssStyle style, Color foreColor, Color outlineColor, Color shadowColor, List<ShadowType> shadowTypes)
        {
            html.Append($@"
                {selector}
                {{
            ");

            if (style.Bold)
                html.Append("font-weight: bold;");

            if (style.Italic)
                html.Append("font-style: italic;");

            if (style.Underline)
                html.Append("text-decoration: underline;");

            html.Append($"font-family: {string.Join(", ", GetFontListContaining(style.Font).Select(f => "\"" + f + "\""))};");
            html.Append($"color: {ToRgba(foreColor)};");

            List<string> shadows = new List<string>();

            if (style.HasOutline && !style.OutlineIsBox)
                shadows.Add($"0 0 2px {ToHex(outlineColor)}, 0 0 2px {ToHex(outlineColor)}, 0 0 3px {ToHex(outlineColor)}, 0 0 4px {ToHex(outlineColor)}");

            if (style.HasShadow)
            {
                if (shadowTypes.Contains(ShadowType.Glow) && !(style.HasOutline && !style.HasOutlineBox))
                    shadows.Add($"0 0 2px {ToHex(shadowColor)}, 0 0 2px {ToHex(shadowColor)}, 0 0 3px {ToHex(shadowColor)}, 0 0 4px {ToHex(shadowColor)}");

                if (shadowTypes.Contains(ShadowType.Bevel))
                {
                    if (shadowColor.R == 0x22 && shadowColor.G == 0x22 && shadowColor.B == 0x22)
                        shadows.Add("-1px -1px 0 #222222, 1px 1px 0 #CCCCCC");
                    else
                        shadows.Add($"-1px -1px 0 {ToHex(shadowColor)}, 1px 1px 0 {ToHex(shadowColor)}");
                }

                if (shadowTypes.Contains(ShadowType.SoftShadow))
                    shadows.Add($"2px 2px 3px {ToHex(shadowColor)}, 2px 2px 4px {ToHex(shadowColor)}, 2px 2px 5px {ToHex(shadowColor)}");

                if (shadowTypes.Contains(ShadowType.HardShadow))
                    shadows.Add($"1px 1px 0 {ToHex(shadowColor)}, 2px 2px 0 {ToHex(shadowColor)}, 3px 3px 0 {ToHex(shadowColor)}");
            }

            if (shadows.Count > 0)
                html.Append($"text-shadow: {string.Join(", ", shadows)};");

            html.AppendLine(@"
                }
            ");
        }

        private static string GetBackgroundImageUrl(AssStyleOptions options)
        {
            byte[] imageData;
            string mimeType;
            if (options?.HasExistingBackgroundImage ?? false)
            {
                try
                {
                    imageData = File.ReadAllBytes(options.BackgroundImagePath);
                    mimeType = ExtensionToMimeType.GetOrDefault(Path.GetExtension(options.BackgroundImagePath).ToLower()) ?? "image/png";
                }
                catch
                {
                    imageData = Resources.Checkers;
                    mimeType = "image/png";
                }
            }
            else
            {
                imageData = Resources.Checkers;
                mimeType = "image/png";
            }

            return $"data:{mimeType};base64,{Convert.ToBase64String(imageData)}";
        }

        private static string GetBackgroundImagePosition(AssStyleOptions options)
        {
            return options?.HasExistingBackgroundImage ?? false ? "center center" : "left top";
        }

        private static string GetBackgroundImageRepeat(AssStyleOptions options)
        {
            return options?.HasExistingBackgroundImage ?? false ? "no-repeat" : "repeat";
        }

        private static string GetTextAlign(AssStyle style, AssStyleOptions options)
        {
            if (!(options?.HasExistingBackgroundImage ?? false))
                return "center";

            if (AnchorPointUtil.IsLeftAligned(style.AnchorPoint))
                return "left";

            if (AnchorPointUtil.IsRightAligned(style.AnchorPoint))
                return "right";

            return "center";
        }

        private static string GetVerticalAlign(AssStyle style, AssStyleOptions options)
        {
            if (!(options?.HasExistingBackgroundImage ?? false))
                return "middle";

            if (AnchorPointUtil.IsTopAligned(style.AnchorPoint))
                return "top";

            if (AnchorPointUtil.IsBottomAligned(style.AnchorPoint))
                return "bottom";

            return "middle";
        }

        private static string[] GetFontListContaining(string font)
        {
            if (string.IsNullOrEmpty(font))
                return GetFontListContaining("Roboto");

            foreach (string[] fontList in FontLists)
            {
                if (fontList.Any(f => f.Equals(font, StringComparison.InvariantCultureIgnoreCase)))
                    return fontList;
            }
            return GetFontListContaining("Roboto");
        }

        private static string ToRgba(Color color)
        {
            return $"rgba({color.R}, {color.G}, {color.B}, {(color.A / 255f).ToString(CultureInfo.InvariantCulture)})";
        }

        private static string ToHex(Color color)
        {
            return $"#{color.R:X02}{color.G:X02}{color.B:X02}";
        }
    }
}
