using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Extensions;
using TShockAPI.Net;

namespace WorldEdit
{
	public static class Tools
	{
		public static string GetClipboardPath(string accountName)
		{
			return Path.Combine("worldedit", String.Format("clipboard-{0}.dat", accountName));
		}
		public static List<int> GetColorByName(string color)
		{
			int ID;
			if (int.TryParse(color, out ID) && ID >= 0 && ID < Main.numTileColors)
				return new List<int> { ID };

			List<int> list = new List<int>();
			for (int i = 0; i < WorldEdit.ColorNames.Count; i++)
			{
				if (WorldEdit.ColorNames[i] == color)
					return new List<int> { i };
				if (WorldEdit.ColorNames[i].StartsWith(color))
					list.Add(i);
			}
			return list;
		}
		public static List<int> GetTileByName(string tile)
		{
			int ID;
			if (int.TryParse(tile, out ID) && ID >= 0 && ID < Main.maxTileSets)
				return new List<int> { ID };

			List<int> list = new List<int>();
			foreach (KeyValuePair<string, int> kv in WorldEdit.TileNames)
			{
				if (kv.Key == tile)
					return new List<int> { kv.Value };
				if (kv.Key.StartsWith(tile))
					list.Add(kv.Value);
			}
			return list;
		}
		public static List<int> GetWallByName(string wall)
		{
			int ID;
			if (int.TryParse(wall, out ID) && ID >= 0 && ID < Main.maxWallTypes)
				return new List<int> { ID };

			List<int> list = new List<int>();
			foreach (KeyValuePair<string, int> kv in WorldEdit.WallNames)
			{
				if (kv.Key == wall)
					return new List<int> { kv.Value };
				if (kv.Key.StartsWith(wall))
					list.Add(kv.Value);
			}
			return list;
		}
		public static bool HasClipboard(string accountName)
		{
			return File.Exists(Path.Combine("worldedit", String.Format("clipboard-{0}.dat", accountName)));
		}
		public static Tile[,] LoadWorldData(string path)
		{
			Tile[,] tile;
			using (var reader = new BinaryReader(new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress)))
			{
				int xLen = reader.ReadInt32();
				int yLen = reader.ReadInt32();
				tile = new Tile[xLen, yLen];

				for (int i = 0; i < xLen; i++)
				{
					for (int j = 0; j < yLen; j++)
						tile[i, j] = ReadTile(reader);
				}
				return tile;
			}
		}
		public static void LoadWorldSection(string path)
		{
			using (var reader = new BinaryReader(new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress)))
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
		public static bool ParseConditions(List<string> parameters, TSPlayer plr, out List<Condition> conditions)
		{
			conditions = new List<Condition>();
			if (!String.Equals(parameters[1], "where"))
			{
				plr.SendErrorMessage("Invalid where clause.");
				return false;
			}

			parameters.RemoveRange(0, 2);
			foreach (string expression in String.Join(" ", parameters).Split(','))
			{
				bool negated = false;
				string[] eq = expression.Split('=');
				string lhs = eq[0].ToLower().Trim();
				if (lhs[0] == '!')
				{
					lhs = lhs.Substring(1).Trim();
					negated ^= true;
				}
				if (lhs[lhs.Length - 1] == '!')
				{
					lhs = lhs.Substring(0, lhs.Length - 1).Trim();
					negated ^= true;
				}
				string rhs = eq.Length > 1 ? eq[1].ToLower().Trim() : "";

				switch (lhs)
				{
					case "color":
						#region color (=)
						{
							if (rhs == "")
							{
								if (negated)
									conditions.Add((i, j) => Main.tile[i, j].color() == 0);
								else
									conditions.Add((i, j) => Main.tile[i, j].color() > 0);
								break;
							}

							List<int> colors = GetColorByName(rhs);
							if (colors.Count == 0)
							{
								plr.SendErrorMessage("Invalid color.");
								return false;
							}
							else if (colors.Count > 1)
							{
								plr.SendErrorMessage("More than one color matched.");
								return false;
							}

							if (negated)
								conditions.Add((i, j) => Main.tile[i, j].color() != colors[0]);
							else
								conditions.Add((i, j) => Main.tile[i, j].color() == colors[0]);
						}
						#endregion
						break;
					case "colorwall":
						#region colorwall (=)
						{
							if (rhs == "")
							{
								if (negated)
									conditions.Add((i, j) => Main.tile[i, j].wallColor() == 0);
								else
									conditions.Add((i, j) => Main.tile[i, j].wallColor() > 0);
								break;
							}

							List<int> colors = GetColorByName(rhs);
							if (colors.Count == 0)
							{
								plr.SendErrorMessage("Invalid color.");
								return false;
							}
							else if (colors.Count > 1)
							{
								plr.SendErrorMessage("More than one color matched.");
								return false;
							}

							if (negated)
								conditions.Add((i, j) => Main.tile[i, j].wallColor() != colors[0]);
							else
								conditions.Add((i, j) => Main.tile[i, j].wallColor() == colors[0]);
						}
						#endregion
						break;
					case "honey":
						#region honey (=)
						{
							bool b = true;
							if (rhs != "" && !bool.TryParse(rhs, out b))
							{
								plr.SendErrorMessage("Invalid honey state.");
								return false;
							}
							if (b ^ negated)
								conditions.Add((i, j) => Main.tile[i, j].liquid > 0 && Main.tile[i, j].honey());
							else
								conditions.Add((i, j) => Main.tile[i, j].liquid == 0 || !Main.tile[i, j].honey());
						}
						#endregion
						break;
					case "lava":
						#region lava (=)
						{
							bool b = true;
							if (rhs != "" && !bool.TryParse(rhs, out b))
							{
								plr.SendErrorMessage("Invalid lava state.");
								return false;
							}
							if (b ^ negated)
								conditions.Add((i, j) => Main.tile[i, j].liquid > 0 && Main.tile[i, j].lava());
							else
								conditions.Add((i, j) => Main.tile[i, j].liquid == 0 || !Main.tile[i, j].lava());
						}
						#endregion
						break;
					case "tile":
						#region tile (=)
						{
							if (rhs == "")
							{
								if (negated)
									conditions.Add((i, j) => !Main.tile[i, j].active());
								else
									conditions.Add((i, j) => Main.tile[i, j].active());
								break;
							}

							List<int> tiles = GetTileByName(rhs);
							if (tiles.Count == 0)
							{
								plr.SendErrorMessage("Invalid tile.");
								return false;
							}
							else if (tiles.Count > 1)
							{
								plr.SendErrorMessage("More than one tile matched.");
								return false;
							}

							if (negated)
								conditions.Add((i, j) => !Main.tile[i, j].active() || Main.tile[i, j].type != tiles[0]);
							else
								conditions.Add((i, j) => Main.tile[i, j].active() && Main.tile[i, j].type == tiles[0]);
						}
						#endregion
						break;
					case "wall":
						#region wall (=)
						{
							if (rhs == "")
							{
								if (negated)
									conditions.Add((i, j) => Main.tile[i, j].wall == 0);
								else
									conditions.Add((i, j) => Main.tile[i, j].wall > 0);
								break;
							}

							List<int> walls = GetWallByName(rhs);
							if (walls.Count == 0)
							{
								plr.SendErrorMessage("Invalid wall.");
								return false;
							}
							else if (walls.Count > 1)
							{
								plr.SendErrorMessage("More than one wall matched.");
								return false;
							}

							if (negated)
								conditions.Add((i, j) => Main.tile[i, j].wall != walls[0]);
							else
								conditions.Add((i, j) => Main.tile[i, j].wall == walls[0]);
						}
						#endregion
						break;
					case "water":
						#region water (=)
						{
							bool b = true;
							if (rhs != "" && !bool.TryParse(rhs, out b))
							{
								plr.SendErrorMessage("Invalid water state.");
								return false;
							}
							if (b ^ negated)
								conditions.Add((i, j) => Main.tile[i, j].liquid > 0 && Main.tile[i, j].liquidType() == 0);
							else
								conditions.Add((i, j) => Main.tile[i, j].liquid == 0 || Main.tile[i, j].liquidType() != 0);
						}
						#endregion
						break;
					case "wire1":
						#region wire1 (=)
						{
							bool b = true;
							if (rhs != "" && !bool.TryParse(rhs, out b))
							{
								plr.SendErrorMessage("Invalid wire1 state.");
								return false;
							}
							if (b ^ negated)
								conditions.Add((i, j) => !Main.tile[i, j].wire());
							else
								conditions.Add((i, j) => Main.tile[i, j].wire());
						}
						#endregion
						break;
					case "wire2":
						#region wire2 (=)
						{
							bool b = true;
							if (rhs != "" && !bool.TryParse(rhs, out b))
							{
								plr.SendErrorMessage("Invalid wire2 state.");
								return false;
							}
							if (b ^ negated)
								conditions.Add((i, j) => !Main.tile[i, j].wire2());
							else
								conditions.Add((i, j) => Main.tile[i, j].wire2());
						}
						#endregion
						break;
					case "wire3":
						#region wire3 (=)
						{
							bool b = true;
							if (rhs != "" && !bool.TryParse(rhs, out b))
							{
								plr.SendErrorMessage("Invalid wire3 state.");
								return false;
							}
							if (b ^ negated)
								conditions.Add((i, j) => !Main.tile[i, j].wire3());
							else
								conditions.Add((i, j) => Main.tile[i, j].wire3());
						}
						#endregion
						break;
				}
			}
			return true;
		}
		public static void PrepareUndo(int x, int y, int x2, int y2, TSPlayer plr)
		{
			if (WorldEdit.Database.GetSqlType() == SqlType.Mysql)
				WorldEdit.Database.Query("INSERT IGNORE INTO WorldEdit VALUES (@0, -1, -1)", plr.UserAccountName);
			else
				WorldEdit.Database.Query("INSERT OR IGNORE INTO WorldEdit VALUES (@0, 0, 0)", plr.UserAccountName);
			WorldEdit.Database.Query("UPDATE WorldEdit SET Redo = -1 WHERE Account = @0", plr.UserAccountName);
			WorldEdit.Database.Query("UPDATE WorldEdit SET Undo = Undo + 1 WHERE Account = @0", plr.UserAccountName);

			int undoLevel = 0;
			using (var reader = WorldEdit.Database.QueryReader("SELECT Undo FROM WorldEdit WHERE Account = @0", plr.UserAccountName))
			{
				if (reader.Read())
					undoLevel = reader.Get<int>("Undo");
			}

			string path = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", plr.UserAccountName, undoLevel));
			SaveWorldSection(x, y, x2, y2, path);

			foreach (string fileName in Directory.EnumerateFiles("worldedit", String.Format("redo-{0}-*.dat", plr.UserAccountName)))
				File.Delete(fileName);
		}
		public static Tile ReadTile(this BinaryReader reader)
		{
			Tile tile = new Tile();
			byte flags = reader.ReadByte();
			byte flags2 = reader.ReadByte();

			tile.actuator((flags & 64) == 64);
			tile.halfBrick((flags & 32) == 32);
			tile.inActive((flags & 128) == 128);
			tile.slope((byte)((flags2 >> 4) & 3));
			tile.inActive((flags & 128) == 128);
			tile.wire((flags & 16) == 16);
			tile.wire2((flags2 & 1) == 1);
			tile.wire3((flags2 & 2) == 2);
			// Color
			if ((flags2 & 4) == 4)
				tile.color(reader.ReadByte());
			// Wall color
			if ((flags2 & 8) == 8)
				tile.wallColor(reader.ReadByte());
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
				tile.wall = reader.ReadByte();
			// Liquid
			if ((flags & 8) == 8)
			{
				tile.liquid = reader.ReadByte();
				tile.liquidType(reader.ReadByte());
			}
			return tile;
		}
		public static bool Redo(string accountName)
		{
			int redoLevel = 0;
			int undoLevel = 0;
			using (var reader = WorldEdit.Database.QueryReader("SELECT Redo, Undo FROM WorldEdit WHERE Account = @0", accountName))
			{
				if (reader.Read())
				{
					redoLevel = reader.Get<int>("Redo") - 1;
					undoLevel = reader.Get<int>("Undo") + 1;
				}
				else
					return false;
			}

			if (redoLevel < -1)
				return false;

			WorldEdit.Database.Query("UPDATE WorldEdit SET Redo = @0 WHERE Account = @1", redoLevel, accountName);
			WorldEdit.Database.Query("UPDATE WorldEdit SET Undo = @0 WHERE Account = @1", undoLevel, accountName);

			string redoPath = Path.Combine("worldedit", String.Format("redo-{0}-{1}.dat", accountName, redoLevel + 1));
			string undoPath = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", accountName, undoLevel));

			using (var reader = new BinaryReader(new GZipStream(new FileStream(redoPath, FileMode.Open), CompressionMode.Decompress)))
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				int x2 = x + reader.ReadInt32() - 1;
				int y2 = y + reader.ReadInt32() - 1;
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
			foreach (ServerSock sock in Netplay.serverSock)
			{
				if (sock.active)
				{
					for (int i = lowX; i <= highX; i++)
					{
						for (int j = lowY; j <= highY; j++)
							sock.tileSection[i, j] = false;
					}
				}
			}
		}
		public static void SaveWorldData(Tile[,] tiles, string path)
		{
			using (var fs = new FileStream(path, FileMode.Create))
			{
				using (var gz = new GZipStream(fs, CompressionMode.Compress))
				{
					using (var writer = new BinaryWriter(gz))
					{
						int xLen = tiles.GetLength(0);
						int yLen = tiles.GetLength(1);
						writer.Write(xLen);
						writer.Write(yLen);
						for (int i = 0; i < xLen; i++)
						{
							for (int j = 0; j < yLen; j++)
								writer.Write(tiles[i, j] ?? new Tile());
						}
					}
				}
			}
		}
		public static void SaveWorldSection(int x, int y, int x2, int y2, string path)
		{
			using (var writer = new BinaryWriter(new GZipStream(new FileStream(path, FileMode.Create), CompressionMode.Compress)))
			{
				writer.Write(x);
				writer.Write(y);
				writer.Write(x2 - x + 1);
				writer.Write(y2 - y + 1);
				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
						writer.Write(Main.tile[i, j]);
				}
			}
		}
		public static void Write(this BinaryWriter writer, Tile tile)
		{
			byte flags = 0;
			byte flags2 = 0;
			if (tile.active())
				flags |= 1;
			if (tile.wall != 0)
				flags |= 4;
			if (tile.liquid != 0)
				flags |= 8;
			if (tile.wire())
				flags |= 16;
			if (tile.halfBrick())
				flags |= 32;
			if (tile.actuator())
				flags |= 64;
			if (tile.inActive())
				flags |= 128;
			if (tile.wire2())
				flags2 |= 1;
			if (tile.wire3())
				flags2 |= 2;
			if (tile.color() != 0)
				flags2 |= 4;
			if (tile.wallColor() != 0)
				flags2 |= 8;
			flags2 |= (byte)(tile.slope() << 4);

			writer.Write(flags);
			writer.Write(flags2);
			if (tile.color() != 0)
				writer.Write(tile.color());
			if (tile.wallColor() != 0)
				writer.Write(tile.wallColor());
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
				writer.Write(tile.wall);
			if (tile.liquid != 0)
			{
				writer.Write(tile.liquid);
				writer.Write(tile.liquidType());
			}
		}
		public static bool Undo(string accountName)
		{
			int redoLevel = 0;
			int undoLevel = 0;
			using (var reader = WorldEdit.Database.QueryReader("SELECT Redo, Undo FROM WorldEdit WHERE Account = @0", accountName))
			{
				if (reader.Read())
				{
					redoLevel = reader.Get<int>("Redo") + 1;
					undoLevel = reader.Get<int>("Undo") - 1;
				}
				else
					return false;
			}

			if (undoLevel < -1)
				return false;

			WorldEdit.Database.Query("UPDATE WorldEdit SET Redo = @0 WHERE Account = @1", redoLevel, accountName);
			WorldEdit.Database.Query("UPDATE WorldEdit SET Undo = @0 WHERE Account = @1", undoLevel, accountName);

			string redoPath = Path.Combine("worldedit", String.Format("redo-{0}-{1}.dat", accountName, redoLevel));
			string undoPath = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", accountName, undoLevel + 1));

			using (var reader = new BinaryReader(new GZipStream(new FileStream(undoPath, FileMode.Open), CompressionMode.Decompress)))
			{
				int x = reader.ReadInt32();
				int y = reader.ReadInt32();
				int x2 = x + reader.ReadInt32() - 1;
				int y2 = y + reader.ReadInt32() - 1;
				SaveWorldSection(x, y, x2, y2, redoPath);
			}
			LoadWorldSection(undoPath);
			File.Delete(undoPath);

			return true;
		}
	}
}