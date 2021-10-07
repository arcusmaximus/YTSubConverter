using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using YTSubConverter.Shared.Formats.Ass;
using YTSubConverter.Shared.Util;

namespace YTSubConverter.Shared
{
    public abstract class StylePreviewGenerator
    {
        protected static readonly string[][] FontLists =
            {
                new[] { "Courier New", "Courier", "Nimbus Mono L", "Cutive Mono", "monospace" },
                new[] { "Times New Roman", "Times", "Georgia", "Cambria", "PT Serif Caption", "serif" },
                new[] { "Lucida Console", "Deja Vu Sans Mono", "DejaVu Sans Mono", "Monaco", "Consolas", "PT Mono", "monospace" },
                new[] { "Comic Sans MS", "Impact", "Handlee", "fantasy" },
                new[] { "Monotype Corsiva", "URW Chancery L", "Apple Chancery", "Dancing Script", "cursive" },
                new[] { "Carrois Gothic SC", "sans-serif-smallcaps" },
                new[] { "Roboto", "YouTube Noto", "Arial Unicode Ms", "Arial", "Helvetica", "Verdana", "PT Sans Caption", "sans-serif" }
            };

        protected static readonly Dictionary<string, string> ExtensionToMimeType =
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

        protected static void GenerateBackgroundImageCss(StringBuilder css, string selector, AssStyleOptions options, bool useDataUrl)
        {
            css.Append($@"
                {selector}
                {{
                    background-image: url({GetBackgroundImageUrl(options, useDataUrl)});
                    background-position: {GetBackgroundImagePosition(options)};
                    background-repeat: {GetBackgroundImageRepeat(options)};
                }}
            ");
        }

        protected static void GenerateBackgroundCss(StringBuilder css, string selector, AssStyle style, AssStyle defaultStyle, float windowsScaleFactor)
        {
            if (selector != null)
                css.Append(selector + " {");

            css.Append($@"
                padding: 1px 8px;
                font-size: {(int)(Math.Max(32 * style.LineHeight / defaultStyle.LineHeight, 24) * windowsScaleFactor)}px;
            ");

            if (style.HasOutline && style.OutlineIsBox)
                css.Append($"background-color: {ToRgba(style.OutlineColor)};");

            if (selector != null)
                css.Append("}");
        }

        protected static void GenerateForegroundCss(StringBuilder css, string selector, AssStyle style, Color foreColor, Color outlineColor, Color shadowColor, List<ShadowType> shadowTypes)
        {
            if (selector != null)
                css.Append(selector + " {");

            if (style.Bold)
                css.Append("font-weight: bold;");

            if (style.Italic)
                css.Append("font-style: italic;");

            if (style.Underline)
                css.Append("text-decoration: underline;");

            css.Append($"font-family: {string.Join(", ", GetFontListContaining(style.Font).Select(f => "\"" + f + "\""))};");
            css.Append($"color: {ToRgba(foreColor)};");

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
                css.Append($"text-shadow: {string.Join(", ", shadows)};");

            if (selector != null)
                css.Append("}");
        }

        protected static string GetBackgroundImageUrl(AssStyleOptions options, bool useDataUrl)
        {
            return useDataUrl ? GetBackgroundImageDataUrl(options) : GetBackgroundImageFileUrl(options);
        }

        protected static string GetBackgroundImageFileUrl(AssStyleOptions options)
        {
            string filePath;
            if (options?.HasExistingBackgroundImage ?? false)
                filePath = options.BackgroundImagePath;
            else
                filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "checkers.png");

            return $"'{filePath}'";
        }

        protected static string GetBackgroundImageDataUrl(AssStyleOptions options)
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

        protected static string GetBackgroundImagePosition(AssStyleOptions options)
        {
            return options?.HasExistingBackgroundImage ?? false ? "center center" : "left top";
        }

        protected static string GetBackgroundImageRepeat(AssStyleOptions options)
        {
            return options?.HasExistingBackgroundImage ?? false ? "no-repeat" : "repeat";
        }

        protected static string GetTextAlign(AssStyle style, AssStyleOptions options)
        {
            if (!(options?.HasExistingBackgroundImage ?? false))
                return "center";

            if (AnchorPointUtil.IsLeftAligned(style.AnchorPoint))
                return "left";

            if (AnchorPointUtil.IsRightAligned(style.AnchorPoint))
                return "right";

            return "center";
        }

        protected static string GetVerticalAlign(AssStyle style, AssStyleOptions options)
        {
            if (!(options?.HasExistingBackgroundImage ?? false))
                return "middle";

            if (AnchorPointUtil.IsTopAligned(style.AnchorPoint))
                return "top";

            if (AnchorPointUtil.IsBottomAligned(style.AnchorPoint))
                return "bottom";

            return "middle";
        }

        protected static string[] GetFontListContaining(string font)
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

        protected static string ToRgba(Color color)
        {
            return $"rgba({color.R}, {color.G}, {color.B}, {(color.A / 255f).ToString(CultureInfo.InvariantCulture)})";
        }

        protected static string ToHex(Color color)
        {
            return $"#{color.R:X02}{color.G:X02}{color.B:X02}";
        }
    }
}
