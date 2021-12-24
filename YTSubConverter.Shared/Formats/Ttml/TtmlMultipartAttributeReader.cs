using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace YTSubConverter.Shared.Formats.Ttml
{
    internal class TtmlMultipartAttributeReader
    {
        private readonly string[] _parts;
        private int _index;

        public TtmlMultipartAttributeReader(string text)
        {
            _parts = Regex.Matches(text, @"(?:\(.+?\)|\S)+")
                          .Cast<Match>()
                          .Select(m => m.Value)
                          .ToArray();
            _index = 0;
        }

        public int Count => _parts.Length;

        public delegate bool Parser<T>(string part, out T value);

        public bool TryRead<T>(Parser<T> tryParse, out T value)
        {
            if (IsAtEnd)
            {
                value = default;
                return false;
            }

            if (!tryParse(_parts[_index], out value))
                return false;

            _index++;
            return true;
        }

        public bool TryReadEnum<T>(out T value)
            where T : struct
        {
            return TryRead(TryParseEnum, out value);
        }

        private static bool TryParseEnum<T>(string part, out T value)
            where T : struct
        {
            return Enum.TryParse(part, true, out value);
        }

        public void Reset()
        {
            _index = 0;
        }

        public bool IsAtEnd => _index == _parts.Length;
    }
}
