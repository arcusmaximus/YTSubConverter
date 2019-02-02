using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using Arc.YTSubConverter.Ass;

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
                GenerateCss(html, "#regular", style, style.PrimaryColor, style.ShadowColor, options.ShadowTypes);
                if (options.IsKaraoke)
                {
                    GenerateCss(
                        html,
                        "#singing",
                        style,
                        !options.CurrentWordTextColor.IsEmpty ? options.CurrentWordTextColor : style.PrimaryColor,
                        !options.CurrentWordShadowColor.IsEmpty ? options.CurrentWordShadowColor : style.ShadowColor,
                        options.ShadowTypes
                    );
                    GenerateCss(html, "#unsung", style, style.SecondaryColor, style.ShadowColor, options.ShadowTypes);
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
                if (options.IsKaraoke)
                    html.Append(@"<span id=""regular"">This is a</span><span id=""singing"">sample</span><span id=""unsung"">text</span>");
                else
                    html.Append(@"<span id=""regular"">Sample text</span>");
            }

            html.Append(@"
                      </div>
                  </body>
                  </html>
            ");
            return html.ToString();
        }

        private static void GenerateCss(StringBuilder html, string selector, AssStyle style, Color foreColor, Color shadowColor, ShadowType shadowTypes)
        {
            html.Append($@"
                {selector}
                {{
                    padding: 3px 5px;
                    border-radius: 3px;
                    font-size: 32px;
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

            if (style.HasOutline)
            {
                if (style.OutlineIsBox)
                    html.Append($"background-color: {ToRgba(style.OutlineColor)};");
                else
                    html.Append($"text-shadow: 0 0 3px {ToHex(style.OutlineColor)}");
            }

            if (style.HasShadow && (!style.HasOutline || style.OutlineIsBox))
            {
                List<string> shadows = new List<string>();
                if ((shadowTypes & ShadowType.Glow) != 0)
                    shadows.Add($"0 0 2px {ToHex(shadowColor)}, 0 0 2px {ToHex(shadowColor)}, 0 0 3px {ToHex(shadowColor)}, 0 0 4px {ToHex(shadowColor)}");

                if ((shadowTypes & ShadowType.SoftShadow) != 0)
                    shadows.Add($"2px 2px 3px {ToHex(shadowColor)}, 2px 2px 4px {ToHex(shadowColor)}, 2px 2px 5px {ToHex(shadowColor)}");

                if ((shadowTypes & ShadowType.HardShadow) != 0)
                    shadows.Add($"1px 1px 0 {ToHex(shadowColor)}, 2px 2px 0 {ToHex(shadowColor)}, 3px 3px 0 {ToHex(shadowColor)}");

                if (shadows.Count > 0)
                    html.Append($"text-shadow: {string.Join(", ", shadows)};");
            }

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
