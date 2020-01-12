using StardewModdingAPI;

namespace SailorStyles_Clothing
{
	internal class Config
	{
		public SButton DebugWarpKey { get; set; }
		public bool DebugMode { get; set; }
		public bool DebugCate { get; set; }

		public Config()
		{
			DebugMode = false;
			DebugWarpKey = SButton.U;
			DebugCate = true;
		}
	}
}
