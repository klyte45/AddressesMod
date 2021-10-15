using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.Addresses.LocaleStruct
{
    internal struct FootballTeamData
    {
        public FootballTeamData(string input)
        {
            try
            {
                var data = input.Split('=');
                Name = data[0];
                Color = ColorExtensions.FromRGB(data[1]);
            }
            catch
            {
                Name = input;
                Color = default;
            }
        }

        public Color Color { get; }
        public string Name { get; }
    }

}
