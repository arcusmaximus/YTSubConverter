using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Arc.YTSubConverter.Formats.Ass;

namespace Arc.YTSubConverter
{
    internal static class StylePreviewGenerator
    {
        private static readonly string BackgroundImageData;

        static StylePreviewGenerator()
        {
            byte[] backgroundImage = Resources.Checkers;
            BackgroundImageData = Convert.ToBase64String(backgroundImage);
        }

        public static string GenerateHtml(AssStyle style, AssStyleOptions options)
        {
            StringBuilder html = new StringBuilder();
            html.Append($@"
                  <!DOCTYPE html>
                  <html>
                  <head>
                      <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                      <style>
                          html, body {{ width: 100%; height: 100%; padding: 0; margin: 0; }}
                          body {{ display: table; background-image: url(data:image/png;base64,{BackgroundImageData}); }}
                          #wrapper {{ display: table-cell; height: 100%; text-align: center; vertical-align: middle; }}
            ");

            if (options != null)
            {
                GenerateBackgroundCss(html, "#background", style);
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

        private static void GenerateBackgroundCss(StringBuilder html, string selector, AssStyle style)
        {
            html.Append($@"
                {selector}
                {{
                    padding: 3px 5px;
                    border-radius: 3px;
                    font-size: 32px;
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

            if (IsSupportedFont(style.Font))
                html.Append($"font-family: '{style.Font}';");
            else
                html.Append("font-family: 'Arial';");

            html.Append($"color: {ToRgba(foreColor)};");

            List<string> shadows = new List<string>();

            if (style.HasOutline && !style.OutlineIsBox)
                shadows.Add($"0 0 2px {ToHex(outlineColor)}, 0 0 2px {ToHex(outlineColor)}, 0 0 3px {ToHex(outlineColor)}, 0 0 4px {ToHex(outlineColor)}");

            if (style.HasShadow)
            {
                if (shadowTypes.Contains(ShadowType.Glow) && !(style.HasOutline && !style.HasOutlineBox))
                    shadows.Add($"0 0 2px {ToHex(shadowColor)}, 0 0 2px {ToHex(shadowColor)}, 0 0 3px {ToHex(shadowColor)}, 0 0 4px {ToHex(shadowColor)}");

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

        private static bool IsSupportedFont(string font)
        {
            return font == "YouTube Noto" ||
                   font == "Courier New" ||
                   font == "Times New Roman" ||
                   font == "Deja Vu Sans Mono" ||
                   font == "Comic Sans MS" ||
                   font == "Monotype Corsiva" ||
                   font == "Carrois Gothic SC";
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
