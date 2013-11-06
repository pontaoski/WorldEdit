using System;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class ReplacePaint : WECommand
	{
		private int paint1;
		private int paint2;

		public ReplacePaint(int x, int y, int x2, int y2, TSPlayer plr, int paint1, int paint2)
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
						if (selectFunc(i, j, plr) && Main.tile[i, j].active() && Main.tile[i, j].color() == paint1)
						{
							Main.tile[i, j].color((byte)paint2);
							edits++;
						}
					}
				}
				ResetSection();
			}

			plr.SendSuccessMessage("Replaced {0} paint with {1} paint. ({2})",
				WorldEdit.ColorNames[paint1], WorldEdit.ColorNames[paint2], edits);
		}
	}
}