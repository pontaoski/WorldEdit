using System;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class ReplacePaintWall : WECommand
	{
		private int paint1;
		private int paint2;

		public ReplacePaintWall(int x, int y, int x2, int y2, TSPlayer plr, int paint1, int paint2)
			: base(x, y, x2, y2, plr)
		{
			this.paint1 = paint1;
			this.paint2 = paint2;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			if (paint1 != paint2)
			{
				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
					{
						if (selectFunc(i, j, plr) && Main.tile[i, j].wall > 0 && Main.tile[i, j].wallColor() == paint1)
						{
							Main.tile[i, j].wallColor((byte)paint2);
							edits++;
						}
					}
				}
				ResetSection();
			}

			plr.SendSuccessMessage("Replaced {0} wall paint with {1} wall paint. ({2})",
				WorldEdit.ColorNames[paint1], WorldEdit.ColorNames[paint2], edits);
		}
	}
}