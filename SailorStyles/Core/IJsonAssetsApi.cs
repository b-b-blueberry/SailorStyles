﻿using System;
using System.Collections.Generic;

namespace SailorStyles
{
	public interface IJsonAssetsApi
	{
		void LoadAssets(string path);

		int GetHatId(string name);
		int GetClothingId(string name);
		IDictionary<string, int> GetAllHatIds();
		IDictionary<string, int> GetAllClothingIds();
		List<string> GetAllHatsFromContentPack(string cp);
		List<string> GetAllClothingFromContentPack(string cp);

        /// <summary>
        /// Raised when JA tries to fix IDs.
        /// </summary>
        event EventHandler IdsFixed;
    }
}
