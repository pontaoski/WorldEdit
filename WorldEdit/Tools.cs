using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using TShockAPI;
using TShockAPI.Net;

namespace WorldEdit
{
	public static class Tools
	{
		public static string GetClipboardPath(TSPlayer plr)
		{
			string id = plr.RealPlayer ? plr.Index.ToString() : "server";
			return Path.Combine("worldedit", String.Format("clipboard-{0}.dat", id));
		}
		public static List<int> GetColorByName(string color)
		{
			int ID;
			if (int.TryParse(color, out ID) && ID > 0 && ID < Main.numTileColors)
			{
				return new List<int> { ID };
			}

			List<int> list = new List<int>();
			for (int i = 0; i < WorldEdit.ColorNames.Count; i++)
			{
				if (WorldEdit.ColorNames[i] == color)
				{
					return new List<int> { i };
				}
				if (WorldEdit.ColorNames[i].StartsWith(color))
				{
					list.Add(i);
				}
			}
			return list;
		}
		public static List<int> GetTileByName(string tile)
		{
			int ID;
			if (int.TryParse(tile, out ID) && ID >= 0 && ID < Main.maxTileSets &&
				!Main.tileFrameImportant[ID] && !WorldEdit.InvalidTiles.Contains(ID))
			{
				return new List<int> { ID };
			}

			List<int> list = new List<int>();
			foreach (KeyValuePair<string, int> kv in WorldEdit.TileNames)
			{
				if (kv.Key == tile)
				{
					return new List<int> { kv.Value };
				}
				if (kv.Key.StartsWith(tile))
				{
					list.Add(kv.Value);
				}
			}
			return list;
		}
		public static List<int> GetWallByName(string wall)
		{
			int ID;
			if (int.TryParse(wall, out ID) && ID >= 0 && ID < Main.maxWallTypes)
			{
				return new List<int> { ID };
			}

			List<int> list = new List<int>();
			foreach (KeyValuePair<string, int> kv in WorldEdit.WallNames)
			{
				if (kv.Key == wall)
				{
					return new List<int> { kv.Value };
				}
				if (kv.Key.StartsWith(wall))
				{
					list.Add(kv.Value);
				}
			}
			return list;
		}
		public static bool HasClipboard(TSPlayer plr)
		{
			string id = plr.RealPlayer ? plr.Index.ToString() : "server";
			return File.Exists(Path.Combine("worldedit", String.Format("clipboard-{0}.dat", id)));
		}
		public static Tile[,] LoadWorldData(string path)
		{
			Tile[,] tile;
			using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
			{
				int xLen = reader.ReadInt32();
				int yLen = reader.ReadInt32();
				tile = new Tile[xLen, yLen];

				for (int i = 0; i < xLen; i++)
				{
					for (int j = 0; j < yLen; j++)
					{
						tile[i, j] = ReadTile(reader);
					}
				}
				return tile;
			}
		}
		public static void LoadWorldSection(string path)
		{
			using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				int xLen = reader.ReadInt32();
				int yLen = reader.ReadInt32();

				for (int i = x; i < x + xLen; i++)
				{
					for (int j = y; j < y + yLen; j++)
					{
						Main.tile[i, j] = reader.ReadTile();
						Main.tile[i, j].skipLiquid(true);
					}
				}
				ResetSection(x, y, x + xLen, y + yLen);
			}
		}
		public static void PrepareUndo(int x, int y, int x2, int y2, TSPlayer plr)
		{
			WorldEdit.GetPlayerInfo(plr).redoLevel = -1;
			WorldEdit.GetPlayerInfo(plr).undoLevel++;
			string id = plr.RealPlayer ? plr.Index.ToString() : "server";
			string path = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", id, WorldEdit.GetPlayerInfo(plr).undoLevel));
			SaveWorldSection(x, y, x2, y2, path);

			foreach (string fileName in Directory.EnumerateFiles("worldedit", String.Format("redo-{0}-*.dat", plr)))
			{
				File.Delete(fileName);
			}
		}
		public static Tile ReadTile(this BinaryReader reader)
		{
			Tile tile = new Tile();
			byte flags = reader.ReadByte();
			byte flags2 = reader.ReadByte();

			tile.actuator((flags & 64) == 64);
			tile.halfBrick((flags & 32) == 32);
			tile.inActive((flags & 128) == 128);
			tile.slope((byte)((flags2 << 4) & 3));
			tile.inActive((flags & 128) == 128);
			tile.wire((flags & 16) == 16);
			tile.wire2((flags2 & 1) == 1);
			tile.wire3((flags2 & 2) == 2);
			// Color
			if ((flags2 & 4) == 4)
			{
				tile.color(reader.ReadByte());
			}
			// Wall color
			if ((flags2 & 8) == 8)
			{
				tile.wallColor(reader.ReadByte());
			}
			// Tile type
			if ((flags & 1) == 1)
			{
				byte type = reader.ReadByte();
				tile.active(true);
				tile.type = type;
				if (Main.tileFrameImportant[type])
				{
					tile.frameX = reader.ReadInt16();
					tile.frameY = reader.ReadInt16();
				}
				else
				{
					tile.frameX = -1;
					tile.frameY = -1;
				}
			}
			// Wall type
			if ((flags & 4) == 4)
			{
				tile.wall = reader.ReadByte();
			}
			// Liquid
			if ((flags & 8) == 8)
			{
				tile.liquid = reader.ReadByte();
				tile.liquidType(reader.ReadByte());
			}
			return tile;
		}
		public static void Redo(TSPlayer plr)
		{
			WorldEdit.GetPlayerInfo(plr).undoLevel++;
			string id = plr.RealPlayer ? plr.Index.ToString() : "server";
			string redoPath = Path.Combine("worldedit", String.Format("redo-{0}-{1}.dat", id, WorldEdit.GetPlayerInfo(plr).redoLevel));
			string undoPath = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", id, WorldEdit.GetPlayerInfo(plr).undoLevel));
			WorldEdit.GetPlayerInfo(plr).redoLevel--;
			using (BinaryReader reader = new BinaryReader(new FileStream(redoPath, FileMode.Open)))
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				int x2 = x + reader.ReadInt32() - 1;
				int y2 = y + reader.ReadInt32() - 1;
				SaveWorldSection(x, y, x2, y2, undoPath);
			}
			LoadWorldSection(redoPath);
			File.Delete(redoPath);
		}
		public static void ResetSection(int x, int y, int x2, int y2)
		{
			int lowX = Netplay.GetSectionX(x);
			int highX = Netplay.GetSectionX(x2);
			int lowY = Netplay.GetSectionY(y);
			int highY = Netplay.GetSectionY(y2);
			foreach (ServerSock sock in Netplay.serverSock)
			{
				for (int i = lowX; i <= highX; i++)
				{
					for (int j = lowY; j <= highY; j++)
					{
						sock.tileSection[i, j] = false;
					}
				}
			}
		}
		public static void SaveWorldData(Tile[,] tiles, string path)
		{
			using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
			{
				int xLen = tiles.GetLength(0);
				int yLen = tiles.GetLength(1);
				writer.Write(xLen);
				writer.Write(yLen);
				for (int i = 0; i < xLen; i++)
				{
					for (int j = 0; j < yLen; j++)
					{
						writer.Write(tiles[i, j] ?? new Tile());
					}
				}
			}
		}
		public static void SaveWorldSection(int x, int y, int x2, int y2, string path)
		{
			using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create)))
			{
				writer.Write(x);
				writer.Write(y);
				writer.Write(x2 - x + 1);
				writer.Write(y2 - y + 1);
				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
					{
						writer.Write(Main.tile[i, j]);
					}
				}
			}
		}
		public static void Write(this BinaryWriter writer, Tile tile)
		{
			byte flags = 0;
			byte flags2 = 0;
			if (tile.active())
			{
				flags |= 1;
			}
			if (tile.wall != 0)
			{
				flags |= 4;
			}
			if (tile.liquid != 0)
			{
				flags |= 8;
			}
			if (tile.wire())
			{
				flags |= 16;
			}
			if (tile.halfBrick())
			{
				flags |= 32;
			}
			if (tile.actuator())
			{
				flags |= 64;
			}
			if (tile.inActive())
			{
				flags |= 128;
			}
			if (tile.wire2())
			{
				flags2 |= 1;
			}
			if (tile.wire3())
			{
				flags2 |= 2;
			}
			if (tile.color() != 0)
			{
				flags2 |= 4;
			}
			if (tile.wallColor() != 0)
			{
				flags2 |= 8;
			}
			flags2 |= (byte)(tile.slope() << 4);

			writer.Write(flags);
			writer.Write(flags2);
			if (tile.color() != 0)
			{
				writer.Write(tile.color());
			}
			if (tile.wallColor() != 0)
			{
				writer.Write(tile.wallColor());
			}
			if (tile.active())
			{
				writer.Write(tile.type);
				if (Main.tileFrameImportant[tile.type])
				{
					writer.Write(tile.frameX);
					writer.Write(tile.frameY);
				}
			}
			if (tile.wall != 0)
			{
				writer.Write(tile.wall);
			}
			if (tile.liquid != 0)
			{
				writer.Write(tile.liquid);
				writer.Write(tile.liquidType());
			}
		}
		public static void Undo(TSPlayer plr)
		{
			WorldEdit.GetPlayerInfo(plr).redoLevel++;
			string id = plr.RealPlayer ? plr.Index.ToString() : "server";
			string redoPath = Path.Combine("worldedit", String.Format("redo-{0}-{1}.dat", id, WorldEdit.GetPlayerInfo(plr).redoLevel));
			string undoPath = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", id, WorldEdit.GetPlayerInfo(plr).undoLevel));
			WorldEdit.GetPlayerInfo(plr).undoLevel--;
			using (BinaryReader reader = new BinaryReader(new FileStream(undoPath, FileMode.Open)))
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				int x2 = x + reader.ReadInt32() - 1;
				int y2 = y + reader.ReadInt32() - 1;
				SaveWorldSection(x, y, x2, y2, redoPath);
			}
			LoadWorldSection(undoPath);
			File.Delete(undoPath);
		}
	}
}