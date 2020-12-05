namespace Arc.YTSubConverter.Util
{
    public class CharacterRange : Range<char>
    {
        public static readonly CharacterRange ArabicRange = new CharacterRange((char)0x600, (char)0x6FF);
        public static readonly CharacterRange HebrewRange = new CharacterRange((char)0x590, (char)0x5FF);

        public static readonly CharacterRange HiraganaRange = new CharacterRange((char)0x3041, (char)0x3097);
        public static readonly CharacterRange KatakanaRange = new CharacterRange((char)0x30A0, (char)0x3100);
        public static readonly CharacterRange IdeographExtensionRange = new CharacterRange((char)0x3400, (char)0x4DB6);
        public static readonly CharacterRange IdeographRange = new CharacterRange((char)0x4E00, (char)0x9FCC);
        public static readonly CharacterRange IdeographCompatibilityRange = new CharacterRange((char)0xF900, (char)0xFA6B);
        public static readonly CharacterRange HangulRange = new CharacterRange((char)0xAC00, (char)0xD7A4);

        public CharacterRange(char start, char end)
            : base(start, end)
        {
        }

        public static bool IsRightToLeft(char c)
        {
            return ArabicRange.Contains(c) || HebrewRange.Contains(c);
        }
    }
}
