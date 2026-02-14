using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Constants
{
    public class MatchOnTypeBlackList
    {
        public static readonly List<Type> BlackList = new List<Type> { typeof(FileAttributeMetadata), typeof(ImageAttributeMetadata) };
    }
}
