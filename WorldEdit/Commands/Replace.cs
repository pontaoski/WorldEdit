﻿using Terraria;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
    public class Replace : WECommand
    {
        private Expression expression;
        private TilePlaceID from;
        private TilePlaceID to;

        public Replace(int x, int y, int x2, int y2, TSPlayer plr, TilePlaceID from, TilePlaceID to, Expression expression)
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
                        to.SetTile(i, j);
                        edits++;
                    }
                }
            }
            ResetSection();
            plr.SendSuccessMessage($"Replaced {from.Name} with {to.Name}. ({edits})");
        }
    }
}