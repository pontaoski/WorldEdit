using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Mow : WECommand
	{
		static int[] mowTiles = new[] { 3, 24, 32, 52, 61, 62, 69, 73, 74, 110, 113, 115, 201, 205, 227 };

		public Mow(int x, int y, int x2, int y2, TSPlayer plr)
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
					int type = Main.tile[i, j].type;
					if (mowTiles.Contains(type))
					{
						Main.tile[i, j].active(false);
						Main.tile[i, j].type = 0;
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Mowed grass, thorns, and vines. ({0})", edits);
		}
	}
}
