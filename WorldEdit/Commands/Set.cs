using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Set : WECommand
	{
		static string[] SpecialTileNames = { "air", "lava", "honey", "water" };

		List<Condition> conditions;
		int tile;

		public Set(int x, int y, int x2, int y2, TSPlayer plr, int tile, List<Condition> conditions)
			: base(x, y, x2, y2, plr)
		{
			this.conditions = conditions;
			this.tile = tile;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (selectFunc(i, j, plr) && conditions.TrueForAll(c => c(i, j)))
					{
						SetTile(i, j, tile);
						edits++;
					}
				}
			}
			ResetSection();

			string tileName = tile < 0 ? SpecialTileNames[-tile - 1] : "tile " + tile;
			plr.SendSuccessMessage("Set tiles to {0}. ({1})", tileName, edits);
		}
	}
}