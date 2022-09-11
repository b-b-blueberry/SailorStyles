using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;

namespace SailorStyles.Editors
{
	internal static class NpcManager
	{
        internal static bool TryLoad(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(ModConsts.GameContentCatSchedulePath))
                e.LoadFromModFile<Dictionary<string, string>>(ModConsts.LocalCatSchedulePath + ".json", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo(ModConsts.GameContentCatSpritesPath))
                e.LoadFromModFile<Texture2D>(ModConsts.LocalCatSpritesPath + ".png", AssetLoadPriority.Exclusive);
            else if (e.NameWithoutLocale.IsEquivalentTo(ModConsts.GameContentCatPortraitPath))
                e.LoadFromModFile<Texture2D>(ModConsts.LocalCatPortraitPath + ".png", AssetLoadPriority.Exclusive);
            else
                return false;
            return true;
        }

        internal static bool TryEdit(AssetRequestedEventArgs e, IModContentHelper helper)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(ModConsts.GameContentAnimationsPath))
            {
                e.Edit((asset) => Edit(asset, helper), AssetEditPriority.Default);
                return true;
            }
            return false;
        }

		private static void Edit(IAssetData asset, IModContentHelper helper)
		{
			var json = helper.Load
				<Dictionary<string, string>>
				(ModConsts.LocalAnimationsPath + ".json");
            var data = asset.AsDictionary<string, string>().Data;

            foreach ((string key, string value) in json)
            {
                _ = data.TryAdd(key, value);
            }
        }
	}
}
