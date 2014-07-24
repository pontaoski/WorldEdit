using System;
using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
	public class Paint : WECommand
	{
		int color;
		Expression expression;

		public Paint(int x, int y, int x2, int y2, TSPlayer plr, int color, Expression expression)
			: base(x, y, x2, y2, plr)
		{
			this.color = color;
			this.expression = expression ?? new TestExpression(new Test((i, j) => true));
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (Main.tile[i, j].active() && Main.tile[i, j].color() != color && select(i, j, plr) && expression.Evaluate(i, j))
					{
						Main.tile[i, j].color((byte)color);
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Painted tiles. ({0})", edits);
		}
	}
}
