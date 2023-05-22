using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
	public class Set : WECommand
	{
		private Expression expression;
		private TilePlaceID tileType;

		public Set(int x, int y, int x2, int y2, MagicWand magicWand, TSPlayer plr, TilePlaceID tileType, Expression expression)
			: base(x, y, x2, y2, magicWand, plr)
		{
			this.tileType = tileType;
			this.expression = expression ?? new TestExpression(new Test(t => true));
		}

		public override void Execute()
        {
            if (!CanUseCommand()) { return; }
            Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (Tools.CanSet(Main.tile[i, j], tileType,
                        select, expression, magicWand, i, j, plr))
					{
						SetTile(i, j, tileType);
						edits++;
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage($"Set tiles to {tileType.name}. ({edits})");
		}
	}
}