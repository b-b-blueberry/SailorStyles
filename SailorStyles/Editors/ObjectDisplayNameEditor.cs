using StardewModdingAPI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using StardewModdingAPI.Events;
using SailorStyles.Core;

namespace SailorStyles.Editors
{
	public static class ObjectDisplayNameEditor
	{
		private static ITranslationHelper i18n => ModEntry.Instance.Helper.Translation;

        internal static bool TryEdit(AssetRequestedEventArgs e)
        {
            if (ModEntry.JsonAssets is null)
                return false;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/ClothingInformation"))
            {
                e.Edit(
                    (asset) =>
                    {
                        var data = asset.AsDictionary<int, string>().Data;

                        // Add localised names and descriptions for new clothes
                        Dictionary<string, bool> packs = ModConsts.ClothingPacks.ToDictionary(pack => pack, isHat => false);
                        localiseNames(
                            source: ref data, packs: packs,
                            nameIndex: 1, descriptionIndex: 2,
                            packSelector: ModEntry.JsonAssets.GetAllClothingFromContentPack,
                            idSelector: ModEntry.JsonAssets.GetClothingId);
                        asset.AsDictionary<int, string>().ReplaceWith(data);
                    });
                return true;
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data/hats"))
            {
                e.Edit((asset) =>
                {
                    var data = asset.AsDictionary<int, string>().Data;

                    // Add localised names and descriptions for new hats
                    Dictionary<string, bool> packs = ModConsts.HatPacks.ToDictionary(pack => pack, isHat => true);
                    packs.Add("Tuxedo Top Hats", true);
                    localiseNames(
                        source: ref data, packs: packs,
                        nameIndex: 5, descriptionIndex: 1, // JA items inexplicably add a blank field at index 4
                        packSelector: ModEntry.JsonAssets.GetAllHatsFromContentPack,
                        idSelector: ModEntry.JsonAssets.GetHatId);

                    asset.AsDictionary<int, string>().ReplaceWith(data);
                });
                return true;
            }
            return false;
        }
        private static void localiseNames(
            ref IDictionary<int, string> source, Dictionary<string, bool> packs,
            int nameIndex, int descriptionIndex,
            Func<string, List<string>> packSelector, Func<string, int> idSelector)
        {
            var items = packs
                .SelectMany(pack => packSelector(ModEntry.GetIdFromContentPackName(pack.Key, pack.Value)));
            var itemsGrouped = items.ToDictionary((k) => k, (v) => idSelector(v));

            foreach (var (name, id) in itemsGrouped)
            {
                string nameNormalized = name.GetNthChunk('/', 0).ToString().ToLowerInvariant().Replace(" ", "");
                var entry = source[id].Split('/', Math.Max(nameIndex, descriptionIndex) + 2);
                if (entry.Length < Math.Max(nameIndex, descriptionIndex))
                {
                    var preLength = entry.Length;
                    Array.Resize(ref entry, Math.Max(nameIndex, descriptionIndex));
                    for (int i = preLength; i < entry.Length; i++)
                        entry[i] = string.Empty;
                }
                entry[nameIndex] = i18n.Get($"item.{nameNormalized}.name").ToString();
                entry[descriptionIndex] = i18n.Get($"item.{nameNormalized}.description").ToString();
                source[id] = string.Join('/', entry);
            }
        }
	}
}
