using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class SetWire : WECommand
	{
		List<Condition> conditions;
		bool wire1;
		bool wire2;
		bool wire3;

		public SetWire(int x, int y, int x2, int y2, TSPlayer plr, bool wire1, bool wire2, bool wire3, List<Condition> conditions)
			: base(x, y, x2, y2, plr)
		{
			this.conditions = conditions;
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
					if (selectFunc(i, j, plr) && conditions.TrueForAll(c => c(i, j)))
					{
						Main.tile[i, j].wire(wire1);
						Main.tile[i, j].wire2(wire2);
						Main.tile[i, j].wire3(wire3);
						edits++;
					}
				}
			}
			ResetSection();

			plr.SendSuccessMessage("Set wires. ({0})", edits);
		}
	}
}
