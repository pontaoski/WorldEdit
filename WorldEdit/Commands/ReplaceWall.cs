using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
    public class ReplaceWall : WECommand
    {
        private Expression expression;
        private WallPlaceID from;
        private WallPlaceID to;

        public ReplaceWall(int x, int y, int x2, int y2, TSPlayer plr, WallPlaceID from, WallPlaceID to, Expression expression)
            : base(x, y, x2, y2, plr)
        {
            this.from = from;
            this.to = to;
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
                    ITile tile = Main.tile[i, j];
                    if (from.Is(tile) && to.CanSet(tile, select, expression, magicWand, i, j, plr))
                    {
                        tile.wall = (ushort)to.wallID;
                        edits++;
                    }
                }
            }
            ResetSection();
            plr.SendSuccessMessage($"Replaced {from.name} with {to.name}. ({edits})");
        }
    }
}