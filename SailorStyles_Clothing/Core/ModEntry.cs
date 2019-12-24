using System;
using System.Collections.Generic;
using System.IO;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using xTile.Dimensions;
using xTile.ObjectModel;

namespace SailorStyles_Clothing
{
	public class ModEntry : Mod
	{
		internal static Config SConfig;
		internal static IModHelper SHelper;
		internal static IMonitor SMonitor;
		internal static ITranslationHelper i18n => SHelper.Translation;
		
		private static IJsonAssetsApi JsonAssets;
		
		private NPC CatNPC;
		private NPC CateNPC;
		internal static bool cate;

		private Dictionary<ISalable, int[]> CatShopStock;

		public override void Entry(IModHelper helper)
		{
			SConfig = helper.ReadConfig<Config>();
			SHelper = helper;
			SMonitor = Monitor;

			helper.Events.Input.ButtonReleased += OnButtonReleased;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			helper.Events.Player.Warped += OnWarped;

			helper.Content.AssetEditors.Add(new Editors.MapEditor(helper, Monitor, SConfig.debugMode));
		}
		
		private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
		{
			e.Button.TryGetKeyboard(out Keys keyPressed);

			if (Game1.activeClickableMenu == null && !Game1.player.UsingTool && !Game1.pickingTool && !Game1.menuUp
				&& (!Game1.eventUp || Game1.currentLocation.currentEvent.playerControlSequence) && !Game1.nameSelectUp
				&& Game1.numberOfSelectedItems == -1)
			{
				if (e.Button.IsActionButton())
				{
					// thanks sundrop
					var grabTile = new Vector2(
						Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;
					if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
						grabTile = Game1.player.GetGrabTile();
					var tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location(
						(int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize), Game1.viewport.Size);
					PropertyValue action = null;
					tile?.Properties.TryGetValue("Action", out action);
					if (action != null)
					{
						string[] strArray = ((string)action).Split(' ');
						string[] args = new string[strArray.Length - 1];
						Array.Copy(strArray, 1, args, 0, args.Length);
						if (strArray[0].Equals(Data.CatID))
							CatShop();
					}
				}

				// debug junk
				if (keyPressed.ToSButton().Equals(SConfig.debugWarpKey) && SConfig.debugMode)
				{
					DebugWarpPlayer();
				}
			}
		}

		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			CatShopStock = new Dictionary<ISalable, int[]>();

			JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
			if (JsonAssets == null)
			{
				Monitor.Log("Can't access the Json Assets API. Is the mod installed correctly?",
					LogLevel.Error);
				return;
			}
			
			var objFolder = new DirectoryInfo(Path.Combine(Helper.DirectoryPath, Data.JAShirtsDir));
			foreach (var subfolder in objFolder.GetDirectories())
				JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, Data.JAShirtsDir, subfolder.Name));

			objFolder = new DirectoryInfo(Path.Combine(Helper.DirectoryPath, Data.JAHatsDir));
			foreach (var subfolder in objFolder.GetDirectories())
				JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, Data.JAHatsDir, subfolder.Name));
		}
		
		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			CatShopRestock();

			// mmmmswsswsss
			var random = new Random();
			var randint = random.Next(Data.CateRate);
			cate = randint == 0 || (SConfig.debugMode && SConfig.debugCate);
			Monitor.Log("CateRate: " + randint + "/" + Data.CateRate + ", " + cate.ToString(),
				SConfig.debugMode ? LogLevel.Debug : LogLevel.Trace);
		}
		
		private void OnWarped(object sender, WarpedEventArgs e)
		{
			ResetLocation(e);
		}

		private void ResetLocation(WarpedEventArgs e)
		{
			if (e.OldLocation.Name.Equals(Data.LocationTarget))
			{
				RemoveNPCs();
			}

			if (e.NewLocation.Name.Equals(Data.LocationTarget))
			{
				if (CatNPC == null && (Game1.dayOfMonth % 7 <= 1 || SConfig.debugMode))
					AddNPCs();

				Monitor.Log("Invalidating cache for map file " + Data.LocationTarget,
					SConfig.debugMode ? LogLevel.Debug : LogLevel.Trace);
				Helper.Content.InvalidateCache(Path.Combine("Maps", Data.LocationTarget));
			}
		}

		private void RemoveNPCs()
		{
			Monitor.Log("Removing NPCs at " + Data.LocationTarget,
				SConfig.debugMode ? LogLevel.Debug : LogLevel.Trace);

			CatNPC = null;
			CateNPC = null;
		}

		private void AddNPCs()
		{
			Monitor.Log("Adding NPCs for " + Data.LocationTarget,
				LogLevel.Trace);
			
			CatNPC = new NPC(
				new AnimatedSprite("Characters\\Bouncer", 0, 16, 32),
				new Vector2(-64000f, 128f),
				Data.LocationTarget,
				2,
				Data.CatID,
				false,
				null,
				Helper.Content.Load<Texture2D>(
					Path.Combine(Data.AssetsDir, Data.CatID + "_arte" + Data.ImgExt)));

			// ahahaha
			CateNPC = new NPC(
				new AnimatedSprite("Characters\\Bouncer", 0, 64, 128),
				new Vector2(-64000f, 128f),
				Data.LocationTarget,
				2,
				Data.CatID + "e",
				false,
				null,
				Helper.Content.Load<Texture2D>(
					Path.Combine(Data.AssetsDir, Data.CatID + "_cate" + Data.ImgExt)));
		}

		private void CatShopRestock()
		{
			CatShopStock.Clear();

			Monitor.Log("JA Hat IDs:", LogLevel.Debug);
			foreach (var id in JsonAssets.GetAllHatIds())
				Monitor.Log($"{id.Key}: {id.Value}", LogLevel.Debug);

			PopulateShop(Data.JAHatsDir, 0);

			Monitor.Log("JA Shirt IDs:", LogLevel.Debug);
			foreach (var id in JsonAssets.GetAllClothingIds())
				Monitor.Log($"{id.Key}: {id.Value}", LogLevel.Debug);

			PopulateShop(Data.JAShirtsDir, 1);
		}
		
		private void PopulateShop(string dir, int type)
		{
			try
			{
				var stock = new List<int>();
				var random = new Random();

				var objFolder = new DirectoryInfo(Path.Combine(Helper.DirectoryPath, dir));
				var firstFolder = objFolder.GetDirectories()[0].GetDirectories()[0].GetDirectories()[0];
				var lastFolder = objFolder.GetDirectories()[objFolder.GetDirectories().Length - 1];
				lastFolder = lastFolder.GetDirectories()[0].GetDirectories()[lastFolder.GetDirectories()[0]
					.GetDirectories().Length - 1];

				Monitor.Log($"CatShop first object: {firstFolder.Name}",
					SConfig.debugMode ? LogLevel.Debug : LogLevel.Trace);
				Monitor.Log($"CatShop last object: {lastFolder.Name}",
					SConfig.debugMode ? LogLevel.Debug : LogLevel.Trace);

				var firstObject = 0;
				var lastObject = 0;

				switch(type)
				{
					case 0:
						firstObject = JsonAssets.GetHatId(firstFolder.Name);
						lastObject = JsonAssets.GetHatId(lastFolder.Name);
						break;
					case 1:
						firstObject = JsonAssets.GetClothingId(firstFolder.Name);
						lastObject = JsonAssets.GetClothingId(lastFolder.Name);
						break;
				}

				var goalQty = (lastObject - firstObject) / Data.CatShopQtyRatio;

				Monitor.Log("CatShop Restock bounds:",
					SConfig.debugMode ? LogLevel.Debug : LogLevel.Trace);
				Monitor.Log("index: " + firstObject + ", end: " + lastObject + ", goalQty: " + goalQty,
					SConfig.debugMode ? LogLevel.Debug : LogLevel.Trace);

				while (stock.Count < goalQty)
				{
					var id = random.Next(firstObject, lastObject);
					if (!stock.Contains(id))
						stock.Add(id);
				}

				foreach (var id in stock)
				{
					switch (type)
					{
						case 0:
							CatShopStock.Add(
								new StardewValley.Objects.Hat(id), new int[2]
								{ Data.ClothingCost, 1 });
							break;
						case 1:
							CatShopStock.Add(
								new StardewValley.Objects.Clothing(id), new int[2]
								{ Data.ClothingCost, 1 });
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log("Sailor Styles failed to populate the clothes shop."
					+ "Did you remove all the clothing folders, or did I do something wrong?",
					LogLevel.Error);
				Monitor.Log("Exception logged:\n" + ex,
					LogLevel.Error);
			}
		}

		private void CatShop()
		{
			Game1.playSound("cat");

			var name = Data.CatID;
			var text = (string) null;

			if (!cate)
			{
				var random = new Random((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
				var whichDialogue = Data.ShopDialogueRoot + random.Next(5);
				if (whichDialogue.EndsWith("5"))
					whichDialogue += $".{Game1.currentSeason}";
				text = i18n.Get(whichDialogue);
			}
			else
			{
				// bllblblbl
				name += "e";
				text = i18n.Get(Data.ShopDialogueRoot + "cate");
			}
			
			Game1.activeClickableMenu = new ShopMenu(CatShopStock);
			((ShopMenu)Game1.activeClickableMenu).portraitPerson
				= cate ? CateNPC : CatNPC;
			((ShopMenu)Game1.activeClickableMenu).potraitPersonDialogue
				= Game1.parseText(text, Game1.dialogueFont, 304);
		}

		private void DebugWarpPlayer()
		{
			Game1.warpFarmer(Data.LocationTarget, 31, 97, 2);
			Monitor.Log($"Warped {Game1.player.Name} to the CatShop.",
				LogLevel.Debug);
		}
	}
}
