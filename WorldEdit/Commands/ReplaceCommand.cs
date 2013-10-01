using System;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class ReplaceCommand : WECommand
	{
		private static string[] SpecialTileNames = { "air", "lava", "water", "wire", "no wire" };

		private int tile1;
		private int tile2;

		public ReplaceCommand(int x, int y, int x2, int y2, TSPlayer plr, int tile1, int tile2)
			: base(x, y, x2, y2, plr)
		{
			this.tile1 = tile1;
			this.tile2 = tile2;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			if (tile1 != tile2)
			{
				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
					{
						if (selectFunc(i, j, plr) &&
							((tile1 >= 0 && Main.tile[i, j].active() && Main.tile[i, j].type == tile1)
							|| (tile1 == -1 && !Main.tile[i, j].active())
							|| (tile1 == -2 && Main.tile[i, j].lava() && Main.tile[i, j].liquid > 0)
							|| (tile1 == -3 && !Main.tile[i, j].lava() && Main.tile[i, j].liquid > 0)))
						{
							SetTile(i, j, tile2);
							edits++;
						}
					}
				}
				ResetSection();
			}

			string tileName1 = tile1 < 0 ? SpecialTileNames[-tile1 - 1] : "tile " + tile1;
			string tileName2 = tile2 < 0 ? SpecialTileNames[-tile2 - 1] : "tile " + tile2;
			plr.SendSuccessMessage(String.Format("Replaced {0} with {1}. ({2})", tileName1, tileName2, edits));
		}
	}
}