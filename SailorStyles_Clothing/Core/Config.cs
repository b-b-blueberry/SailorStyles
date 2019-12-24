using StardewModdingAPI;

namespace SailorStyles_Clothing
{
	class Config
	{
		public SButton debugKey { get; set; }
		public bool debugMode { get; set; }
		public bool debugCate { get; set; }

		public Config()
		{
			debugMode = true;
			debugKey = SButton.J;
			debugCate = true;
		}
	}
}
