using System.Text;
using YTSubConverter.Shared.Formats.Ass;

namespace YTSubConverter.Shared
{
    public class HtmlStylePreviewGenerator : StylePreviewGenerator
    {
        public static string Generate(AssStyle style, AssStyleOptions options, AssStyle defaultStyle, float windowsScaleFactor)
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
                              -ms-user-select: none;
                              -webkit-user-select: none;
                              user-select: none;
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

            GenerateBackgroundImageCss(html, "body", options, true);

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
                  <body oncontextmenu=""return false;"">
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
    }
}
