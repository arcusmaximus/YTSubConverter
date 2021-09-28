using System.Collections.Generic;

namespace Arc.YTSubConverter.Shared.Formats.Ass.KaraokeTypes
{
    public interface IKaraokeType
    {
        IEnumerable<AssLine> Apply(AssKaraokeStepContext context);
    }
}
