using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class FixSlopes : WECommand
	{
		public FixSlopes(int x, int y, int x2, int y2, TSPlayer plr)
			: base(x, y, x2, y2, plr)
		{
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (Main.tile[i, j].slope() != 0 && TileSolid(i, j - 1))
					{
						Main.tile[i, j].slope(0);
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Fixed nearby slopes. ({0})", edits);
		}

		bool TileSolid(int x, int y)
		{
			if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
			{
				return true;
			}
			return Main.tile[x, y].active() && Main.tileSolid[Main.tile[x, y].type];
		}
	}
}
