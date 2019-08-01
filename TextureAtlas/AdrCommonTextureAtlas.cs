using Klyte.Addresses.Utils;
using Klyte.Commons.Interfaces;

namespace Klyte.Addresses.TextureAtlas
{
    public class AdrCommonTextureAtlas : TextureAtlasDescriptor<AdrCommonTextureAtlas, AdrResourceLoader, AdrCommonTextureAtlas.SpriteNames>
    {
        protected override string ResourceName => "UI.Images.sprites.png";
        protected override string CommonName => "AddressesSprites";

        public enum SpriteNames
        {
            AddressesIcon
        };
    }
}
