using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using OTAPI.Tile;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using Terraria.GameContent.Tile_Entities;
using Terraria.DataStructures;

namespace WorldEdit
{
	public static class Tools
	{
		private const int BUFFER_SIZE = 1048576;
		private const int MAX_UNDOS = 50;

		public static string GetClipboardPath(int accountID)
		{
			return Path.Combine("worldedit", String.Format("clipboard-{0}.dat", accountID));
		}
		public static bool CorrectName(string Name)
		{
			char[] Invalid = Path.GetInvalidFileNameChars();
			foreach (char c in Name)
			{
				if (Invalid.Contains(c))
				{ return false; }
			}
			return true;
		}
		public static List<int> GetColorID(string color)
		{
			int ID;
			if (int.TryParse(color, out ID) && ID >= 0 && ID < Main.numTileColors)
				return new List<int> { ID };

			var list = new List<int>();
			foreach (var kvp in WorldEdit.Colors)
			{
				if (kvp.Key == color)
					return new List<int> { kvp.Value };
				if (kvp.Key.StartsWith(color))
					list.Add(kvp.Value);
			}
			return list;
		}
		public static List<int> GetTileID(string tile)
		{
			int ID;
			if (int.TryParse(tile, out ID) && ID >= 0 && ID < Main.maxTileSets)
				return new List<int> { ID };

			var list = new List<int>();
			foreach (var kvp in WorldEdit.Tiles)
			{
				if (kvp.Key == tile)
					return new List<int> { kvp.Value };
				if (kvp.Key.StartsWith(tile))
					list.Add(kvp.Value);
			}
			return list;
		}
		public static List<int> GetWallID(string wall)
		{
			int ID;
			if (int.TryParse(wall, out ID) && ID >= 0 && ID < Main.maxWallTypes)
				return new List<int> { ID };

			var list = new List<int>();
			foreach (var kvp in WorldEdit.Walls)
			{
				if (kvp.Key == wall)
					return new List<int> { kvp.Value };
				if (kvp.Key.StartsWith(wall))
					list.Add(kvp.Value);
			}
			return list;
		}
		public static int GetSlopeID(string slope)
		{
			int ID;
			if (int.TryParse(slope, out ID) && ID >= 0 && ID < 6)
				return ID;

			if (!WorldEdit.Slopes.TryGetValue(slope, out int Slope)) { return -1; }

			return Slope;
		}
		public static bool HasClipboard(int accountID)
		{
			return File.Exists(Path.Combine("worldedit", String.Format("clipboard-{0}.dat", accountID)));
		}
		public static Tuple<Tile, string, Item, Item[]>[,] LoadWorldDataNew(string path)
		{
			Tuple<Tile, string, Item, Item[]>[,] tile;
			// GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
			using (var reader =
				new BinaryReader(
					new BufferedStream(
						new GZipStream(File.Open(path, FileMode.Open), CompressionMode.Decompress), BUFFER_SIZE)))
			{
				reader.ReadInt32();
				reader.ReadInt32();
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();
				tile = new Tuple<Tile, string, Item, Item[]>[width, height];

				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < height; j++)
						tile[i, j] = ReadTile(reader);
				}
				return tile;
			}
		}
		private static Tuple<Tile[,], int, int> LoadWorldDataOld(string path)
		{
			Tile[,] tile;
			// GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
			using (var reader =
				new BinaryReader(
					new BufferedStream(
						new GZipStream(File.Open(path, FileMode.Open), CompressionMode.Decompress), BUFFER_SIZE)))
			{
				reader.ReadInt32();
				reader.ReadInt32();
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();
				tile = new Tile[width, height];

				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < height; j++)
					{
						tile[i, j] = ReadTileOld(reader);
					}
				}

				return new Tuple<Tile[,], int, int>(tile, width, height);
			}
		}
		public static void LoadWorldSection(string path)
		{
			// GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
			using (var reader =
				new BinaryReader(
					new BufferedStream(
						new GZipStream(File.Open(path, FileMode.Open), CompressionMode.Decompress), BUFFER_SIZE)))
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				int width = reader.ReadInt32();
				int height = reader.ReadInt32();

				for (int i = x; i < x + width; i++)
				{
					for (int j = y; j < y + height; j++)
					{
						var Tile = reader.ReadTile();
						if ((Tile.Item2 != null) || (Tile.Item3 != null)
							|| (Tile.Item4 != null))
						{
							Main.tile[i, j] = new Tile();
							Main.tile[i + 1, j] = new Tile();
							Main.tile[i, j + 1] = new Tile();
							Main.tile[i + 1, j + 1] = new Tile();
						}
						Main.tile[i, j] = Tile.Item1;
						Main.tile[i, j].skipLiquid(true);
						if (Tile.Item2 != null)
						{
							int SignID = Sign.ReadSign(i, j);
							string Text = Tile.Item2;
							if (SignID != -1)
							{ Sign.TextSign(SignID, Text); }
						}
						if (Tile.Item3 != null)
						{
							int FrameID = TEItemFrame.Place(i, j);
							if (FrameID != -1)
							{
								WorldGen.PlaceObject(i, j, Terraria.ID.TileID.ItemFrame);
								TEItemFrame frame = (TEItemFrame)TileEntity.ByID[FrameID];
								frame.item = Tile.Item3;
							}
						}
						else if (Tile.Item4 != null)
						{
							int ChestID = Chest.CreateChest(i, j);
							if (ChestID != -1)
							{
								WorldGen.PlaceChest(i, j);
								for (int a = 0; a < 40; a++)
								{
									Main.chest[ChestID].item[a] = Tile.Item4[a];
								}
							}
						}
					}
				}
				ResetSection(x, y, x + width, y + height);
			}
		}
		public static void PrepareUndo(int x, int y, int x2, int y2, TSPlayer plr)
		{
			if (WorldEdit.Database.GetSqlType() == SqlType.Mysql)
				WorldEdit.Database.Query("INSERT IGNORE INTO WorldEdit VALUES (@0, -1, -1)", plr.User.ID);
			else
				WorldEdit.Database.Query("INSERT OR IGNORE INTO WorldEdit VALUES (@0, 0, 0)", plr.User.ID);
			WorldEdit.Database.Query("UPDATE WorldEdit SET RedoLevel = -1 WHERE Account = @0", plr.User.ID);
			WorldEdit.Database.Query("UPDATE WorldEdit SET UndoLevel = UndoLevel + 1 WHERE Account = @0", plr.User.ID);

			int undoLevel = 0;
			using (var reader = WorldEdit.Database.QueryReader("SELECT UndoLevel FROM WorldEdit WHERE Account = @0", plr.User.ID))
			{
				if (reader.Read())
					undoLevel = reader.Get<int>("UndoLevel");
			}

			string path = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", plr.User.ID, undoLevel));
			SaveWorldSection(x, y, x2, y2, path);

			foreach (string fileName in Directory.EnumerateFiles("worldedit", String.Format("redo-{0}-*.dat", plr.User.ID)))
				File.Delete(fileName);
			File.Delete(Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", plr.User.ID, undoLevel - MAX_UNDOS)));
		}
		public static Tuple<Tile, string, Item, Item[]> ReadTile(this BinaryReader reader)
		{
			Tile tile = new Tile();
			string sign = null;
			Item item = null;
			Item[] items = null;
			tile.sTileHeader = reader.ReadInt16();
			tile.bTileHeader = reader.ReadByte();
			tile.bTileHeader2 = reader.ReadByte();
			// Tile type
			if (tile.active())
			{
				tile.type = reader.ReadUInt16();
				if (Main.tileFrameImportant[tile.type])
				{
					tile.frameX = reader.ReadInt16();
					tile.frameY = reader.ReadInt16();
				}
			}
			tile.wall = reader.ReadByte();
			tile.liquid = reader.ReadByte();
			
			if ((tile.type == Terraria.ID.TileID.Signs)
				|| (tile.type == Terraria.ID.TileID.AnnouncementBox)
				|| (tile.type == Terraria.ID.TileID.Tombstones))
			{
				int signID = reader.ReadInt32();
				if (signID != -1)
				{
					sign = reader.ReadString();
				}
			}
			else if (tile.type == Terraria.ID.TileID.ItemFrame)
			{
				int frameID = reader.ReadInt32();
				if (frameID != -1)
				{
					item = new Item();
					item.netDefaults(reader.ReadInt32());
					item.prefix = reader.ReadByte();
				}
			}
			else if ((tile.type == Terraria.ID.TileID.Containers)
				|| (tile.type == Terraria.ID.TileID.Dressers))
			{
				int chestID = reader.ReadInt32();
				if (chestID != -1)
				{
					int Length = reader.ReadInt32();
					items = new Item[Length];
					for (int i = 0; i < Length; i++)
					{
						items[i] = new Item();
						items[i].netDefaults(reader.ReadInt32());
						items[i].stack = reader.ReadInt32();
						items[i].prefix = reader.ReadByte();
					}
				}
			}
			return new Tuple<Tile, string, Item, Item[]>(tile, sign, item, items);
		}
		private static Tile ReadTileOld(this BinaryReader reader)
		{
			Tile tile = new Tile();
			tile.sTileHeader = reader.ReadInt16();
			tile.bTileHeader = reader.ReadByte();
			tile.bTileHeader2 = reader.ReadByte();

			// Tile type
			if (tile.active())
			{
				tile.type = reader.ReadUInt16();
				if (Main.tileFrameImportant[tile.type])
				{
					tile.frameX = reader.ReadInt16();
					tile.frameY = reader.ReadInt16();
				}
			}
			tile.wall = reader.ReadByte();
			tile.liquid = reader.ReadByte();
			return tile;
		}
		public static bool Redo(int accountID)
		{
			int redoLevel = 0;
			int undoLevel = 0;
			using (var reader = WorldEdit.Database.QueryReader("SELECT RedoLevel, UndoLevel FROM WorldEdit WHERE Account = @0", accountID))
			{
				if (reader.Read())
				{
					redoLevel = reader.Get<int>("RedoLevel") - 1;
					undoLevel = reader.Get<int>("UndoLevel") + 1;
				}
				else
					return false;
			}

			if (redoLevel < -1)
				return false;

			string redoPath = Path.Combine("worldedit", String.Format("redo-{0}-{1}.dat", accountID, redoLevel + 1));
			WorldEdit.Database.Query("UPDATE WorldEdit SET RedoLevel = @0 WHERE Account = @1", redoLevel, accountID);

			if (!File.Exists(redoPath))
				return false;

			string undoPath = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", accountID, undoLevel));
			WorldEdit.Database.Query("UPDATE WorldEdit SET UndoLevel = @0 WHERE Account = @1", undoLevel, accountID);

			using (var reader = new BinaryReader(new GZipStream(new FileStream(redoPath, FileMode.Open), CompressionMode.Decompress)))
			{
				int x = Math.Max(0, reader.ReadInt32());
				int y = Math.Max(0, reader.ReadInt32());
				int x2 = Math.Min(x + reader.ReadInt32() - 1, Main.maxTilesX - 1);
				int y2 = Math.Min(y + reader.ReadInt32() - 1, Main.maxTilesY - 1);
				SaveWorldSection(x, y, x2, y2, undoPath);
			}
			LoadWorldSection(redoPath);
			File.Delete(redoPath);
			return true;
		}
		public static void ResetSection(int x, int y, int x2, int y2)
		{
			int lowX = Netplay.GetSectionX(x);
			int highX = Netplay.GetSectionX(x2);
			int lowY = Netplay.GetSectionY(y);
			int highY = Netplay.GetSectionY(y2);
			foreach (RemoteClient sock in Netplay.Clients.Where(s => s.IsActive))
			{
				for (int i = lowX; i <= highX; i++)
				{
					for (int j = lowY; j <= highY; j++)
						sock.TileSections[i, j] = false;
				}
			}
		}
		public static void SaveWorldSection(int x, int y, int x2, int y2, string path)
		{
			// GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
			using (var writer =
				new BinaryWriter(
					new BufferedStream(
						new GZipStream(File.Open(path, FileMode.Create), CompressionMode.Compress), BUFFER_SIZE)))
			{
				writer.Write(x);
				writer.Write(y);
				writer.Write(x2 - x + 1);
				writer.Write(y2 - y + 1);

				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
						writer.Write((Main.tile[i, j] ?? new Tile()), i, j);
				}
			}
		}
		public static void Write(this BinaryWriter writer, ITile tile, int X, int Y)
		{
			writer.Write(tile.sTileHeader);
			writer.Write(tile.bTileHeader);
			writer.Write(tile.bTileHeader2);

			if (tile.active())
			{
				writer.Write(tile.type);
				if (Main.tileFrameImportant[tile.type])
				{
					writer.Write(tile.frameX);
					writer.Write(tile.frameY);
				}
			}
			writer.Write(tile.wall);
			writer.Write(tile.liquid);
			if (tile.active())
			{
				if ((tile.type == Terraria.ID.TileID.Signs)
					|| (tile.type == Terraria.ID.TileID.AnnouncementBox)
					|| (tile.type == Terraria.ID.TileID.Tombstones))
				{
					if (((tile.frameX % 36) == 0) && (tile.frameY == 0))
					{
						int SignID = Sign.ReadSign(X, Y, false);
						writer.Write(SignID);
						if (SignID != -1)
						{
							Sign Sign = Main.sign[SignID];
							writer.Write(Sign.text);
						}
					}
					else { writer.Write(-1); }
				}
				else if (tile.type == Terraria.ID.TileID.ItemFrame)
				{
					if (((tile.frameX % 36) == 0) && (tile.frameY == 0))
					{
						int FrameID = TEItemFrame.Find(X, Y);
						writer.Write(FrameID);
						if (FrameID != -1)
						{
							TEItemFrame Frame = (TEItemFrame)TileEntity.ByID[FrameID];
							writer.Write(Frame.item.netID);
							writer.Write(Frame.item.prefix);
						}
					}
					else { writer.Write(-1); }
				}
				else if ((tile.type == Terraria.ID.TileID.Containers)
					|| (tile.type == Terraria.ID.TileID.Dressers))
				{
					if (((tile.frameX % 36) == 0) && (tile.frameY == 0))
					{
						int ChestID = Chest.FindChest(X, Y);
						writer.Write(ChestID);
						if (ChestID != -1)
						{
							Chest Chest = Main.chest[ChestID];
							writer.Write((Chest.item == null) ? 0 : Chest.item.Length);
							if (Chest.item != null)
							{
								foreach (Item i in Chest.item)
								{
									if (i == null)
									{
										writer.Write(new Item().netID);
										writer.Write(new Item().stack);
										writer.Write(new Item().prefix);
									}
									else
									{
										writer.Write(i.netID);
										writer.Write(i.stack);
										writer.Write(i.prefix);
									}
								}
							}
						}
					}
					else { writer.Write(-1); }
				}
			}
		}
		public static void Write(this BinaryWriter writer, Tuple<Tile, string, Item, Item[]> tile)
		{
			writer.Write(tile.Item1.sTileHeader);
			writer.Write(tile.Item1.bTileHeader);
			writer.Write(tile.Item1.bTileHeader2);

			if (tile.Item1.active())
			{
				writer.Write(tile.Item1.type);
				if (Main.tileFrameImportant[tile.Item1.type])
				{
					writer.Write(tile.Item1.frameX);
					writer.Write(tile.Item1.frameY);
				}
			}
			writer.Write(tile.Item1.wall);
			writer.Write(tile.Item1.liquid);
			if ((tile.Item1.type == Terraria.ID.TileID.Signs)
				|| (tile.Item1.type == Terraria.ID.TileID.AnnouncementBox)
				|| (tile.Item1.type == Terraria.ID.TileID.Tombstones))
			{
				if ((tile.Item1.frameX == 0) && (tile.Item1.frameY == 0))
				{
					if (tile.Item2 != null)
					{ writer.Write(tile.Item2); }
					else { writer.Write(-1); }
				}
				else { writer.Write(-1); }
			}
			else if (tile.Item1.type == Terraria.ID.TileID.ItemFrame)
			{
				if ((tile.Item1.frameX == 0) && (tile.Item1.frameY == 0))
				{
					if (tile.Item3 != null)
					{
						writer.Write(tile.Item3.netID);
						writer.Write(tile.Item3.prefix);
					}
					else { writer.Write(-1); }
				}
				else { writer.Write(-1); }
			}
			else if ((tile.Item1.type == Terraria.ID.TileID.Containers)
				|| (tile.Item1.type == Terraria.ID.TileID.Dressers))
			{
				if ((tile.Item1.frameX == 0) && (tile.Item1.frameY == 0))
				{
					if (tile.Item4 != null)
					{
						writer.Write(40);
						foreach (Item i in tile.Item4)
						{
							writer.Write(i.netID);
							writer.Write(i.stack);
							writer.Write(i.prefix);
						}
					}
					else { writer.Write(-1); }
				}
				else { writer.Write(-1); }
			}
		}
		private static void WriteTileOld(this BinaryWriter writer, ITile tile)
		{
			writer.Write(tile.sTileHeader);
			writer.Write(tile.bTileHeader);
			writer.Write(tile.bTileHeader2);

			if (tile.active())
			{
				writer.Write(tile.type);
				if (Main.tileFrameImportant[tile.type])
				{
					writer.Write(tile.frameX);
					writer.Write(tile.frameY);
				}
			}
			writer.Write(tile.wall);
			writer.Write(tile.liquid);
		}
		public static void Convert(string file)
		{
			string newfile = file.Substring(0, (file.LastIndexOf('\\') + 1)) + "schematic-new-" + file.Substring(file.LastIndexOf('\\') + 11);
			if (File.Exists(newfile)) File.Delete(newfile);

			var Old = LoadWorldDataOld(file);

			Tile[,] tile = Old.Item1;
			using (var writer =
					new BinaryWriter(
						new BufferedStream(
							new GZipStream(File.Open(file, FileMode.Create), CompressionMode.Compress), BUFFER_SIZE)))
			{
				writer.Write(0);
				writer.Write(0);
				writer.Write(Old.Item2);
				writer.Write(Old.Item3);
				for (int i = 0; i < Old.Item2; i++)
				{
					for (int j = 0; j < Old.Item3; j++)
					{ writer.Write(new Tuple<Tile, string, Item, Item[]>(tile[i, j], null, null, null)); }
				}
			}

			File.Move(file, newfile);
		}
		public static bool Undo(int accountID)
		{
			int redoLevel = 0;
			int undoLevel = 0;
			using (var reader = WorldEdit.Database.QueryReader("SELECT RedoLevel, UndoLevel FROM WorldEdit WHERE Account = @0", accountID))
			{
				if (reader.Read())
				{
					redoLevel = reader.Get<int>("RedoLevel") + 1;
					undoLevel = reader.Get<int>("UndoLevel") - 1;
				}
				else
					return false;
			}

			if (undoLevel < -1)
				return false;

			string undoPath = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", accountID, undoLevel + 1));
			WorldEdit.Database.Query("UPDATE WorldEdit SET UndoLevel = @0 WHERE Account = @1", undoLevel, accountID);

			if (!File.Exists(undoPath))
				return false;

			string redoPath = Path.Combine("worldedit", String.Format("redo-{0}-{1}.dat", accountID, redoLevel));
			WorldEdit.Database.Query("UPDATE WorldEdit SET RedoLevel = @0 WHERE Account = @1", redoLevel, accountID);

			using (var reader = new BinaryReader(new GZipStream(new FileStream(undoPath, FileMode.Open), CompressionMode.Decompress)))
			{
				int x = Math.Max(0, reader.ReadInt32());
				int y = Math.Max(0, reader.ReadInt32());
				int x2 = Math.Min(x + reader.ReadInt32() - 1, Main.maxTilesX - 1);
				int y2 = Math.Min(y + reader.ReadInt32() - 1, Main.maxTilesY - 1);
				SaveWorldSection(x, y, x2, y2, redoPath);
			}
			LoadWorldSection(undoPath);
			File.Delete(undoPath);
			return true;
		}
	}
}