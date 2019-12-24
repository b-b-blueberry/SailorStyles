﻿using System.IO;

namespace SailorStyles_Clothing
{
	class Data
	{
		// files
		internal const string CatID = "zss_cat";
		internal const string ShopDialogueRoot = "catshop.text.";
		internal const string ImgExt = ".png";
		internal const string AssetsDir = "Assets";
		internal static readonly string JAObjDir = Path.Combine(Data.AssetsDir, "Objects");

		// keys
		internal const string LocationTarget = "Forest";
		//internal const string ExtraLayerID = "CatShop_Buildings";

		// values
		internal const int CatShopQuantity = 25;
		internal const int CateRate = 100;
		internal const int ClothingCost = 50;
	}
}
