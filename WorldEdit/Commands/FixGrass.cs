using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class FixGrass : WECommand
	{
		public FixGrass(int x, int y, int x2, int y2, TSPlayer plr)
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
					if (type == 2 || type == 23 || type == 60 || type == 70 || type == 109 || type == 199)
					{
						if (TileSolid(i - 1, j - 1) && TileSolid(i - 1, j) && TileSolid(i - 1, j + 1) && TileSolid(i, j - 1)
							&& TileSolid(i, j + 1) && TileSolid(i + 1, j) && TileSolid(i + 1, j) && TileSolid(i + 1, j + 1))
						{
							type = (type == 60 || type == 70) ? (byte)59 : (byte)0;
							edits++;
						}
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Fixed grass. ({0})", edits);
		}
	}
}