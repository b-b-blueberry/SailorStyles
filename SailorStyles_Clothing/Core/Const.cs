using System.Collections.Generic;
using System.IO;

namespace SailorStyles_Clothing
{
	internal class Const
	{
		// files
		internal const string CatId = "zss_cat";
		internal const string ShopDialogueRoot = "catshop.text.";
		internal const string ImgExt = ".png";
		internal const string JsonExt = ".json";
		
		internal const string CatDir = "Cat";
		internal const string HatsDir = "Hats";
		internal const string ClothingDir = "Clothing";

		internal const string ContentPackPrefix = "SailorStyles Clothing:";

		internal static readonly List<string> HatPacks = new List<string> {
			"Hats and Pieces"
		};

		internal static readonly List<string> ClothingPacks = new List<string> {
			"Everyday Heroes",
			"Sailor Suits",
			"Uniform Operation",
		};

		internal static readonly string AnimDescs = Path.Combine("Data", "animationDescriptions");
		internal static readonly string CatSchedule = Path.Combine("Characters", "schedules", CatId);

		internal static readonly string CatTiles = Path.Combine("Assets", CatDir, CatId + "_tilesheet");
		internal static readonly string CatSprite = Path.Combine("Characters", CatId);
		internal static readonly string CatPortrait = Path.Combine("Portraits", CatId);

		// keys
		internal const string LocationTarget = "Forest";

		// values
		internal const int CatShopQtyRatio = 5;
		internal const int ClothingCost = 50;

		internal const int CatX = 33;
		internal const int CatY = 96;
	}
}
