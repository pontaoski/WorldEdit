using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Flood : WECommand
	{
		private int liquid;

		public Flood(int x, int y, int x2, int y2, TSPlayer plr, int liquid)
			: base(x, y, x2, y2, plr)
		{
			this.liquid = liquid;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (!Main.tile[i, j].active() || !Main.tileSolid[Main.tile[i, j].type])
					{
						Main.tile[i, j].liquidType((byte)liquid);
						Main.tile[i, j].liquid = 255;
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Flooded nearby area. ({0})", edits);
		}
	}
}