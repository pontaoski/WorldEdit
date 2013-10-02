using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Paint : WECommand
	{
		private int color;

		public Paint(int x, int y, int x2, int y2, TSPlayer plr, int color)
			: base(x, y, x2, y2, plr)
		{
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
					if (selectFunc(i, j, plr) && Main.tile[i, j].active() && Main.tileSolid[Main.tile[i, j].type]
						&& Main.tile[i, j].color() != color)
					{
						Main.tile[i, j].color((byte)color);
						edits++;
					}
				}
			}
			ResetSection();

			plr.SendSuccessMessage("Painted tiles {0}. ({1})", WorldEdit.ColorNames[color], edits);
		}
	}
}
