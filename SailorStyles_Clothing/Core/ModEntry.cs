using System;
using System.Collections.Generic;
using System.IO;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using xTile.Dimensions;
using xTile.ObjectModel;

/*
 * authors note
 * update all the manifest and content-pack files u idiot
 */

/*
 * todo
 *
 * find a way to bring back my blessed cate
 * remove pants before release
 *
 */

namespace SailorStyles_Clothing
{
	public class ModEntry : Mod
	{
		internal static ModEntry Instance;

		internal Config Config;
		internal ITranslationHelper i18n => Helper.Translation;
		
		private static IJsonAssetsApi _jsonAssets;
		
		private NPC _catNpc;
		//private NPC _cateNpc;
		//internal static bool Cate;

		private Dictionary<ISalable, int[]> _catShopStock;

		public override void Entry(IModHelper helper)
		{
			Instance = this;
			Config = helper.ReadConfig<Config>();

			helper.Events.Input.ButtonReleased += OnButtonReleased;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			helper.Events.Player.Warped += OnWarped;

			helper.Content.AssetEditors.Add(new Editors.AnimDescEditor(helper));
			helper.Content.AssetLoaders.Add(new Editors.CustomNpcLoader(helper));
			helper.Content.AssetEditors.Add(new Editors.MapEditor(helper));
		}
		
		private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
		{
			e.Button.TryGetKeyboard(out var keyPressed);

			if (Game1.activeClickableMenu != null || Game1.player.UsingTool || Game1.pickingTool || Game1.menuUp
			    || (Game1.eventUp && !Game1.currentLocation.currentEvent.playerControlSequence)
			    || Game1.nameSelectUp ||Game1.numberOfSelectedItems != -1)
				return;

			if (e.Button.IsActionButton())
			{
				// thanks sundrop
				var grabTile = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y)
				               / Game1.tileSize;
				if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
					grabTile = Game1.player.GetGrabTile();
				var tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location(
					(int)grabTile.X * Game1.tileSize, (int)grabTile.Y * Game1.tileSize), Game1.viewport.Size);
				var action = (PropertyValue) null;
				tile?.Properties.TryGetValue("Action", out action);
				if (action != null)
				{
					var strArray = ((string)action).Split(' ');
					var args = new string[strArray.Length - 1];
					Array.Copy(strArray, 1, args, 0, args.Length);
					if (strArray[0].Equals(Const.CatId))
						CatShop();
				}
			}

			// debug junk
			if (keyPressed.ToSButton().Equals(Config.DebugWarpKey) && Config.DebugMode)
			{
				DebugWarpPlayer();
			}
		}

		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			_catShopStock = new Dictionary<ISalable, int[]>();

			_jsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
			if (_jsonAssets == null)
			{
				Log.E("Can't access the Json Assets API. Is the mod installed correctly?");
				return;
			}
			
			var objFolder = new DirectoryInfo(Path.Combine(Helper.DirectoryPath, Const.JsonShirtsDir));
			foreach (var subfolder in objFolder.GetDirectories())
				_jsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, Const.JsonShirtsDir, subfolder.Name));

			objFolder = new DirectoryInfo(Path.Combine(Helper.DirectoryPath, Const.JsonHatsDir));
			foreach (var subfolder in objFolder.GetDirectories())
				_jsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, Const.JsonHatsDir, subfolder.Name));
		}
		
		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			CatShopRestock();

			// mmmmswsswsss
			/*
			var random = new Random();
			var randint = random.Next(Const.CateRate);
			Cate = randint == 0 || (Config.DebugMode && Config.DebugCate);
			Log.D("CateRate: " + randint + "/" + Const.CateRate + ", " + Cate,
				Config.DebugMode);
			*/
		}
		
		private void OnWarped(object sender, WarpedEventArgs e)
		{
			ResetLocation(e);
		}

		private void ResetLocation(WarpedEventArgs e)
		{
			Log.D($"Resetting location and NPC data ({Const.LocationTarget})",
				Config.DebugMode);
			
			Helper.Content.InvalidateCache(Const.AnimDescs);

			if (e.OldLocation.Name.Equals(Const.LocationTarget))
				RemoveNpcs();

			if (!e.NewLocation.Name.Equals(Const.LocationTarget))
				return;

			if (_catNpc == null && (Game1.dayOfMonth % 7 <= 1 || Config.DebugMode))
				AddNpcs();

			Helper.Content.InvalidateCache(Path.Combine("Maps", Const.LocationTarget));
		}

		private void RemoveNpcs()
		{
			Log.D($"Removing NPCs from {Const.LocationTarget}.",
				Config.DebugMode);

			Game1.getLocationFromName(Const.LocationTarget).characters.Remove(_catNpc);
			_catNpc = null;
			/*
			Game1.getLocationFromName(Const.LocationTarget).characters.Remove(_cateNpc);
			_cateNpc = null;
			*/
		}

		private void AddNpcs()
		{
			Log.D($"Adding NPCs to {Const.LocationTarget}.",
				Config.DebugMode);

			_catNpc = new NPC(
				new AnimatedSprite(Const.CatSprite, 0, 16, 32),
				new Vector2(Const.CatX, Const.CatY) * 64.0f,
				Const.LocationTarget,
				2,
				Const.CatId,
				false,
				null,
				Helper.Content.Load<Texture2D>($@"Portraits/{Const.CatId}",
					ContentSource.GameContent));
			LoadNpcSchedule(_catNpc);
			Game1.getLocationFromName(Const.LocationTarget).addCharacter(_catNpc);
			/*
			Log.D($"Cat name     : {_catNpc.Name}");
			Log.D($"Cat position : {_catNpc.position.X}, {_catNpc.position.Y}");
			Log.D($"Cat texture  : {Const.CatSprite}");

			Log.D("Cat schedule : ");
			if (_catNpc.Schedule != null)
				foreach (var entry in _catNpc.Schedule)
					Log.D($"{entry.Key}: {entry.Value.endOfRouteBehavior}");
			else
				Log.D("null");
			*/
			// ahahaha
			/*
			_cateNpc = new NPC(
				new AnimatedSprite("Characters\\Bouncer", 0, 64, 128),
				new Vector2(-64000f, 128f),
				Const.LocationTarget,
				2,
				Const.CatId + "e",
				false,
				null,
				Helper.Content.Load<Texture2D>(Const.CatePortrait));
			*/
		}

		private void CatShopRestock()
		{
			_catShopStock.Clear();
			/*
			Log.D("JA Hat IDs:",
				Config.DebugMode);
			foreach (var id in JsonAssets.GetAllHatIds())
				Log.D($"{id.Key}: {id.Value}",
					Config.DebugMode);
			*/
			PopulateShop(Const.JsonHatsDir, 0);
			/*
			Log.D("JA Shirt IDs:",
				Config.DebugMode);
			foreach (var id in JsonAssets.GetAllClothingIds())
				Log.D($"{id.Key}: {id.Value}",
					Config.DebugMode);
			*/
			PopulateShop(Const.JsonShirtsDir, 1);
		}

		private void LoadNpcSchedule(NPC npc)
		{
			npc.Schedule = npc.getSchedule(Game1.dayOfMonth);
			npc.scheduleTimeToTry = 9999999;
			npc.ignoreScheduleToday = false;
			npc.followSchedule = true;
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

				Log.D($"CatShop first object: {firstFolder.Name}",
					Config.DebugMode);
				Log.D($"CatShop last object: {lastFolder.Name}",
					Config.DebugMode);

				var firstObject = 0;
				var lastObject = 0;

				switch(type)
				{
					case 0:
						firstObject = _jsonAssets.GetHatId(firstFolder.Name);
						lastObject = _jsonAssets.GetHatId(lastFolder.Name);
						break;
					case 1:
						firstObject = _jsonAssets.GetClothingId(firstFolder.Name);
						lastObject = _jsonAssets.GetClothingId(lastFolder.Name);
						break;
					default:
						Log.E("The CatShop hit a dead end. This feature wasn't finished!");
						throw new NotImplementedException();
				}

				var goalQty = (lastObject - firstObject) / Const.CatShopQtyRatio;

				Log.D("CatShop Restock bounds:",
					Config.DebugMode);
				Log.D($"index: {firstObject}, end: {lastObject}, goalQty: {goalQty}",
					Config.DebugMode);

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
							_catShopStock.Add(
								new StardewValley.Objects.Hat(id), new[]
								{ Const.ClothingCost, 1 });
							break;
						case 1:
							_catShopStock.Add(
								new StardewValley.Objects.Clothing(id), new[]
								{ Const.ClothingCost, 1 });
							break;
						default:
							Log.E("The CatShop hit a dead end. This feature wasn't finished!");
							throw new NotImplementedException();
					}
				}
			}
			catch (Exception ex)
			{
				Log.E("Sailor Styles failed to populate the clothes shop."
					+ "Did you remove all the clothing folders, or did I do something wrong?");
				Log.E("Exception logged:\n" + ex);
			}
		}

		private void CatShop()
		{
			Game1.playSound("cat");
			
			var text = (string) null;
			/*
			if (!Cate)
			{
			*/
				var random = new Random((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
				var whichDialogue = Const.ShopDialogueRoot + random.Next(5);
				if (whichDialogue.EndsWith("5"))
					whichDialogue += $".{Game1.currentSeason}";
				text = i18n.Get(whichDialogue);
			/*
			}
			else
			{
				// bllblblbl
				text = i18n.Get(Const.ShopDialogueRoot + "Cate");
			}
			*/
			
			Game1.activeClickableMenu = new ShopMenu(_catShopStock);
			((ShopMenu) Game1.activeClickableMenu).portraitPerson
				//= Cate ? _cateNpc : _catNpc;
				= _catNpc;
			((ShopMenu)Game1.activeClickableMenu).potraitPersonDialogue
				= Game1.parseText(text, Game1.dialogueFont, 304);
		}

		private static void DebugWarpPlayer()
		{
			Game1.warpFarmer(Const.LocationTarget, 31, 97, 2);
			Log.D($"Pressed {Instance.Config.DebugWarpKey}: Warped {Game1.player.Name} to the CatShop.");
		}
	}
}
