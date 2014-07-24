using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
	public class SetWire : WECommand
	{
		Expression expression;
		bool wire;
		bool wire2;
		bool wire3;

		public SetWire(int x, int y, int x2, int y2, TSPlayer plr, bool wire1, bool wire2, bool wire3, Expression expression)
			: base(x, y, x2, y2, plr)
		{
			this.expression = expression ?? new TestExpression(new Test((i, j) => true));
			this.wire = wire1;
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
					if (select(i, j, plr) && expression.Evaluate(i, j))
					{
						Main.tile[i, j].wire(wire);
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
