using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
	public class Set : WECommand
	{
		int tile;
		Expression expression;

		public Set(int x, int y, int x2, int y2, TSPlayer plr, int tile, Expression expression)
			: base(x, y, x2, y2, plr)
		{
			this.tile = tile;
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
					if (((tile >= 0 && (!Main.tile[i, j].active() || Main.tile[i, j].type != tile)) ||
						(tile == -1 && Main.tile[i, j].active()) ||
						(tile == -2 && (Main.tile[i, j].liquid == 0 || Main.tile[i, j].liquidType() != 1)) ||
						(tile == -3 && (Main.tile[i, j].liquid == 0 || Main.tile[i, j].liquidType() != 2)) ||
						(tile == -4 && (Main.tile[i, j].liquid == 0 || Main.tile[i, j].liquidType() != 0))) &&
						select(i, j, plr) && expression.Evaluate(i, j))
					{
						SetTile(i, j, tile);
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Set tiles. ({0})", edits);
		}
	}
}