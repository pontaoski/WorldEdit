using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Drain : WECommand
	{
		public Drain(int x, int y, int x2, int y2, TSPlayer plr)
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
					if (Main.tile[i, j].liquid != 0)
					{
						Main.tile[i, j].liquid = 0;
						Main.tile[i, j].liquidType(0);
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Drained nearby area. ({0})", edits);
		}
	}
}
