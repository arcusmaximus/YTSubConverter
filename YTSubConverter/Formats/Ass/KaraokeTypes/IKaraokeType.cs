using System.Collections.Generic;

namespace Arc.YTSubConverter.Formats.Ass.KaraokeTypes
{
    internal interface IKaraokeType
    {
        IEnumerable<AssLine> Apply(AssKaraokeStepContext context);
    }
}
