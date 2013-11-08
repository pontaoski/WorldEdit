using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class PaintWall : WECommand
	{
		List<Condition> conditions;
		int color;

		public PaintWall(int x, int y, int x2, int y2, TSPlayer plr, int color, List<Condition> conditions)
			: base(x, y, x2, y2, plr)
		{
			this.conditions = conditions;
			this.color = color;
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
						Main.tile[i, j].wallColor((byte)color);
						edits++;
					}
				}
			}
			ResetSection();

			plr.SendSuccessMessage("Painted walls {0}. ({1})", WorldEdit.ColorNames[color], edits);
		}
	}
}
