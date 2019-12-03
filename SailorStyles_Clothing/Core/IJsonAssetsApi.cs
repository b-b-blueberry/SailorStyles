using System.Collections.Generic;

namespace SailorStyles_Clothing
{
	public interface IJsonAssetsApi
	{
		void LoadAssets(string path);

		int GetClothingId(string name);
		IDictionary<string, int> GetAllClothingIds();
	}
}
