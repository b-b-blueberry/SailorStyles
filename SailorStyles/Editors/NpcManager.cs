using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;

namespace SailorStyles.Editors
{
	internal class NpcManager : IAssetLoader, IAssetEditor
	{
		private IModHelper Helper => ModEntry.Instance.Helper;

		public bool CanLoad<T>(IAssetInfo asset)
		{
			return asset.AssetNameEquals(ModConsts.GameContentCatSchedulePath) 
				|| asset.AssetNameEquals(ModConsts.GameContentCatSpritesPath) 
				|| asset.AssetNameEquals(ModConsts.GameContentCatPortraitPath);
		}

		public T Load<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals(ModConsts.GameContentCatSchedulePath))
				return (T)(object) Helper.Content.Load
					<Dictionary<string, string>>
					(ModConsts.LocalCatSchedulePath + ".json");
			if (asset.AssetNameEquals(ModConsts.GameContentCatSpritesPath))
				return (T) (object) Helper.Content.Load
					<Texture2D>
					(ModConsts.LocalCatSpritesPath + ".png");
			if (asset.AssetNameEquals(ModConsts.GameContentCatPortraitPath))
				return (T) (object) Helper.Content.Load
					<Texture2D>
					(ModConsts.LocalCatPortraitPath + ".png");
			return (T) (object) null;
		}

		public bool CanEdit<T>(IAssetInfo asset)
		{
			return asset.AssetNameEquals(ModConsts.GameContentAnimationsPath);
		}

		public void Edit<T>(IAssetData asset)
		{
			var json = Helper.Content.Load
				<Dictionary<string, string>>
				(ModConsts.LocalAnimationsPath + ".json");
			foreach (var pair in json)
				if (!asset.AsDictionary<string, string>().Data.ContainsKey(pair.Key))
					asset.AsDictionary<string, string>().Data.Add(pair);
		}
	}
}
