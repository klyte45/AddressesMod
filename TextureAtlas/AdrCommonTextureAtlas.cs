using ColossalFramework;
using ColossalFramework.UI;
using Klyte.Addresses.Utils;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.Addresses.TextureAtlas
{
    public class AdrCommonTextureAtlas : TextureAtlasDescriptor<AdrCommonTextureAtlas, AdrResourceLoader>
    {
        protected override string ResourceName => "UI.Images.sprites.png";
        protected override string CommonName => "AddressesSprites";
        protected override string[] SpriteNames => new string[] {
                    "AddressesIcon","AddressesIconSmall","ToolbarIconGroup6Hovered","ToolbarIconGroup6Focused","HelicopterIndicator","RemoveUnwantedIcon","24hLineIcon", "PerHourIcon"
                };
    }
}
