using Klyte.Commons.Utils;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Utils
{
    public sealed class AdrResourceLoader : KlyteResourceLoader<AdrResourceLoader>
    {
        protected override string prefix => "Klyte.Addresses.";
    }
}
