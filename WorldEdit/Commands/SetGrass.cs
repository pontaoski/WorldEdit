using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
    public class SetGrass : WECommand
    {
        private Expression expression;
        private string grass;

        private static ushort[] grassTiles = new[]
        {
            TileID.CorruptGrass,
            TileID.FleshGrass,
            TileID.FleshGrass,
            TileID.Grass,
            TileID.HallowedGrass,
            TileID.JungleGrass,
            TileID.MushroomGrass
        };

        private static ushort[] tiles = new[]
        {
            TileID.Dirt,
            TileID.Dirt,
            TileID.Dirt,
            TileID.Dirt,
            TileID.Dirt,
            TileID.Mud,
            TileID.Mud,
        };

        private static string[] grassTiles_ = new[]
        {
            "corrupt",
            "flesh",
            "crimson",
            "normal",
            "hallowed",
            "jungle",
            "mushroom"
        };

        public SetGrass(int x, int y, int x2, int y2, TSPlayer plr, string grass, Expression expression)
            : base(x, y, x2, y2, plr)
        {
            this.expression = expression ?? new TestExpression(new Test(t => true));
            this.grass = grass;
        }

        public override void Execute()
        {
            if (x < 2) x = 2;
            else if (x > (Main.maxTilesX - 3)) x = (Main.maxTilesX - 3);
            if (y < 2) y = 2;
            else if (y > (Main.maxTilesY - 3)) y = (Main.maxTilesY - 3);
            if (x2 < 2) x2 = 2;
            else if (x2 > (Main.maxTilesX - 3)) x2 = (Main.maxTilesX - 3);
            if (y2 < 2) y2 = 2;
            else if (y2 > (Main.maxTilesY - 3)) y2 = (Main.maxTilesY - 3);

            Tools.PrepareUndo(x, y, x2, y2, plr);
            int index = grassTiles_.ToList().IndexOf(grass);
            int edits = 0;
            for (int i = x; i <= x2; i++)
            {
                for (int j = y; j <= y2; j++)
                {
                    bool XY = Main.tile[i, j].active();
                    bool mXmY = Main.tile[i - 1, j - 1].active();
                    bool mXpY = Main.tile[i - 1, j + 1].active();
                    bool pXmY = Main.tile[i + 1, j - 1].active();
                    bool pXpY = Main.tile[i + 1, j + 1].active();
                    bool mXY = Main.tile[i - 1, j].active();
                    bool pXY = Main.tile[i + 1, j].active();
                    bool XmY = Main.tile[i, j - 1].active();
                    bool XpY = Main.tile[i, j + 1].active();

                    if (XY && !(mXmY && mXpY && pXmY && pXpY && mXY && pXY && XmY && XpY)
                        && expression.Evaluate((Tile)Main.tile[i, j])
                        && (Main.tile[i, j].type == tiles[index]))
                    {
                        Main.tile[i, j].type = grassTiles[index];
                        edits++;
                    }
                }
            }
            ResetSection();
            plr.SendSuccessMessage("Set {1} grass. ({0})", edits, grassTiles_[index]);
        }
    }
}