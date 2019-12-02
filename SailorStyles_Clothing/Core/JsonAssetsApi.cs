using System.Collections.Generic;

namespace SailorStyles_Clothing
{
	public interface JsonAssetsApi
	{
		void LoadAssets(string path);

		int GetObjectId(string name);
		IDictionary<string, int> GetAllObjectIds();
	}
}
