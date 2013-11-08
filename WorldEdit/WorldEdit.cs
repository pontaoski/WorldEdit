using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Data;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using WorldEdit.Commands;

namespace WorldEdit
{
	[ApiVersion(1, 14)]
	public class WorldEdit : TerrariaPlugin
	{
		public static List<int[]> BiomeConversions = new List<int[]>();
		public static List<string> BiomeNames = new List<string>();
		public static List<string> ColorNames = new List<string>();
		public static List<int> InvalidTiles = new List<int>();
		public static PlayerInfo[] Players = new PlayerInfo[257];
		public static List<Func<int, int, TSPlayer, bool>> Selections = new List<Func<int, int, TSPlayer, bool>>();
		public static List<string> SelectionNames = new List<string>();
		public static Dictionary<string, int> TileNames = new Dictionary<string, int>();
		public static Dictionary<string, int> WallNames = new Dictionary<string, int>();

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
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);

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
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
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
		void OnInitialize(EventArgs e)
		{
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.all", All, "/all"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.biome", Biome, "/biome"));
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
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.paint", Paint, "/paint"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.paintwall", PaintWall, "/paintwall"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.paste", Paste, "/paste"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.point", Point1, "/point1"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.point", Point2, "/point2"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.point", PointCmd, "/point") { AllowServer = false });
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.history.redo", Redo, "/redo"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.region", RegionCmd, "/region"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.replace", Replace, "/replace"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.replacepaint", ReplacePaint, "/replacepaint"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.replacepaintwall", ReplacePaintWall, "/replacepaintwall"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.replacewall", ReplaceWall, "/replacewall"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.clipboard.rotate", Rotate, "/rotate"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.schematic", Schematic, "/schematic", "/schem"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.selecttype", Select, "/select"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.set", Set, "/set"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.setwall", SetWall, "/setwall"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.region.setwire", SetWire, "/setwire"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.shift", Shift, "/shift"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.selection.size", Size, "/size"));
			TShockAPI.Commands.ChatCommands.Add(new Command("worldedit.history.undo", Undo, "/undo"));

			#region Biomes
			// Format: dirt, stone, sand, grass, plants, tall plants, vines, thorn

			BiomeConversions.Add(new[] { 0, 203, 234, 199, -1, -1, 205, 32 });
			BiomeConversions.Add(new[] { 0, 25, 112, 23, 24, -1, -1, 32 });
			BiomeConversions.Add(new[] { 0, 117, 116, 109, 110, 113, 52, -1 });
			BiomeConversions.Add(new[] { 59, 1, 53, 60, 61, 74, 62, 69 });
			BiomeConversions.Add(new[] { 59, 1, 53, 70, 71, -1, -1, -1 });
			BiomeConversions.Add(new[] { 0, 1, 53, 2, 3, 73, 52, -1 });
			BiomeConversions.Add(new[] { 147, 161, 53, 147, -1, -1, -1, -1 });
			BiomeNames.Add("crimson");
			BiomeNames.Add("corruption");
			BiomeNames.Add("hallow");
			BiomeNames.Add("jungle");
			BiomeNames.Add("mushroom");
			BiomeNames.Add("normal");
			BiomeNames.Add("snow");
			#endregion
			#region Color Names
			ColorNames.Add("blank");
			ColorNames.Add("red");
			ColorNames.Add("orange");
			ColorNames.Add("yellow");
			ColorNames.Add("lime");
			ColorNames.Add("green");
			ColorNames.Add("teal");
			ColorNames.Add("cyan");
			ColorNames.Add("sky blue");
			ColorNames.Add("blue");
			ColorNames.Add("purple");
			ColorNames.Add("violet");
			ColorNames.Add("pink");
			ColorNames.Add("deep red");
			ColorNames.Add("deep orange");
			ColorNames.Add("deep yellow");
			ColorNames.Add("deep lime");
			ColorNames.Add("deep green");
			ColorNames.Add("deep teal");
			ColorNames.Add("deep cyan");
			ColorNames.Add("deep sky blue");
			ColorNames.Add("deep blue");
			ColorNames.Add("deep purple");
			ColorNames.Add("deep violet");
			ColorNames.Add("deep pink");
			ColorNames.Add("black");
			ColorNames.Add("white");
			ColorNames.Add("gray");
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
			TileNames.Add("air", -1);
			TileNames.Add("lava", -2);
			TileNames.Add("honey", -3);
			TileNames.Add("water", -4);

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
			TileNames.Add("icerod", 127);
			TileNames.Add("active stone", 130);
			TileNames.Add("inactive stone", 131);
			TileNames.Add("demonite brick", 140);
			TileNames.Add("candy cane", 145);
			TileNames.Add("green candy cane", 146);
			TileNames.Add("snow", 147);
			TileNames.Add("snow brick", 148);
			TileNames.Add("adamantite beam", 150);
			TileNames.Add("sandstone brick", 151);
			TileNames.Add("ebonstone brick", 152);
			TileNames.Add("red stucco", 153);
			TileNames.Add("yellow stucco", 154);
			TileNames.Add("green stucco", 155);
			TileNames.Add("gray stucco", 156);
			TileNames.Add("ebonwood", 157);
			TileNames.Add("rich mahogany", 158);
			TileNames.Add("pearlwood", 159);
			TileNames.Add("rainbow brick", 160);
			TileNames.Add("ice", 161);
			TileNames.Add("thin ice", 162);
			TileNames.Add("purple ice", 162);
			TileNames.Add("pink ice", 162);
			TileNames.Add("tin", 166);
			TileNames.Add("lead", 167);
			TileNames.Add("tungsten", 168);
			TileNames.Add("platinum", 169);
			TileNames.Add("tin brick", 175);
			TileNames.Add("tungsten brick", 176);
			TileNames.Add("platinum brick", 177);
			TileNames.Add("cactus", 188);
			TileNames.Add("cloud", 189);
			TileNames.Add("glowing mushroom", 190);
			TileNames.Add("tree", 191);
			TileNames.Add("leaf", 192);
			TileNames.Add("slime", 193);
			TileNames.Add("bone", 194);
			TileNames.Add("flesh", 195);
			TileNames.Add("rain cloud", 196);
			TileNames.Add("frozen slime", 197);
			TileNames.Add("asphalt", 198);
			TileNames.Add("crimson dirt", 199);
			TileNames.Add("red ice", 200);
			TileNames.Add("sunplate", 202);
			TileNames.Add("crimstone", 203);
			TileNames.Add("crimtane", 204);
			TileNames.Add("ice brick", 206);
			TileNames.Add("shadewood", 208);
			TileNames.Add("chlorophyte", 211);
			TileNames.Add("palladium", 221);
			TileNames.Add("orichalcum", 222);
			TileNames.Add("titanium", 223);
			TileNames.Add("slush", 224);
			TileNames.Add("hive", 225);
			TileNames.Add("lihzahrd brick", 226);
			TileNames.Add("honey block", 229);
			TileNames.Add("crispy honey block", 230);
			TileNames.Add("wooden spike", 232);
			TileNames.Add("crimsand", 234);
			TileNames.Add("teleporter", 235);
			TileNames.Add("metal bar", 239);
			TileNames.Add("palladium column", 248);
			TileNames.Add("bubblegum", 249);
			TileNames.Add("titanstone", 250);
			TileNames.Add("pumpkin", 251);
			TileNames.Add("hay", 252);
			TileNames.Add("spooky wood", 253);
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
			WallNames.Add("adamantite beam", 32);
			WallNames.Add("demonite brick", 33);
			WallNames.Add("sandstone brick", 34);
			WallNames.Add("ebonstone brick", 35);
			WallNames.Add("red stucco", 36);
			WallNames.Add("yellow stucco", 37);
			WallNames.Add("green stucco", 38);
			WallNames.Add("gray stucco", 39);
			WallNames.Add("ebonwood", 41);
			WallNames.Add("rich mahogany", 42);
			WallNames.Add("pearlwood", 43);
			WallNames.Add("rainbow brick", 44);
			WallNames.Add("tin brick", 45);
			WallNames.Add("tungsten brick", 46);
			WallNames.Add("platinum brick", 47);
			WallNames.Add("grass", 66);
			WallNames.Add("jungle", 67);
			WallNames.Add("flower", 68);
			WallNames.Add("cactus", 72);
			WallNames.Add("cloud", 73);
			WallNames.Add("mushroom", 74);
			WallNames.Add("bone", 75);
			WallNames.Add("slime", 76);
			WallNames.Add("flesh", 77);
			WallNames.Add("disc", 82);
			WallNames.Add("ice brick", 84);
			WallNames.Add("shadewood", 85);
			WallNames.Add("purple glass", 88);
			WallNames.Add("yellow glass", 89);
			WallNames.Add("blue glass", 90);
			WallNames.Add("green glass", 91);
			WallNames.Add("red glass", 92);
			WallNames.Add("multicolor glass", 93);
			WallNames.Add("blue slab", 100);
			WallNames.Add("blue tiled", 101);
			WallNames.Add("pink slab", 102);
			WallNames.Add("pink tiled", 103);
			WallNames.Add("green slab", 104);
			WallNames.Add("green tiled", 105);
			WallNames.Add("wooden fence", 106);
			WallNames.Add("metal fence", 107);
			WallNames.Add("hive", 108);
			WallNames.Add("palladium column", 109);
			WallNames.Add("bubblegum", 110);
			WallNames.Add("titanstone", 111);
			WallNames.Add("lihzahrd brick", 112);
			WallNames.Add("pumpkin", 113);
			WallNames.Add("hay", 114);
			WallNames.Add("spooky wood", 115);
			#endregion
			CommandQueueThread = new Thread(QueueCallback);
			CommandQueueThread.Name = "WorldEdit Callback";
			CommandQueueThread.Start();
			Directory.CreateDirectory("worldedit");
		}
		void OnLeave(LeaveEventArgs e)
		{
			File.Delete(Path.Combine("worldedit", String.Format("clipboard-{0}.dat", e.Who)));
			foreach (string fileName in Directory.EnumerateFiles("worldedit", String.Format("??do-{0}-*.dat", e.Who)))
			{
				File.Delete(fileName);
			}
			Players[e.Who] = new PlayerInfo();
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
			CommandQueue.Add(new Biome(x, y, x2, y2, e.Player, biome1, biome2));
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
						info.y += amount;
					else
						info.y2 += amount;
					e.Player.SendSuccessMessage("Contracted selection down {0}.", amount);
					break;

				case "l":
				case "left":
					if (info.x < info.x2)
						info.x2 -= amount;
					else
						info.x -= amount;
					e.Player.SendSuccessMessage("Contracted selection left {0}.", amount);
					break;

				case "r":
				case "right":
					if (info.x < info.x2)
						info.x += amount;
					else
						info.x2 += amount;
					e.Player.SendSuccessMessage("Contracted selection right {0}.", amount);
					break;

				case "u":
				case "up":
					if (info.y < info.y2)
						info.y2 -= amount;
					else
						info.y -= amount;
					e.Player.SendSuccessMessage("Contracted selection up {0}.", amount);
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
			CommandQueue.Add(new Copy(x, y, x2, y2, e.Player));
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
			CommandQueue.Add(new Cut(x, y, x2, y2, e.Player));
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
			CommandQueue.Add(new Drain(x, y, x2, y2, e.Player));
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
						info.y2 += amount;
					else
						info.y += amount;
					e.Player.SendSuccessMessage("Expanded selection down {0}.", amount);
					break;

				case "l":
				case "left":
					if (info.x < info.x2)
						info.x -= amount;
					else
						info.x2 -= amount;
					e.Player.SendSuccessMessage("Expanded selection left {0}.", amount);
					break;

				case "r":
				case "right":
					if (info.x < info.x2)
						info.x2 += amount;
					else
						info.x += amount;
					e.Player.SendSuccessMessage("Expanded selection right {0}.", amount);
					break;

				case "u":
				case "up":
					if (info.y < info.y2)
						info.y -= amount;
					else
						info.y2 -= amount;
					e.Player.SendSuccessMessage("Expanded selection up {0}.", amount);
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
			CommandQueue.Add(new FixGrass(x, y, x2, y2, e.Player));
		}
		void Flood(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //flood <liquid> <radius>");
				return;
			}

			int liquid = 0;
			if (e.Parameters[0].ToLower() == "lava")
				liquid = 1;
			else if (e.Parameters[0].ToLower() == "honey")
				liquid = 2;
			else if (e.Parameters[0].ToLower() != "water")
			{
				e.Player.SendErrorMessage("Invalid liquid type!");
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
			CommandQueue.Add(new Flood(x, y, x2, y2, e.Player, liquid));
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
					flip ^= 1;
				else if (c == 'y')
					flip ^= 2;
				else
				{
					e.Player.SendErrorMessage("Invalid direction.");
					return;
				}
			}
			CommandQueue.Add(new Flip(e.Player, flip));
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
			e.Player.SendSuccessMessage("Inset selection by {0}.", amount);
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
			e.Player.SendSuccessMessage("Outset selection by {0}.", amount);
		}
		void Paint(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //paint <color>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<int> colors = Tools.GetColorByName(e.Parameters[0].ToLower());
			if (colors.Count == 0)
				e.Player.SendErrorMessage("Invalid color.");
			else if (colors.Count > 1)
				e.Player.SendErrorMessage("More than one color matched.");
			else
				CommandQueue.Add(new Paint(info.x, info.y, info.x2, info.y2, e.Player, colors[0]));
		}
		void PaintWall(CommandArgs e)
		{
			if (e.Parameters.Count != 1)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //paintwall <color>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<int> colors = Tools.GetColorByName(e.Parameters[0].ToLower());
			if (colors.Count == 0)
				e.Player.SendErrorMessage("Invalid color.");
			else if (colors.Count > 1)
				e.Player.SendErrorMessage("More than one color matched.");
			else
				CommandQueue.Add(new PaintWall(info.x, info.y, info.x2, info.y2, e.Player, colors[0]));
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

			CommandQueue.Add(new Paste(info.x, info.y, e.Player));
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
				|| !int.TryParse(e.Parameters[1], out y) || y < 0 || y > Main.maxTilesY)
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
				|| !int.TryParse(e.Parameters[1], out y) || y < 0 || y > Main.maxTilesY)
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
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //point <1 | 2>");
				return;
			}

			switch (e.Parameters[0])
			{
				case "1":
					Players[e.Player.Index].pt = 1;
					e.Player.SendInfoMessage("Hit a block to set point 1.");
					return;
				case "2":
					Players[e.Player.Index].pt = 2;
					e.Player.SendInfoMessage("Hit a block to set point 2.");
					return;
				default:
					e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //point <1 | 2>");
					return;
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
			CommandQueue.Add(new Redo(e.Player, steps));
		}
		void RegionCmd(CommandArgs e)
		{
			if (e.Parameters.Count > 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //region <region name> | //region <x> <y>");
				return;
			}
			else if (e.Parameters.Count == 0)
			{
				Players[e.Player.Index].pt = 3;
				e.Player.SendInfoMessage("Hit a block to select that region.");
			}
			else if (e.Parameters.Count == 1)
			{
				Region curReg = TShock.Regions.ZacksGetRegionByName(e.Parameters[0]);
				if (curReg == null)
					e.Player.SendErrorMessage("Invalid region.");
				else
				{
					PlayerInfo info = GetPlayerInfo(e.Player);
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
						e.Player.SendInfoMessage("Overlapping regions; using first encountered...");
					Region curReg = TShock.Regions.GetRegionByName(regions[0]);

					info.x = curReg.Area.X;
					info.y = curReg.Area.Y;
					info.x2 = curReg.Area.X + curReg.Area.Width;
					info.y2 = curReg.Area.Y + curReg.Area.Height;
				}
				else
					e.Player.SendErrorMessage("Invalid region.");
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

			List<int> tiles1 = Tools.GetTileByName(e.Parameters[0].ToLower());
			List<int> tiles2 = Tools.GetTileByName(e.Parameters[1].ToLower());
			if (tiles1.Count == 0)
				e.Player.SendErrorMessage("Invalid tile.");
			else if (tiles1.Count > 1)
				e.Player.SendErrorMessage("More than one tile matched.");
			else if (tiles2.Count == 0)
				e.Player.SendErrorMessage("Invalid tile.");
			else if (tiles2.Count > 1)
				e.Player.SendErrorMessage("More than one tile matched.");
			else
				CommandQueue.Add(new Replace(info.x, info.y, info.x2, info.y2, e.Player, tiles1[0], tiles2[0]));
		}
		void ReplacePaint(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //replacepaint <color 1> <color 2>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<int> colors1 = Tools.GetColorByName(e.Parameters[0].ToLower());
			List<int> colors2 = Tools.GetColorByName(e.Parameters[1].ToLower());
			if (colors1.Count == 0)
				e.Player.SendErrorMessage("Invalid color.");
			else if (colors1.Count > 1)
				e.Player.SendErrorMessage("More than one color matched.");
			else if (colors2.Count == 0)
				e.Player.SendErrorMessage("Invalid color.");
			else if (colors2.Count > 1)
				e.Player.SendErrorMessage("More than one color matched.");
			else
				CommandQueue.Add(new ReplacePaint(info.x, info.y, info.x2, info.y2, e.Player, colors1[0], colors2[0]));
		}
		void ReplacePaintWall(CommandArgs e)
		{
			if (e.Parameters.Count != 2)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //replacepaintwall <color 1> <color 2>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			List<int> colors1 = Tools.GetColorByName(e.Parameters[0].ToLower());
			List<int> colors2 = Tools.GetColorByName(e.Parameters[1].ToLower());
			if (colors1.Count == 0)
				e.Player.SendErrorMessage("Invalid color.");
			else if (colors1.Count > 1)
				e.Player.SendErrorMessage("More than one color matched.");
			else if (colors2.Count == 0)
				e.Player.SendErrorMessage("Invalid color.");
			else if (colors2.Count > 1)
				e.Player.SendErrorMessage("More than one color matched.");
			else
				CommandQueue.Add(new ReplacePaintWall(info.x, info.y, info.x2, info.y2, e.Player, colors1[0], colors2[0]));
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

			List<int> walls1 = Tools.GetWallByName(e.Parameters[0].ToLower());
			List<int> walls2 = Tools.GetWallByName(e.Parameters[1].ToLower());
			if (walls1.Count == 0)
				e.Player.SendErrorMessage("Invalid wall.");
			else if (walls1.Count > 1)
				e.Player.SendErrorMessage("More than one wall matched.");
			else if (walls2.Count == 0)
				e.Player.SendErrorMessage("Invalid wall.");
			else if (walls2.Count > 1)
				e.Player.SendErrorMessage("More than one wall matched.");
			else
				CommandQueue.Add(new ReplaceWall(info.x, info.y, info.x2, info.y2, e.Player, walls1[0], walls2[0]));
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
			CommandQueue.Add(new Rotate(e.Player, degrees));
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

						int pageNumber;
						if (!PaginationTools.TryParsePageNumber(e.Parameters, 0, e.Player, out pageNumber))
							return;

						var schematics = new List<string>(Directory.EnumerateFiles("worldedit", "schematic-*.dat"));
						PaginationTools.SendPage(e.Player, pageNumber, PaginationTools.BuildLinesFromTerms(schematics),
							new PaginationTools.Settings
							{
								HeaderFormat = "Schematics ({0}/{1}):",
								FooterFormat = "Type /schematic list {0} for more."
							});
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
			e.Player.SendSuccessMessage("Set selection type to {0}.", name);
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

			List<int> tiles = Tools.GetTileByName(e.Parameters[0].ToLower());
			if (tiles.Count == 0)
				e.Player.SendErrorMessage("Invalid tile.");
			else if (tiles.Count > 1)
				e.Player.SendErrorMessage("More than one tile matched.");
			else
				CommandQueue.Add(new Set(info.x, info.y, info.x2, info.y2, e.Player, tiles[0]));
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

			List<int> walls = Tools.GetWallByName(e.Parameters[0].ToLower());
			if (walls.Count == 0)
				e.Player.SendErrorMessage("Invalid wall.");
			else if (walls.Count > 1)
				e.Player.SendErrorMessage("More than one wall matched.");
			else
				CommandQueue.Add(new SetWall(info.x, info.y, info.x2, info.y2, e.Player, walls[0]));
		}
		void SetWire(CommandArgs e)
		{
			if (e.Parameters.Count != 3)
			{
				e.Player.SendErrorMessage("Invalid syntax! Proper syntax: //setwire <wire 1 state> <wire 2 state> <wire 3 state>");
				return;
			}
			PlayerInfo info = GetPlayerInfo(e.Player);
			if (info.x == -1 || info.y == -1 || info.x2 == -1 || info.y2 == -1)
			{
				e.Player.SendErrorMessage("Invalid selection.");
				return;
			}

			bool wire1 = false;
			if (e.Parameters[0].ToLower() == "on")
				wire1 = true;
			else if (e.Parameters[0].ToLower() != "off")
			{
				e.Player.SendErrorMessage("Invalid wire 1 state.");
				return;
			}

			bool wire2 = false;
			if (e.Parameters[1].ToLower() == "on")
				wire2 = true;
			else if (e.Parameters[1].ToLower() != "off")
			{
				e.Player.SendErrorMessage("Invalid wire 2 state.");
				return;
			}

			bool wire3 = false;
			if (e.Parameters[2].ToLower() == "on")
				wire3 = true;
			else if (e.Parameters[2].ToLower() != "off")
			{
				e.Player.SendErrorMessage("Invalid wire 3 state.");
				return;
			}

			CommandQueue.Add(new SetWire(info.x, info.y, info.x2, info.y2, e.Player, wire1, wire2, wire3));
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
					e.Player.SendSuccessMessage("Shifted selection down {0}.", amount);
					break;

				case "l":
				case "left":
					info.x -= amount;
					info.x2 -= amount;
					e.Player.SendSuccessMessage("Shifted selection left {0}.", amount);
					break;

				case "r":
				case "right":
					info.x += amount;
					info.x2 += amount;
					e.Player.SendSuccessMessage("Shifted selection right {0}.", amount);
					break;

				case "u":
				case "up":
					info.y -= amount;
					info.y2 -= amount;
					e.Player.SendSuccessMessage("Shifted selection up {0}.", amount);
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
			e.Player.SendInfoMessage("Selection size: {0} x {1}", lenX, lenY);
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
			CommandQueue.Add(new Undo(e.Player, steps));
		}
	}
}