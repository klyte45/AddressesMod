using Klyte.Addresses.Utils;
using Klyte.Commons.Interfaces;
using static Klyte.Commons.TextureAtlas.CommonTextureAtlas;

namespace Klyte.Addresses.TextureAtlas
{
    public class AdrCommonTextureAtlas : TextureAtlasDescriptor<AdrCommonTextureAtlas, AdrResourceLoader, SpriteNames>
    {
        protected override string ResourceName => "UI.Images.sprites.png";
        protected override string CommonName => "AddressesSprites";

        private enum SpriteNames
        {
            AddressesIcon, AddressesIconSmall, ToolbarIconGroup6Hovered, ToolbarIconGroup6Focused, HelicopterIndicator, RemoveUnwantedIcon, Icon24hLine, PerHourIcon
        };
    }
}
