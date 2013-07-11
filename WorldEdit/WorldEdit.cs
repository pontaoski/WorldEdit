using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Data;
using Hooks;
using Terraria;
using TShockAPI.DB;
using TShockAPI;
using WorldEdit.Commands;

namespace WorldEdit
{
	[APIVersion(1, 12)]
	public class WorldEdit : TerrariaPlugin
	{
		public static List<byte[]> BiomeConversions = new List<byte[]>();
		public static List<string> BiomeNames = new List<string>();
		public static List<byte> InvalidTiles = new List<byte>();
		public static PlayerInfo[] Players = new PlayerInfo[257];
		public static List<Func<int, int, TSPlayer, bool>> Selections = new List<Func<int, int, TSPlayer, bool>>();
		public static List<string> SelectionNames = new List<string>();
		public static Dictionary<string, byte> TileNames = new Dictionary<string, byte>();
		public static Dictionary<string, byte> WallNames = new Dictionary<string, byte>();

		public override string Author
		{
			get { return "MarioE"; }
		}
		private BlockingCollection<WECommand> CommandQueue = new BlockingCollection<WECommand>();
		private Thread CommandQueueThread;
		public override string Description
		{
			get { return "Adds commands for mass editing of blocks."; }
		}
		public override string Name
		{
			get { return "WorldEdit"; }
		}
		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public WorldEdit(Main game)
			: base(game)
		{
			for (int i = 0; i < 257; i++)
			{
				Players[i] = new PlayerInfo();
			}
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				GameHooks.Initialize -= OnInitialize;
				NetHooks.GetData -= OnGetData;
				ServerHooks.Leave -= OnLeave;

				CommandQueueThread.Abort();
				File.Delete(Path.Combine("worldedit", "clipboard-server.dat"));
				foreach (string fileName in Directory.EnumerateFiles("worldedit", "??do-server-*.dat"))
				{
					File.Delete(fileName);
				}
			}
		}
		public override void Initialize()
		{
			GameHooks.Initialize += OnInitialize;
			NetHooks.GetData += OnGetData;
			ServerHooks.Leave += OnLeave;
		}

		public static PlayerInfo GetPlayerInfo(TSPlayer player)
		{
			if (player.RealPlayer)
			{
				return Players[player.Index];
			}
			else
			{
				return Players[256];
			}
		}

		void OnGetData(GetDataEventArgs e)
		{
			if (!e.Handled && e.MsgID == PacketTypes.Tile)
			{
				PlayerInfo info = Players[e.Msg.whoAmI];
				if (info.pt != 0)
				{
					int X = BitConverter.ToInt32(e.Msg.readBuffer, e.Index + 1);
					int Y = BitConverter.ToInt32(e.Msg.readBuffer, e.Index + 5);
					if (X >= 0 && Y >= 0 && X < Main.maxTilesX && Y < Main.maxTilesY)
					{
						if (info.pt == 1)
						{
							info.x = X;
							info.y = Y;
							TShock.Players[e.Msg.whoAmI].SendInfoMessage("Set point 1.");
						}
						else if (info.pt == 3)
						{
							List<string> Reg = TShock.Regions.InAreaRegionName(X, Y);
							if (Reg.Count == 0)
							{
								TShock.Players[e.Msg.whoAmI].SendErrorMessage("No region exists there.");
								return;
							}
							Region curReg = TShock.Regions.GetRegionByName(Reg[0]);
							info.x = curReg.Area.X;
							info.y = curReg.Area.Y;
							info.x2 = curReg.Area.X + curReg.Area.Width;
							info.y2 = curReg.Area.Y + curReg.Area.Height;
							TShock.Players[e.Msg.whoAmI].SendInfoMessage("Set region.");
						}
						else
						{
							info.x2 = X;
							info.y2 = Y;
							TShock.Players[e.Msg.whoAmI].SendInfoMessage("Set point 2.");
						}
						info.pt = 0;
						e.Handled = true;
						TShock.Players[e.Msg.whoAmI].SendTileSquare(X, Y, 3);
					}
				}
			}
		}
		void OnInitialize()
		{
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.all", All, "/all"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.biome", Biome, "/biome"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.clear", ClearClipboard, "/clearclipboard"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.history.clear", ClearHistory, "/clearhistory"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.contract", Contract, "/contract"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.copy", Copy, "/copy"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.cut", Cut, "/cut"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.utils.drain", Drain, "/drain"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.expand", Expand, "/expand"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.utils.fixgrass", FixGrass, "/fixgrass"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.flip", Flip, "/flip"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.utils.flood", Flood, "/flood"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.inset", Inset, "/inset"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.outset", Outset, "/outset"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.paste", Paste, "/paste"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.point", Point1, "/point1"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.point", Point2, "/point2"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.point", PointCmd, "/point") { AllowServer = false });
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.history.redo", Redo, "/redo"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.region", Region, "/region"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.replace", Replace, "/replace"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.replacewall", ReplaceWall, "/replacewall"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.rotate", Rotate, "/rotate"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.schematic", Schematic, "/schematic", "/schem"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.selecttype", Select, "/select"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.set", Set, "/set"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.setwall", SetWall, "/setwall"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.shift", Shift, "/shift"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.size", Size, "/size"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.history.undo", Undo, "/undo"));

			#region Biomes
			// 255 => remove
			byte[] Corruption = { 0, 25, 112, 23, 24, 255, 255, 32 };
			byte[] Hallow = { 0, 117, 116, 109, 110, 113, 52, 255 };
			byte[] Jungle = { 59, 1, 53, 60, 61, 74, 62, 69 };
			byte[] Mushroom = { 59, 1, 53, 70, 71, 255, 255, 255 };
			byte[] Normal = { 0, 1, 53, 2, 3, 73, 52, 255 };
			BiomeConversions.Add(Corruption);
			BiomeConversions.Add(Hallow);
			BiomeConversions.Add(Jungle);
			BiomeConversions.Add(Mushroom);
			BiomeConversions.Add(Normal);
			BiomeNames.Add("corruption");
			BiomeNames.Add("hallow");
			BiomeNames.Add("jungle");
			BiomeNames.Add("mushroom");
			BiomeNames.Add("normal");
			#endregion
			#region Invalid Tiles
			InvalidTiles.Add(33);
			InvalidTiles.Add(49);
			InvalidTiles.Add(78);
			#endregion
			#region Selections
			Selections.Add((i, j, plr) => ((i + j) & 1) == 0);
			Selections.Add((i, j, plr) => ((i + j) & 1) == 1);
			Selections.Add((i, j, plr) =>
			{
				PlayerInfo info = GetPlayerInfo(plr);

				int X = Math.Min(info.x, info.x2);
				int Y = Math.Min(info.y, info.y2);
				int X2 = Math.Max(info.x, info.x2);
				int Y2 = Math.Max(info.y, info.y2);

				Vector2 center = new Vector2((float)(X2 - X) / 2, (float)(Y2 - Y) / 2);
				float major = Math.Max(center.X, center.Y);
				float minor = Math.Min(center.X, center.Y);
				if (center.Y > center.X)
				{
					float temp = major;
					major = minor;
					minor = temp;
				}
				return (i - center.X - X) * (i - center.X - X) / (major * major) + (j - center.Y - Y) * (j - center.Y - Y) / (minor * minor) <= 1;
			});
			Selections.Add((i, j, plr) => true);
			Selections.Add((i, j, plr) =>
			{
				PlayerInfo info = GetPlayerInfo(plr);
				return i == info.x || i == info.x2 || j == info.y || j == info.y2;
			});
			SelectionNames.Add("altcheckers");
			SelectionNames.Add("checkers");
			SelectionNames.Add("ellipse");
			SelectionNames.Add("normal");
			SelectionNames.Add("outline");
			#endregion
			#region Tile Names
			TileNames.Add("dirt", 0);
			TileNames.Add("stone", 1);
			TileNames.Add("grass", 2);
			TileNames.Add("iron", 6);
			TileNames.Add("copper", 7);
			TileNames.Add("gold", 8);
			TileNames.Add("silver", 9);
			TileNames.Add("platform", 19);
			TileNames.Add("demonite", 22);
			TileNames.Add("corrupt grass", 23);
			TileNames.Add("ebonstone", 25);
			TileNames.Add("wood", 30);
			TileNames.Add("meteorite", 37);
			TileNames.Add("gray brick", 38);
			TileNames.Add("red brick", 39);
			TileNames.Add("clay", 40);
			TileNames.Add("blue brick", 41);
			TileNames.Add("green brick", 43);
			TileNames.Add("pink brick", 44);
			TileNames.Add("gold brick", 45);
			TileNames.Add("silver brick", 46);
			TileNames.Add("copper brick", 47);
			TileNames.Add("spike", 48);
			TileNames.Add("cobweb", 51);
			TileNames.Add("sand", 53);
			TileNames.Add("glass", 54);
			TileNames.Add("obsidian", 56);
			TileNames.Add("ash", 57);
			TileNames.Add("hellstone", 58);
			TileNames.Add("mud", 59);
			TileNames.Add("jungle grass", 60);
			TileNames.Add("sapphire", 63);
			TileNames.Add("ruby", 64);
			TileNames.Add("emerald", 65);
			TileNames.Add("topaz", 66);
			TileNames.Add("amethyst", 67);
			TileNames.Add("diamond", 68);
			TileNames.Add("mushroom grass", 70);
			TileNames.Add("obsidian brick", 75);
			TileNames.Add("hellstone brick", 76);
			TileNames.Add("cobalt", 107);
			TileNames.Add("mythril", 108);
			TileNames.Add("hallowed grass", 109);
			TileNames.Add("adamantite", 111);
			TileNames.Add("ebonsand", 112);
			TileNames.Add("pearlsand", 116);
			TileNames.Add("pearlstone", 117);
			TileNames.Add("pearlstone brick", 118);
			TileNames.Add("iridescent brick", 119);
			TileNames.Add("mudstone block", 120);
			TileNames.Add("cobalt brick", 121);
			TileNames.Add("mythril brick", 122);
			TileNames.Add("silt", 123);
			TileNames.Add("wooden beam", 124);
			TileNames.Add("ice", 127);
			TileNames.Add("active stone", 130);
			TileNames.Add("inactive stone", 131);
			TileNames.Add("demonite brick", 140);
			TileNames.Add("candy cane", 145);
			TileNames.Add("green candy cane", 146);
			TileNames.Add("snow", 147);
			TileNames.Add("snow brick", 148);
			// These are not actually correct, but are for ease of usage.
			TileNames.Add("air", 149);
			TileNames.Add("lava", 150);
			TileNames.Add("water", 151);
			TileNames.Add("wire", 152);
			#endregion
			#region Wall Names
			WallNames.Add("air", 0);
			WallNames.Add("stone", 1);
			WallNames.Add("ebonstone", 3);
			WallNames.Add("wood", 4);
			WallNames.Add("gray brick", 5);
			WallNames.Add("red brick", 6);
			WallNames.Add("gold brick", 10);
			WallNames.Add("silver brick", 11);
			WallNames.Add("copper brick", 12);
			WallNames.Add("hellstone brick", 13);
			WallNames.Add("mud", 15);
			WallNames.Add("dirt", 16);
			WallNames.Add("blue brick", 17);
			WallNames.Add("green brick", 18);
			WallNames.Add("pink brick", 19);
			WallNames.Add("obsidian brick", 20);
			WallNames.Add("glass", 21);
			WallNames.Add("pearlstone brick", 22);
			WallNames.Add("iridescent brick", 23);
			WallNames.Add("mudstone brick", 24);
			WallNames.Add("cobalt brick", 25);
			WallNames.Add("mythril brick", 26);
			WallNames.Add("planked", 27);
			WallNames.Add("pearlstone", 28);
			WallNames.Add("candy cane", 29);
			WallNames.Add("green candy cane", 30);
			WallNames.Add("snow brick", 31);
			#endregion
			CommandQueueThread = new Thread(QueueCallback);
			CommandQueueThread.Name = "WorldEdit Callback";
			CommandQueueThread.Start();
			Directory.CreateDirectory("worldedit");
		}
		void OnLeave(int plr)
		{
			File.Delete(Path.Combine("worldedit", String.Format("clipboard-{0}.dat", plr)));
			foreach (string fileName in Directory.EnumerateFiles("worldedit", String.Format("??do-{0}-*.dat", plr)))
			{
				File.Delete(fileName);
			}
			Players[plr] = new PlayerInfo();
		}

		void QueueCallback(object t)
		{
			while (!Netplay.disconnect)
			{
				WECommand command = CommandQueue.Take();
				command.Position();
				command.Execute();
			}
		}

		void All(CommandArgs e)
		{
			PlayerInfo info = GetPlayerInfo(e.Player);
			info.x = info.y = 0;
			info.x2 = Main.maxTilesX - 1;
			info.y2 = Main.maxTilesY - 1;
			e.Player.SendSuccessMessage("Selected all tiles.");
		}
		void Biome(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //biome <biome1> <biome2>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			byte biome1 = (byte)BiomeNames.IndexOf(e.Parameters[0].ToLower());
			byte biome2 = (byte)BiomeNames.IndexOf(e.Parameters[1].ToLower());
			if (biome1 == 255 || biome2 == 255)
			{
				e.Player.SendErrorMessage("Invalid biome.");
				return;
			}

			int x = Math.Min(info.x, info.x2);
			int y = Math.Min(info.y, info.y2);
			int x2 = Math.Max(info.x, info.x2);
			int y2 = Math.Max(info.y, info.y2);
			CommandQueue.Add(new BiomeCommand(x, y, x2, y2, e.Player, biome1, biome2));
		}
		void ClearClipboard(CommandArgs e)
		{
			File.Delete(Path.Combine("worldedit", String.Format("clipboard-{0}.dat", e.Player.Index)));
			e.Player.SendSuccessMessage("Cleared clipboard.");
		}
		void ClearHistory(CommandArgs e)
		{
			foreach (string fileName in Directory.EnumerateFiles("worldedit", "??do-" + e.Player.Index + "-*.dat"))
			{
				File.Delete(fileName);
			}
			e.Player.SendSuccessMessage("Cleared history.");
		}
		void Contract(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //contract <amount> <direction>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			int amount;
			if (!int.TryParse(e.Parameters[0], out amount) || amount < 0)
			{
				e.Player.SendErrorMessage("Invalid contraction amount.");
				return;
			}
			switch (e.Parameters[1].ToLower())
			{
				case "d":
				case "down":
					if (info.y < info.y2)
					{
						info.y += amount;
					}
					else
					{
						info.y2 += amount;
					}
					e.Player.SendSuccessMessage(String.Format("Contracted selection down {0}.", amount));
					break;

				case "l":
				case "left":
					if (info.x < info.x2)
					{
						info.x2 -= amount;
					}
					else
					{
						info.x -= amount;
					}
					e.Player.SendSuccessMessage(String.Format("Contracted selection left {0}.", amount));
					break;

				case "r":
				case "right":
					if (info.x < info.x2)
					{
						info.x += amount;
					}
					else
					{
						info.x2 += amount;
					}
					e.Player.SendSuccessMessage(String.Format("Contracted selection right {0}.", amount));
					break;

				case "u":
				case "up":
					if (info.y < info.y2)
					{
						info.y2 -= amount;
					}
					else
					{
						info.y -= amount;
					}
					e.Player.SendSuccessMessage(String.Format("Contracted selection up {0}.", amount));
					break;

				default:
					e.Player.SendSuccessMessage("Invalid direction.");
					break;
			}
		}
		void Copy(CommandArgs e)
		{
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			int x = Math.Min(info.x, info.x2);
			int y = Math.Min(info.y, info.y2);
			int x2 = Math.Max(info.x, info.x2);
			int y2 = Math.Max(info.y, info.y2);
			CommandQueue.Add(new CopyCommand(x, y, x2, y2, e.Player));
		}
		void Cut(CommandArgs e)
		{
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			int x = Math.Min(info.x, info.x2);
			int y = Math.Min(info.y, info.y2);
			int x2 = Math.Max(info.x, info.x2);
			int y2 = Math.Max(info.y, info.y2);
			CommandQueue.Add(new CutCommand(x, y, x2, y2, e.Player));
		}
		void Drain(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //drain <radius>");
				return;
			}

			int radius;
			if (!int.TryParse(e.Parameters[0], out radius) || radius <= 0)
			{
				e.Player.SendErrorMessage("Invalid radius.");
				return;
			}
			int x = e.Player.TileX - radius;
			int x2 = e.Player.TileX + radius + 2;
			int y = e.Player.TileY - radius + 1;
			int y2 = e.Player.TileY + radius + 1;
			CommandQueue.Add(new DrainCommand(x, y, x2, y2, e.Player));
		}
		void Expand(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //expand <amount> <direction>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			int amount;
			if (!int.TryParse(e.Parameters[0], out amount) || amount < 0)
			{
				e.Player.SendErrorMessage("Invalid expansion amount.");
				return;
			}
			switch (e.Parameters[1].ToLower())
			{
				case "d":
				case "down":
					if (info.y < info.y2)
					{
						info.y2 += amount;
					}
					else
					{
						info.y += amount;
					}
					e.Player.SendSuccessMessage(String.Format("Expanded selection down {0}.", amount));
					break;

				case "l":
				case "left":
					if (info.x < info.x2)
					{
						info.x -= amount;
					}
					else
					{
						info.x2 -= amount;
					}
					e.Player.SendSuccessMessage(String.Format("Expanded selection left {0}.", amount));
					break;

				case "r":
				case "right":
					if (info.x < info.x2)
					{
						info.x2 += amount;
					}
					else
					{
						info.x += amount;
					}
					e.Player.SendSuccessMessage(String.Format("Expanded selection right {0}.", amount));
					break;

				case "u":
				case "up":
					if (info.y < info.y2)
					{
						info.y -= amount;
					}
					else
					{
						info.y2 -= amount;
					}
					e.Player.SendSuccessMessage(String.Format("Expanded selection up {0}.", amount));
					break;

				default:
					e.Player.SendErrorMessage("Invalid direction.");
					break;
			}
		}
		void FixGrass(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: /fixgrass <radius>");
				return;
			}

			int radius;
			if (!int.TryParse(e.Parameters[0], out radius) || radius <= 0)
			{
				e.Player.SendErrorMessage("Invalid radius.");
				return;
			}
			int x = e.Player.TileX - radius;
			int x2 = e.Player.TileX + radius + 2;
			int y = e.Player.TileY - radius + 1;
			int y2 = e.Player.TileY + radius + 1;
			CommandQueue.Add(new FixGrassCommand(x, y, x2, y2, e.Player));
		}
		void Flood(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //flood <lava|water> <radius>");
				return;
			}
			string liquid = e.Parameters[0].ToLower();
			if (liquid != "water" && liquid != "lava")
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //flood <lava|water> <radius>");
				return;
			}

			int radius;
			if (!int.TryParse(e.Parameters[1], out radius) || radius <= 0)
			{
				e.Player.SendErrorMessage("Invalid radius.");
				return;
			}
			int x = e.Player.TileX - radius;
			int x2 = e.Player.TileX + radius + 2;
			int y = e.Player.TileY - radius + 1;
			int y2 = e.Player.TileY + radius + 1;
			CommandQueue.Add(new FloodCommand(x, y, x2, y2, e.Player, liquid == "lava"));
		}
		void Flip(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //flip <direction>");
				return;
			}
			if (!Tools.HasClipboard(e.Player))
			{
				e.Player.SendErrorMessage("Invalid clipboard.");
				return;
			}

			byte flip = 0;
			foreach (char c in e.Parameters[0].ToLower())
			{
				if (c == 'x')
				{
					flip ^= 1;
				}
				else if (c == 'y')
				{
					flip ^= 2;
				}
				else
				{
					e.Player.SendErrorMessage("Invalid direction.");
					return;
				}
			}
			CommandQueue.Add(new FlipCommand(e.Player, flip));
		}
		void Inset(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //inset <amount>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			int amount;
			if (!int.TryParse(e.Parameters[0], out amount) || amount < 0)
			{
				e.Player.SendErrorMessage("Invalid inset amount.");
				return;
			}
			if (info.x < info.x2)
			{
				info.x += amount;
				info.x2 -= amount;
			}
			else
			{
				info.x -= amount;
				info.x2 += amount;
			}
			if (info.y < info.y2)
			{
				info.y += amount;
				info.y2 -= amount;
			}
			else
			{
				info.y -= amount;
				info.y2 += amount;
			}
			e.Player.SendSuccessMessage(String.Format("Inset selection by {0}.", amount));
		}
		void Outset(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //outset <amount>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			int amount;
			if (!int.TryParse(e.Parameters[0], out amount) || amount < 0)
			{
				e.Player.SendErrorMessage("Invalid outset amount.");
				return;
			}
			if (info.x < info.x2)
			{
				info.x -= amount;
				info.x2 += amount;
			}
			else
			{
				info.x += amount;
				info.x2 -= amount;
			}
			if (info.y < info.y2)
			{
				info.y -= amount;
				info.y2 += amount;
			}
			else
			{
				info.y += amount;
				info.y2 -= amount;
			}
			e.Player.SendSuccessMessage(String.Format("Outset selection by {0}.", amount));
		}
		void Paste(CommandArgs e)
		{
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1)
			{
				e.Player.SendErrorMessage("Invalid first point.");
				return;
			}
			if (!Tools.HasClipboard(e.Player))
			{
				e.Player.SendErrorMessage("Invalid clipboard.");
				return;
			}

			CommandQueue.Add(new PasteCommand(info.x, info.y, e.Player));
		}
		void Point1(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //point1 <x> <y>");
				return;
			}

			int x, y;
			if (!int.TryParse(e.Parameters[0], out x) || x < 0 || x > Main.maxTilesX
				|| !int.TryParse(e.Parameters[0], out y) || y < 0 || y > Main.maxTilesY)
			{
				e.Player.SendErrorMessage("Invalid coordinates.");
				return;
			}

			PlayerInfo info = GetPlayerInfo(e.Player);
			info.x = x;
			info.y = y;
		}
		void Point2(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //point2 <x> <y>");
				return;
			}

			int x, y;
			if (!int.TryParse(e.Parameters[0], out x) || x < 0 || x > Main.maxTilesX
				|| !int.TryParse(e.Parameters[0], out y) || y < 0 || y > Main.maxTilesY)
			{
				e.Player.SendErrorMessage("Invalid coordinates.");
				return;
			}

			PlayerInfo info = GetPlayerInfo(e.Player);
			info.x2 = x;
			info.y2 = y;
		}
		void PointCmd(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //point <1|2>");
				return;
			}

			switch (e.Parameters[0])
			{
				case "1":
					Players[e.Player.Index].pt = 1;
					e.Player.SendInfoMessage("Hit a block to set point 1.");
					break;
				case "2":
					Players[e.Player.Index].pt = 2;
					e.Player.SendInfoMessage("Hit a block to set point 2.");
					break;
				default:
					e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //point <1|2>");
					break;
			}
		}
		void Redo(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //redo [steps]");
				return;
			}

			int steps = 1;
			if (e.Parameters.Count == 1 && (!int.TryParse(e.Parameters[0], out steps) || steps <= 0))
			{
				e.Player.SendErrorMessage("Invalid number of steps.");
				return;
			}
			CommandQueue.Add(new RedoCommand(e.Player, steps));
		}
		void Region(CommandArgs e)
		{
			if (e.Parameters.Count > 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //region [region name] | [x] [y]");
				return;
			}
			else if (e.Parameters.Count == 0)
			{
				Players[e.Player.Index].pt = 3;
				e.Player.SendInfoMessage("Hit a block to use that region.");
			}
			else if (e.Parameters.Count == 1)
			{
				PlayerInfo info = GetPlayerInfo(e.Player);
				Region curReg = TShock.Regions.ZacksGetRegionByName(e.Parameters[0].ToLower());
				if (curReg == null)
				{
					e.Player.SendErrorMessage("Invalid region.");
				}
				else
				{
					info.x = curReg.Area.X;
					info.y = curReg.Area.Y;
					info.x2 = curReg.Area.X + curReg.Area.Width;
					info.y2 = curReg.Area.Y + curReg.Area.Height;
				}
			}
			else
			{
				int x;
				int y;
				if (!int.TryParse(e.Parameters[0], out x) || !int.TryParse(e.Parameters[1], out y))
				{
					e.Player.SendErrorMessage("Invalid coordinates.");
					return;
				}

				PlayerInfo info = GetPlayerInfo(e.Player);
				List<string> regions = TShock.Regions.InAreaRegionName(x, y);
				if (regions.Count >= 1)
				{
					if (regions.Count > 1)
					{
						e.Player.SendInfoMessage("Overlapping regions; using first encountered...");
						/*int q = 0;
						foreach (string reg in regions)
						{
							e.Player.SendMessage("Region " + q + ": " + reg);
							q++;
						}*/

					}
					Region curReg = TShock.Regions.GetRegionByName(regions[0]);

					info.x = curReg.Area.X;
					info.y = curReg.Area.Y;
					info.x2 = curReg.Area.X + curReg.Area.Width;
					info.y2 = curReg.Area.Y + curReg.Area.Height;
				}
				else
				{
					e.Player.SendErrorMessage("Invalid region.");
				}
			}
		}
		void Replace(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //replace <tile1> <tile2>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<byte> values1 = Tools.GetTileByName(e.Parameters[0].ToLower());
			List<byte> values2 = Tools.GetTileByName(e.Parameters[1].ToLower());
			if (values1.Count == 0)
			{
				e.Player.SendErrorMessage("Invalid tile.");
			}
			else if (values1.Count > 1)
			{
				e.Player.SendErrorMessage("More than one tile matched.");
			}
			else if (values2.Count == 0)
			{
				e.Player.SendErrorMessage("Invalid tile.");
			}
			else if (values2.Count > 1)
			{
				e.Player.SendErrorMessage("More than one tile matched.");
			}
			else
			{
				CommandQueue.Add(new ReplaceCommand(info.x, info.y, info.x2, info.y2, e.Player, values1[0], values2[0]));
			}
		}
		void ReplaceWall(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //replacewall <wall1> <wall2>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<byte> values1 = Tools.GetWallByName(e.Parameters[0].ToLower());
			List<byte> values2 = Tools.GetWallByName(e.Parameters[1].ToLower());
			if (values1.Count == 0)
			{
				e.Player.SendErrorMessage("Invalid wall.");
			}
			else if (values1.Count > 1)
			{
				e.Player.SendErrorMessage("More than one wall matched.");
			}
			else if (values2.Count == 0)
			{
				e.Player.SendErrorMessage("Invalid wall.");
			}
			else if (values2.Count > 1)
			{
				e.Player.SendErrorMessage("More than one wall matched.");
			}
			else
			{
				CommandQueue.Add(new ReplaceWallCommand(info.x, info.y, info.x2, info.y2, e.Player, values1[0], values2[0]));
			}
		}
		void Rotate(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //rotate <angle>");
				return;
			}
			if (!Tools.HasClipboard(e.Player))
			{
				e.Player.SendErrorMessage("Invalid clipboard.");
				return;
			}

			int degrees;
			if (!int.TryParse(e.Parameters[0], out degrees) || degrees % 90 != 0)
			{
				e.Player.SendErrorMessage("Invalid angle.");
				return;
			}
			CommandQueue.Add(new RotateCommand(e.Player, degrees));
		}
		void Schematic(CommandArgs e)
		{
			string subCmd = e.Parameters.Count == 0 ? "help" : e.Parameters[0].ToLower();
			switch (subCmd)
			{
				case "del":
				case "delete":
					{
						if (e.Parameters.Count != 2)
						{
							e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //schematic delete <name>");
							break;
						}
						string schematicPath = Path.Combine("worldedit", String.Format("schematic-{0}.dat", e.Parameters[1]));
						if (!File.Exists(schematicPath))
						{
							e.Player.SendErrorMessage("Invalid schematic.");
							break;
						}
						File.Delete(schematicPath);
						e.Player.SendErrorMessage("Deleted schematic.");
					}
					break;
				case "help":
					e.Player.SendSuccessMessage("Schematics commands:");
					e.Player.SendInfoMessage("//schematic delete <name>");
					e.Player.SendInfoMessage("//schematic list [page]");
					e.Player.SendInfoMessage("//schematic load <name>");
					e.Player.SendInfoMessage("//schematic save <name>");
					break;
				case "list":
					{
						if (e.Parameters.Count > 2)
						{
							e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //schematic list [page]");
							break;
						}

						List<string> schematics = new List<string>(Directory.EnumerateFiles("worldedit", "schematic-*.dat"));
						if (schematics.Count == 0)
						{
							e.Player.SendErrorMessage("No schematics exist.");
							break;
						}

						int maxPages = (int)Math.Ceiling(schematics.Count / 15d);
						int page = 1;
						if (e.Parameters.Count == 2)
						{
							if (!int.TryParse(e.Parameters[1], out page) || page <= 0 || page > maxPages)
							{
								e.Player.SendErrorMessage("Invalid page.");
								break;
							}
						}
						page--;

						e.Player.SendSuccessMessage(String.Format("Schematics: (Page {0}/{1})", page + 1, maxPages));
						StringBuilder line = new StringBuilder();
						for (int i = page * 15; i < page * 15 + 15 && i < schematics.Count; i++)
						{
							string schematic = schematics[i];
							line.Append(schematic.Substring(20, schematic.Length - 24));
							if ((i + 1) % 5 == 0)
							{
								e.Player.SendInfoMessage(line.ToString());
								line.Clear();
							}
							else if (i != schematics.Count - 1)
							{
								line.Append(", ");
							}
						}
						if (line.Length != 0)
						{
							e.Player.SendInfoMessage(line.ToString());
						}
					}
					break;
				case "load":
					{
						if (e.Parameters.Count != 2)
						{
							e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //schematic load <name>");
							break;
						}
						string schematicPath = Path.Combine("worldedit", String.Format("schematic-{0}.dat", e.Parameters[1]));
						if (!File.Exists(schematicPath))
						{
							e.Player.SendErrorMessage("Invalid schematic.");
							return;
						}
						string id = e.Player.RealPlayer ? e.Player.Index.ToString() : "server";
						string clipboardPath = Path.Combine("worldedit", String.Format("clipboard-{0}.dat", id));
						File.Copy(schematicPath, clipboardPath, true);
						e.Player.SendSuccessMessage("Loaded schematic to clipboard.");
					}
					break;
				case "add":
				case "save":
					{
						if (e.Parameters.Count != 2)
						{
							e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //schematic save <name>");
							break;
						}
						string id = e.Player.RealPlayer ? e.Player.Index.ToString() : "server";
						string clipboardPath = Path.Combine("worldedit", String.Format("clipboard-{0}.dat", id));
						if (!File.Exists(clipboardPath))
						{
							e.Player.SendErrorMessage("Invalid clipboard.");
							break;
						}
						string schematicPath = Path.Combine("worldedit", String.Format("schematic-{0}.dat", e.Parameters[1]));
						File.Copy(clipboardPath, schematicPath, true);
						e.Player.SendSuccessMessage("Saved clipboard to schematic.");
					}
					break;
				default:
					e.Player.SendErrorMessage("Unknown subcommand.");
					break;
			}
		}
		void Select(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //select <selection type>");
				return;
			}

			string name = e.Parameters[0].ToLower();
			int ID = SelectionNames.IndexOf(name);
			if (ID < 0)
			{
				e.Player.SendErrorMessage("Invalid selection type.");
				return;
			}
			GetPlayerInfo(e.Player).select = ID;
			e.Player.SendSuccessMessage(String.Format("Set selection type to {0}.", name));
		}
		void Set(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //set <tile>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<byte> values = Tools.GetTileByName(e.Parameters[0].ToLower());
			if (e.Parameters[0].ToLower() == "nowire")
			{
				values.Add(153);
			}
			if (values.Count == 0)
			{
				e.Player.SendErrorMessage("Invalid tile.");
			}
			else if (values.Count > 1)
			{
				e.Player.SendErrorMessage("More than one tile matched.");
			}
			else
			{
				CommandQueue.Add(new SetCommand(info.x, info.y, info.x2, info.y2, e.Player, values[0]));
			}
		}
		void SetWall(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //setwall <wall>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<byte> values = Tools.GetWallByName(e.Parameters[0].ToLower());
			if (values.Count == 0)
			{
				e.Player.SendErrorMessage("Invalid wall.");
			}
			else if (values.Count > 1)
			{
				e.Player.SendErrorMessage("More than one wall matched.");
			}
			else
			{
				CommandQueue.Add(new SetWallCommand(info.x, info.y, info.x2, info.y2, e.Player, values[0]));
			}
		}
		void Shift(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //shift <amount> <direction>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			int amount;
			if (!int.TryParse(e.Parameters[0], out amount) || amount < 0)
			{
				e.Player.SendErrorMessage("Invalid shift amount.");
				return;
			}
			switch (e.Parameters[1].ToLower())
			{
				case "d":
				case "down":
					info.y += amount;
					info.y2 += amount;
					e.Player.SendSuccessMessage(String.Format("Shifted selection down {0}.", amount));
					break;

				case "l":
				case "left":
					info.x -= amount;
					info.x2 -= amount;
					e.Player.SendSuccessMessage(String.Format("Shifted selection left {0}.", amount));
					break;

				case "r":
				case "right":
					info.x += amount;
					info.x2 += amount;
					e.Player.SendSuccessMessage(String.Format("Shifted selection right {0}.", amount));
					break;

				case "u":
				case "up":
					info.y -= amount;
					info.y2 -= amount;
					e.Player.SendSuccessMessage(String.Format("Shifted selection up {0}.", amount));
					break;

				default:
					e.Player.SendErrorMessage("Invalid direction.");
					break;
			}
		}
		void Size(CommandArgs e)
		{
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}
			int lenX = Math.Abs(info.x - info.x2) + 1;
			int lenY = Math.Abs(info.y - info.y2) + 1;
			e.Player.SendInfoMessage(String.Format("Selection size: {0} x {1}", lenX, lenY));
		}
		void Undo(CommandArgs e)
		{
			if (e.Parameters.Count > 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //undo [steps]");
				return;
			}

			int steps = 1;
			if (e.Parameters.Count == 1 && (!int.TryParse(e.Parameters[0], out steps) || steps <= 0))
			{
				e.Player.SendErrorMessage("Invalid number of steps.");
				return;
			}
			CommandQueue.Add(new UndoCommand(e.Player, steps));
		}
	}
}