using System;
using System.Linq;
using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
	public abstract class WECommand
	{
		public TSPlayer plr;
		public Selection select;
		public int x;
		public int x2;
		public int y;
		public int y2;

		protected WECommand(int x, int y, int x2, int y2, TSPlayer plr)
		{
			this.plr = plr;
			this.select = WorldEdit.GetPlayerInfo(plr).select ?? WorldEdit.Selections["normal"];
			this.x = x;
			this.x2 = x2;
			this.y = y;
			this.y2 = y2;
		}

		public abstract void Execute();
		public void Position()
		{
			int temp;
			if (x < 0)
				x = 0;
			if (y < 0)
				y = 0;
			if (x2 >= Main.maxTilesX)
				x2 = Main.maxTilesX - 1;
			if (y2 >= Main.maxTilesY)
				y2 = Main.maxTilesY - 1;
			if (x > x2)
			{
				temp = x2;
				x2 = x;
				x = temp;
			}
			if (y > y2)
			{
				temp = y2;
				y2 = y;
				y = temp;
			}
		}
		public void ResetSection()
		{
			int lowX = Netplay.GetSectionX(x);
			int highX = Netplay.GetSectionX(x2);
			int lowY = Netplay.GetSectionY(y);
			int highY = Netplay.GetSectionY(y2);
			foreach (ServerSock sock in Netplay.serverSock.Where(s => s.active))
			{
				for (int i = lowX; i <= highX; i++)
				{
					for (int j = lowY; j <= highY; j++)
						sock.tileSection[i, j] = false;
				}
			}
		}
		public void SetTile(int i, int j, int tile)
		{
			switch (tile)
			{
				case -1:
					Main.tile[i, j].active(false);
					Main.tile[i, j].frameX = -1;
					Main.tile[i, j].frameY = -1;
					Main.tile[i, j].liquidType(0);
					Main.tile[i, j].liquid = 0;
					Main.tile[i, j].type = 0;
					return;
				case -2:
					Main.tile[i, j].active(false);
					Main.tile[i, j].liquidType(1);
					Main.tile[i, j].liquid = 255;
					Main.tile[i, j].type = 0;
					return;
				case -3:
					Main.tile[i, j].active(false);
					Main.tile[i, j].liquidType(2);
					Main.tile[i, j].liquid = 255;
					Main.tile[i, j].type = 0;
					return;
				case -4:
					Main.tile[i, j].active(false);
					Main.tile[i, j].liquidType(0);
					Main.tile[i, j].liquid = 255;
					Main.tile[i, j].type = 0;
					return;
				default:
					if (Main.tileFrameImportant[tile])
						WorldGen.PlaceTile(i, j, tile);
					else
					{
						Main.tile[i, j].active(true);
						Main.tile[i, j].frameX = -1;
						Main.tile[i, j].frameY = -1;
						Main.tile[i, j].liquidType(0);
						Main.tile[i, j].liquid = 0;
						Main.tile[i, j].type = (ushort)tile;
					}
					return;
			}
		}
		public bool TileSolid(int x, int y)
		{
			return x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY || (Main.tile[x, y].active() && Main.tileSolid[Main.tile[x, y].type]);
		}
	}
}