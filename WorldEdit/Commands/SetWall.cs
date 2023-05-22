using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
	public class SetWall : WECommand
	{
		private Expression expression;
		private WallPlaceID wallType;

		public SetWall(int x, int y, int x2, int y2, MagicWand magicWand, TSPlayer plr, WallPlaceID wallType, Expression expression)
			: base(x, y, x2, y2, magicWand, plr)
		{
			this.expression = expression ?? new TestExpression(new Test(t => true));
			this.wallType = wallType;
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
					if (Tools.CanSet(Main.tile[i, j], wallType,
                        select, expression, magicWand, i, j, plr))
                    {
                        Main.tile[i, j].wall = (ushort)wallType.wallID;
						edits++;
					}
				}
			}
			ResetSection();

			plr.SendSuccessMessage($"Set walls to {wallType.name}. ({edits})");
		}
	}
}