using StardewModdingAPI;

namespace SailorStyles_Clothing
{
	class Config
	{
		public SButton debugKey { get; set; }
		public bool debugMode { get; set; }

		public Config()
		{
			debugMode = true;
			debugKey = SButton.J;
		}
	}
}
