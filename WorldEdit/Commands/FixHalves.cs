using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class FixHalves : WECommand
	{
		public FixHalves(int x, int y, int x2, int y2, TSPlayer plr)
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
					if (Main.tile[i, j].halfBrick() && TileSolid(i, j - 1))
					{
						Main.tile[i, j].halfBrick(false);
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Fixed half blocks. ({0})", edits);
		}
	}
}
