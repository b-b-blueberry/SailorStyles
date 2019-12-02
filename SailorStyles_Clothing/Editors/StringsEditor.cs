using StardewModdingAPI;

namespace SailorStyles_Clothing.Editors
{
	class StringsEditor : IAssetEditor
	{
		public bool CanEdit<T>(IAssetInfo asset)
		{
			return asset.AssetNameEquals(@"Strings/StringsFromCSFiles");
		}

		public void Edit<T>(IAssetData asset)
		{
			/*
			var data = asset.AsDictionary<string, string>().Data;
			foreach (var s in Data.Strings)
				data.Add(s.Key, s.Value);
			foreach (var s in Data.StringsCate)
				data.Add(s.Key, s.Value);
				*/
		}
	}
}
