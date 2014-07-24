using System;
using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
	public class SetWall : WECommand
	{
		Expression expression;
		int wall;

		public SetWall(int x, int y, int x2, int y2, TSPlayer plr, int wall, Expression expression)
			: base(x, y, x2, y2, plr)
		{
			this.expression = expression ?? new TestExpression(new Test((i, j) => true));
			this.wall = wall;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (Main.tile[i, j].wall != wall && select(i, j, plr) && expression.Evaluate(i, j))
					{
						Main.tile[i, j].wall = (byte)wall;
						edits++;
					}
				}
			}
			ResetSection();

			string wallName = wall == 0 ? "air" : "wall " + wall;
			plr.SendSuccessMessage("Set walls to {0}. ({1})", wallName, edits);
		}
	}
}