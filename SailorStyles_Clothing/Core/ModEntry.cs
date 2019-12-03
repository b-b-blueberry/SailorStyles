using System;
using System.Collections.Generic;
using System.IO;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

using PyTK.Extensions;
using PyTK.Types;

using Harmony;  // el diavolo

namespace SailorStyles_Clothing
{
	public class ModEntry : Mod
	{
		internal static Config config;
		internal static IModHelper SHelper;
		internal static IMonitor SMonitor;
		internal static ITranslationHelper i18n => SHelper.Translation;

		private static IJsonAssetsApi ja;

		private const string LocationTarget = "Forest";
		private const string ExtraLayerID = "CatShop_Buildings";
		internal static bool cate;

		public override void Entry(IModHelper helper)
		{
			config = helper.ReadConfig<Config>();
			SHelper = helper;
			SMonitor = Monitor;

			helper.Events.Input.ButtonReleased += OnButtonReleased;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			//helper.Events.Player.Warped += OnWarped;
			
			var harmony = HarmonyInstance.Create("blueberry.SailorStyles_Shirts_Merchant");

			harmony.Patch(
				original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.setUpShopOwner)),
				prefix: new HarmonyMethod(typeof(ShopMenuPatch), nameof(ShopMenuPatch.Prefix)));
		}

		private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
		{
			if (Game1.activeClickableMenu == null && !Game1.player.UsingTool && !Game1.pickingTool && !Game1.menuUp
				&& (!Game1.eventUp || Game1.currentLocation.currentEvent.playerControlSequence) && !Game1.nameSelectUp
				&& Game1.numberOfSelectedItems == -1)
			{
				if (e.Button.IsActionButton())
				{
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
			}
		}

		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			ja = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
			if (ja == null)
			{
				Monitor.Log("Can't access the Json Assets API. Is the mod installed correctly?",
					LogLevel.Error);
				return;
			}
			ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "Objects", "SailorSuits"));
			ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "Objects", "EverydayHeroes"));
			ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "Objects", "UniformOperation"));
		}

		private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			PrepareLocation();
		}

		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			Monitor.Log("JA Objects: ", LogLevel.Debug);
			Monitor.Log(ja.GetAllClothingIds().Count.ToString(), LogLevel.Debug);
			foreach (var item in ja.GetAllClothingIds())
			{
				Monitor.Log("Loaded " + item.Key, LogLevel.Debug);
			}
			var itemId = ja.GetClothingId("Sailor Moon");
			Game1.clothingInformation.TryGetValue(itemId, out var itemTest);
			Monitor.Log(itemTest, LogLevel.Debug);
			/*
			foreach(var item in ja.GetAllClothingIds())
			{
				var packs = Helper.ContentPacks.GetOwned();
				foreach (var pack in packs)
				{
					//pack.
				}
			}
			*/
			DebugWarpPlayer();

			var random = new Random();
			cate = (random.Next(50) == 0);
		}
		
		private void OnWarped(object sender, WarpedEventArgs e)
		{
			//if (Game1.dayOfMonth % 7 <= 1 && e.NewLocation.Name.Equals(LocationTarget) && e.IsLocalPlayer)
			if (e.NewLocation.Name.Equals(LocationTarget) && e.IsLocalPlayer)
			{
				e.OldLocation.Map.GetLayer("Buildings").AfterDraw -= DrawOverBuildings;
				e.NewLocation.Map.GetLayer("Buildings").AfterDraw += DrawOverBuildings;
			}
		}

		private void DebugWarpPlayer()
		{
			Game1.warpFarmer(LocationTarget, 31, 97, 2);
		}

		private void CatShop()
		{
			Game1.currentLocation.playSound("cat");
			Game1.activeClickableMenu = new ShopMenu(
				CatShopStock(), 0, Data.CatID, null, null, null);
		}

		private Dictionary<ISalable, int[]> CatShopStock()
		{
			var stock = new Dictionary<ISalable, int[]>();

			// todo add json assets objects
			Utility.AddStock(stock, null, -1, 1);

			return stock;
		}
		
		private void DrawOverBuildings(object s, LayerEventArgs e)
		{
			Game1.currentLocation.map.GetLayer(ExtraLayerID)?.Draw(
				Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
		}
		
		private void PrepareLocation()
		{
			var loc = Game1.getLocationFromName(LocationTarget);
			AddTilesheet(loc.Map);
			//AddLayers(loc.Map);
			AddTiles(loc.Map);
			AddNPCs(loc);
		}
		
		private void AddTilesheet(Map map)
		{
			var path = Helper.Content.GetActualAssetKey(
				Path.Combine("Assets", Data.CatID + "_tilesheet" + Data.ImgExt));

			var texture = Helper.Content.Load<Texture2D>(path);
			var sheet = new TileSheet(
				Data.CatID, map, path, new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));

			map.AddTileSheet(sheet);
			map.LoadTileSheets(Game1.mapDisplayDevice);
		}

		private void AddLayers(Map map)
		{
			var layer = new Layer(
				ExtraLayerID, map, new Size(map.DisplayWidth / 64, map.DisplayHeight / 64), new Size(16, 16));
			layer.Properties.Add("DrawAbove", "Buildings");
			map.AddLayer(layer);
		}

		private void AddTiles(Map map)
		{
			var sheet = map.GetTileSheet(Data.CatID);
			var layer = map.GetLayer("Front");
			var tiles = layer.Tiles;
			var mode = BlendMode.Additive;
			
			tiles[30, 94] = new StaticTile(layer, sheet, mode, 28);
			tiles[31, 94] = new StaticTile(layer, sheet, mode, 29);
			tiles[32, 94] = new StaticTile(layer, sheet, mode, 30);
			
			layer = map.GetLayer("Buildings");
			tiles = layer.Tiles;
			tiles[31, 95].Properties.Add("Action", new PropertyValue(Data.CatID));
			
			layer = map.GetLayer(ExtraLayerID);

			if (layer == null)
				return;

			// broken
			tiles = layer.Tiles;

			if (!cate)
			{
				tiles[30, 94] = new StaticTile(layer, sheet, mode, 29);
				tiles[30, 95] = new StaticTile(layer, sheet, mode, 29 + sheet.SheetWidth);

				if (Game1.timeOfDay < 1300)
				{
					tiles[31, 94] = new StaticTile(layer, sheet, mode, 0);
					tiles[31, 95] = new StaticTile(layer, sheet, mode, 0 + sheet.SheetWidth);
				}
				else if (Game1.timeOfDay < 2100)
				{
					tiles[31, 94] = new AnimatedTile(layer, new StaticTile[]{
						new StaticTile(layer, sheet, mode, 1),
						new StaticTile(layer, sheet, mode, 2)
						}, 10000);
					tiles[31, 95] = new AnimatedTile(layer, new StaticTile[]{
						new StaticTile(layer, sheet, mode, 1 + sheet.SheetWidth),
						new StaticTile(layer, sheet, mode, 2 + sheet.SheetWidth)
						}, 10000);
				}
				else
				{
					tiles[31, 94] = new StaticTile(layer, sheet, mode, 6);
					tiles[31, 95] = new StaticTile(layer, sheet, mode, 6 + sheet.SheetWidth);
				}
			}
			else
			{
				// eeeewwsws
				if (Game1.timeOfDay < 2100)
				{
					tiles[30, 95] = new StaticTile(layer, sheet, mode, 15);
					tiles[31, 94] = new StaticTile(layer, sheet, mode, 14 + sheet.SheetWidth);
					tiles[31, 95] = new StaticTile(layer, sheet, mode, 15 + sheet.SheetWidth);
				}
				else
				{
					tiles[30, 95] = new StaticTile(layer, sheet, mode, 17);
					tiles[31, 94] = new StaticTile(layer, sheet, mode, 16 + sheet.SheetWidth);
					tiles[31, 95] = new StaticTile(layer, sheet, mode, 17 + sheet.SheetWidth);
				}
			}
		}

		private void AddNPCs(GameLocation loc)
		{
			loc.addCharacter(new NPC(
				new AnimatedSprite("Characters\\Bouncer", 0, 16, 32),
				new Vector2(-64000f, 128f),
				LocationTarget,
				2,
				Data.CatID,
				false,
				null,
				Helper.Content.Load<Texture2D>(Path.Combine("Assets", Data.CatID + "_arte" + Data.ImgExt))));
			
			// ahahaha
			loc.addCharacter(new NPC(
				new AnimatedSprite("Characters\\Bouncer", 0, 64, 128),
				new Vector2(-64000f, 128f),
				LocationTarget,
				2,
				Data.CatID + "e",
				false,
				null,
				Helper.Content.Load<Texture2D>(Path.Combine("Assets", Data.CatID + "_cate" + Data.ImgExt))));
		}
	}

	class ShopMenuPatch
	{
		internal static bool Prefix(string ___potraitPersonDialogue, NPC ___portraitPerson, string who)
		{
			if (who.Equals(Data.CatID))
			{
				var text = (string)null;
				var name = Data.CatID;

				if (!ModEntry.cate)
				{
					var random = new Random((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
					text = ModEntry.i18n.Get(Data.StringKey + random.Next(5));
				}
				else
				{
					// bllblblbl
					name += "e";
					text = ModEntry.i18n.Get(Data.StringKey + "cate");
				}

				___potraitPersonDialogue = Game1.parseText(text, Game1.dialogueFont, 304);
				___portraitPerson = Game1.getCharacterFromName(name, false);
				return false;
			}
			return true;
		}
	}
}
