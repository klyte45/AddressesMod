using ColossalFramework.Globalization;
using Klyte.Addresses.Utils;
using Klyte.Commons.i18n;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Klyte.Addresses.i18n
{
    public sealed class AdrLocaleUtils : KlyteLocaleUtils<AdrLocaleUtils, AdrResourceLoader>
    {
        public override string prefix => "ADR_";

        protected override string packagePrefix => "Klyte.Addresses";
    }
}
