using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
		
		private static IJsonAssetsApi _ja;
		
		private NPC _catNpc;

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
			helper.Content.AssetLoaders.Add(new Editors.NpcLoader(helper));
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

			_ja = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
			if (_ja == null)
			{
				Log.E("Can't access the Json Assets API. Is the mod installed correctly?");
				return;
			}
			
			foreach (var pack in Const.HatPacks)
				_ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "Assets", Const.HatsDir, pack));
			foreach (var pack in Const.ClothingPacks)
				_ja.LoadAssets(Path.Combine(Helper.DirectoryPath, "Assets", Const.ClothingDir, pack));
		}
		
		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			CatShopRestock();
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
		}

		private void CatShopRestock()
		{
			_catShopStock.Clear();
			PopulateShop(true);
			PopulateShop(false);
		}

		private void LoadNpcSchedule(NPC npc)
		{
			npc.Schedule = npc.getSchedule(Game1.dayOfMonth);
			npc.scheduleTimeToTry = 9999999;
			npc.ignoreScheduleToday = false;
			npc.followSchedule = true;
		}

		private void PopulateShop(bool isHat)
		{
			try
			{
				var random = new Random();
				var stock = new List<int>();
				var contentPacks = isHat
					? Const.HatPacks
					: Const.ClothingPacks;
				
				Log.D($"Hats : {_ja.GetAllHatIds().Count} -- Clothing : {_ja.GetAllClothingIds().Count}",
					Config.DebugMode);

				foreach (var pack in contentPacks)
				{
					var packName = $"{Const.ContentPackPrefix} {pack}";
					var contentNames = isHat
						? _ja.GetAllHatsFromContentPack("HAT")
						: _ja.GetAllClothingFromContentPack(packName);

					Log.D($"Using content pack [{packName}]",
						Config.DebugMode);

					if (contentNames == null || contentNames.Count == 0)
					{
						Log.E("Failed to populate content names.");
						throw new NullReferenceException();
					}

					Log.D("ContentNames : ",
						Config.DebugMode);
					foreach (var name in contentNames)
						Log.D(name,
							Config.DebugMode);

					var goalQty = contentNames.Count / Const.CatShopQtyRatio;
					foreach (var name in contentNames)
					{
						var currentQty = 0;
						while (currentQty < goalQty)
						{
							var id = isHat
								? random.Next(
									_ja.GetHatId(contentNames.First()),
									_ja.GetHatId(contentNames.Last()))
								: random.Next(
									_ja.GetClothingId(contentNames.First()),
									_ja.GetClothingId(contentNames.Last()));

							if (!stock.Contains(id))
							{
								stock.Add(isHat
									? _ja.GetHatId(name)
									: _ja.GetClothingId(name));
								++currentQty;
							}
						}
					}
					foreach (var id in stock)
						if (isHat)
							_catShopStock.Add(new StardewValley.Objects.Hat(id), new[]
								{Const.ClothingCost, 1});
						else
							_catShopStock.Add(new StardewValley.Objects.Clothing(id), new[]
								{ Const.ClothingCost, 1 });
				}
			}
			catch (Exception ex)
			{
				Log.E("Sailor Styles failed to populate the clothes shop."
					+ " Did you install the clothing folders, or did I break something?");
				Log.E("Exception logged:\n" + ex);
			}
		}

		private void CatShop()
		{
			Game1.playSound("cat");
			
			var random = new Random((int)((long)Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
			var whichDialogue = Const.ShopDialogueRoot + random.Next(5);
			if (whichDialogue.EndsWith("5"))
				whichDialogue += $".{Game1.currentSeason}";
			var text = i18n.Get(whichDialogue);
			
			Game1.activeClickableMenu = new ShopMenu(_catShopStock);
			((ShopMenu) Game1.activeClickableMenu).portraitPerson = _catNpc;
			((ShopMenu)Game1.activeClickableMenu).potraitPersonDialogue
				= Game1.parseText(text, Game1.dialogueFont, 304);
		}

		private static void DebugWarpPlayer()
		{
			Game1.warpFarmer(Const.LocationTarget, 31, 97, 2);
			Log.D($"Pressed {Instance.Config.DebugWarpKey} : Warped {Game1.player.Name} to the CatShop.");
		}
	}
}
