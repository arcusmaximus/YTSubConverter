using System;
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
                GenerateCss(html, style, options);

            html.Append(@"
                      </style>
                  </head>
                  <body>
                      <div id=""wrapper"">
            ");

            if (options != null)
                html.Append(@"<span id=""preview"">Sample text</span>");

            html.Append(@"
                      </div>
                  </body>
                  </html>
            ");
            return html.ToString();
        }

        private static void GenerateCss(StringBuilder html, AssStyle style, AssStyleOptions options)
        {
            html.Append(@"
                #preview
                {
                    padding: 3px;
                    font-size: 12pt;
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

            html.Append($"color: {ToRgba(style.PrimaryColor)};");

            if (style.HasOutline)
            {
                if (style.OutlineIsBox)
                    html.Append($"background-color: {ToRgba(style.OutlineColor)};");
                else
                    html.Append($"text-shadow: 0 0 3px {ToHex(style.OutlineColor)}");
            }

            if (style.HasShadow && (!style.HasOutline || style.OutlineIsBox))
            {
                switch (options.ShadowType)
                {
                    case ShadowType.Glow:
                        html.Append($"text-shadow: 0 0 3px {ToHex(style.ShadowColor)};");
                        break;

                    case ShadowType.SoftShadow:
                        html.Append($"text-shadow: 2px 2px 3px {ToHex(style.ShadowColor)};");
                        break;

                    case ShadowType.HardShadow:
                        html.Append($"text-shadow: 1px 1px 0 {ToHex(style.ShadowColor)};");
                        break;
                }
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
