using System;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class SetWire : WECommand
	{
		private bool wire1;
		private bool wire2;
		private bool wire3;

		public SetWire(int x, int y, int x2, int y2, TSPlayer plr, bool wire1, bool wire2, bool wire3)
			: base(x, y, x2, y2, plr)
		{
			this.wire1 = wire1;
			this.wire2 = wire2;
			this.wire3 = wire3;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (selectFunc(i, j, plr))
					{
						if (Main.tile[i, j].wire() != wire1)
						{
							Main.tile[i, j].wire(wire1);
						}
						if (Main.tile[i, j].wire2() != wire2)
						{
							Main.tile[i, j].wire2(wire2);
						}
						if (Main.tile[i, j].wire3() != wire3)
						{
							Main.tile[i, j].wire3(wire3);
						}

						if (Main.tile[i, j].wire() != wire1 || Main.tile[i, j].wire2() != wire2
							|| Main.tile[i, j].wire3() != wire3)
						{
							edits++;
						}
					}
				}
			}
			ResetSection();

			plr.SendSuccessMessage("Set wires. ({0})", edits);
		}
	}
}
