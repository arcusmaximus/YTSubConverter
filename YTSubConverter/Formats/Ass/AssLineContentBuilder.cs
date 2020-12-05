using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Arc.YTSubConverter.Formats.Ass
{
    internal class AssLineContentBuilder
    {
        private readonly StringBuilder _pendingTags = new StringBuilder();
        private readonly StringBuilder _content = new StringBuilder();

        public void AppendTag(string tagName, params object[] args)
        {
            _pendingTags.Append("\\");
            _pendingTags.Append(tagName);
            if (args.Length > 1)
                _pendingTags.Append("(");

            bool first = true;
            foreach (object arg in args)
            {
                if (first)
                    first = false;
                else
                    _pendingTags.Append(",");

                AppendTagArgument(tagName, arg);
            }

            if (args.Length > 1)
                _pendingTags.Append(")");
        }

        private void AppendTagArgument(string tagName, object arg)
        {
            if (arg == null)
                return;

            switch (arg)
            {
                case bool boolValue:
                    _pendingTags.Append(boolValue ? "1" : "0");
                    break;

                case int intValue:
                    if (HasHexadecimalArgument(tagName))
                        _pendingTags.Append($"&H{intValue:X}&");
                    else
                        _pendingTags.Append(intValue);

                    break;

                case float floatValue:
                    _pendingTags.Append(floatValue.ToString(CultureInfo.InvariantCulture));
                    break;

                case string stringValue:
                    _pendingTags.Append(stringValue);
                    break;

                case Color colorValue:
                    _pendingTags.Append($"&H{colorValue.B:X02}{colorValue.G:X02}{colorValue.R:X02}&");
                    break;

                case AssStyle style:
                    _pendingTags.Append(style.Name);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        public void AppendText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            FlushTags();

            text = Regex.Replace(text, @"\\([nNh])", "\\{}$1");
            text = Regex.Replace(text, @"\r\n", "\\N");
            text = text.Replace("\xA0", "\\h");
            _content.Append(text);
        }

        private void FlushTags()
        {
            if (_pendingTags.Length == 0)
                return;

            _content.Append("{");
            _content.Append(_pendingTags);
            _content.Append("}");
            _pendingTags.Clear();
        }

        public override string ToString()
        {
            FlushTags();
            return _content.ToString();
        }

        private static bool HasHexadecimalArgument(string tagName)
        {
            switch (tagName)
            {
                case "1a":
                case "2a":
                case "3a":
                case "4a":
                    return true;

                default:
                    return false;
            }
        }
    }
}
