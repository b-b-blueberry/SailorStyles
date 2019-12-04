using System.IO;

using StardewValley;
using StardewModdingAPI;

using Microsoft.Xna.Framework.Graphics;

using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace SailorStyles_Clothing.Editors
{
	class MapEditor : IAssetEditor
	{
		private IModHelper Helper;
		private IMonitor Monitor;

		public MapEditor(IModHelper helper, IMonitor monitor)
		{
			Helper = helper;
			Monitor = monitor;
		}

		public bool CanEdit<T>(IAssetInfo asset)
		{
			return asset.AssetNameEquals(Path.Combine("Maps", Data.LocationTarget));
		}

		public void Edit<T>(IAssetData asset)
		{
			if (asset.AssetNameEquals(Path.Combine("Maps", Data.LocationTarget)))
			{
				if (Game1.dayOfMonth % 7 <= 1)
				{
					Monitor.Log("Patching map file " + Data.LocationTarget,
						LogLevel.Trace);
					PrepareMap((Map)asset.Data);
				}
			}
		}

		private void PrepareMap(Map map)
		{
			AddTilesheet(map);
			AddLayers(map);
			AddTiles(map);
		}

		private void AddTilesheet(Map map)
		{
			var path = Helper.Content.GetActualAssetKey(
				Path.Combine("Assets", Data.CatID + "_tilesheet" + Data.ImgExt));

			var texture = Helper.Content.Load<Texture2D>(path);
			var sheet = new TileSheet(
				Data.CatID, map, path,
				new Size(texture.Width / 16, texture.Height / 16),
				new Size(16, 16));

			map.AddTileSheet(sheet);
			map.LoadTileSheets(Game1.mapDisplayDevice);
		}

		private void AddLayers(Map map)
		{
			var layer = map.GetLayer("Buildings");
			layer = new Layer(
				Data.ExtraLayerID, map, layer.LayerSize, layer.TileSize);
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

			layer = map.GetLayer(Data.ExtraLayerID);

			if (layer == null)
			{
				Monitor.Log("Failed to add CatShop sprites: " +
					"Extra map layer couldn't be added.",
					LogLevel.Error);
				return;
			}

			tiles = layer.Tiles;

			if (!ModEntry.cate)
			{
				tiles[30, 94] = new StaticTile(layer, sheet, mode, 31);
				tiles[30, 95] = new StaticTile(layer, sheet, mode, 31 + sheet.SheetWidth);

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
					tiles[31, 94] = new StaticTile(layer, sheet, mode, 15);
					tiles[30, 95] = new StaticTile(layer, sheet, mode, 14 + sheet.SheetWidth);
					tiles[31, 95] = new StaticTile(layer, sheet, mode, 15 + sheet.SheetWidth);
				}
				else
				{
					tiles[31, 94] = new StaticTile(layer, sheet, mode, 17);
					tiles[30, 95] = new StaticTile(layer, sheet, mode, 16 + sheet.SheetWidth);
					tiles[31, 95] = new StaticTile(layer, sheet, mode, 17 + sheet.SheetWidth);
				}
			}
		}
	}
}
