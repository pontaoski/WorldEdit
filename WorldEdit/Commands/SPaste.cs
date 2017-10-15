using OTAPI.Tile;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
    public class SPaste : WECommand
    {
        private readonly int alignment;
        private readonly Expression expression;
        private readonly bool tiles;
        private readonly bool tilePaints;
        private readonly bool emptyTiles;
        private readonly bool walls;
        private readonly bool wallPaints;
        private readonly bool wires;
        private readonly bool liquids;

        public SPaste(int x, int y, TSPlayer plr, int alignment, Expression expression, bool tiles, bool tilePaints, bool emptyTiles, bool walls, bool wallPaints, bool wires, bool liquids)
            : base(x, y, int.MaxValue, int.MaxValue, plr)
        {
            this.alignment = alignment;
            this.expression = expression;
            this.tiles = tiles;
            this.tilePaints = tilePaints;
            this.emptyTiles = emptyTiles;
            this.walls = walls;
            this.wallPaints = wallPaints;
            this.wires = wires;
            this.liquids = liquids;
        }

        public override void Execute()
        {
            var clipboardPath = Tools.GetClipboardPath(plr.User.ID);

            var data = Tools.LoadWorldData(clipboardPath);

            var width = data.Width - 1;
            var height = data.Height - 1;

            if ((alignment & 1) == 0)
                x2 = x + width;
            else
            {
                x2 = x;
                x -= width;
            }
            if ((alignment & 2) == 0)
                y2 = y + height;
            else
            {
                y2 = y;
                y -= height;
            }

            if (!CanUseCommand()) { return; }
            Tools.PrepareUndo(x, y, x2, y2, plr);

            for (var i = x; i <= x2; i++)
            {
                for (var j = y; j <= y2; j++)
                {
                    var index1 = i - x;
                    var index2 = j - y;

                    if (i < 0 || j < 0 || i >= Main.maxTilesX || j >= Main.maxTilesY ||
                        expression != null && !expression.Evaluate(data.Tiles[index1, index2]))
                    {
                        continue;
                    }

                    ITile tile = (ITile)Main.tile[i, j].Clone();

                    if (tiles) { tile = data.Tiles[index1, index2]; }
                    else
                    {
                        tile.wall = data.Tiles[index1, index2].wall;
                        tile.wallColor(data.Tiles[index1, index2].wallColor());
                        tile.liquid = data.Tiles[index1, index2].liquid;
                        tile.liquidType(data.Tiles[index1, index2].liquidType());
                        tile.wire(data.Tiles[index1, index2].wire());
                        tile.wire2(data.Tiles[index1, index2].wire2());
                        tile.wire3(data.Tiles[index1, index2].wire3());
                        tile.wire4(data.Tiles[index1, index2].wire4());
                        tile.actuator(data.Tiles[index1, index2].actuator());
                        tile.inActive(data.Tiles[index1, index2].inActive());
                    }

                    if (emptyTiles || tile.active() || (tile.wall != 0) || (tile.liquid != 0) || tile.wire() || tile.wire2() || tile.wire3() || tile.wire4())
                    {
                        if (!tilePaints)
                        { tile.color(Main.tile[i, j].color()); }
                        if (!walls)
                        {
                            tile.wall = Main.tile[i, j].wall;
                            tile.wallColor(Main.tile[i, j].wallColor());
                        }
                        if (!wallPaints)
                        { tile.wallColor(Main.tile[i, j].wallColor()); }
                        if (!liquids)
                        {
                            tile.liquid = Main.tile[i, j].liquid;
                            tile.liquidType(Main.tile[i, j].liquidType());
                        }
                        if (!wires)
                        {
                            tile.wire(Main.tile[i, j].wire());
                            tile.wire2(Main.tile[i, j].wire2());
                            tile.wire3(Main.tile[i, j].wire3());
                            tile.wire4(Main.tile[i, j].wire4());
                            tile.actuator(Main.tile[i, j].actuator());
                            tile.inActive(Main.tile[i, j].inActive());
                        }

                        Main.tile[i, j] = tile;
                    }
                }
            }

            foreach (var sign in data.Signs)
            {
                var id = Sign.ReadSign(sign.X + x, sign.Y + y);
                if (id == -1)
                {
                    continue;
                }

                Sign.TextSign(id, sign.Text);
            }

            foreach (var itemFrame in data.ItemFrames)
            {
                var ifX = itemFrame.X + x;
                var ifY = itemFrame.Y + y;

                var id = TEItemFrame.Place(ifX, ifY);
                if (id == -1)
                {
                    continue;
                }

                WorldGen.PlaceObject(ifX, ifY, TileID.ItemFrame);
                var frame = (TEItemFrame)TileEntity.ByID[id];

                frame.item = new Item();
                frame.item.netDefaults(itemFrame.Item.NetId);
                frame.item.stack = itemFrame.Item.Stack;
                frame.item.prefix = itemFrame.Item.PrefixId;
            }

            foreach (var chest in data.Chests)
            {
                int chestX = chest.X + x, chestY = chest.Y + y;

                int id;
                if ((id = Chest.FindChest(chestX, chestY)) == -1 &&
                    (id = Chest.CreateChest(chestX, chestY)) == -1)
                {
                    continue;
                }

                WorldGen.PlaceChest(chestX, chestY);
                for (var index = 0; index < chest.Items.Length; index++)
                {
                    var netItem = chest.Items[index];
                    var item = new Item();
                    item.netDefaults(netItem.NetId);
                    item.stack = netItem.Stack;
                    item.prefix = netItem.PrefixId;
                    Main.chest[id].item[index] = item;

                }
            }

            ResetSection();
            plr.SendSuccessMessage("Pasted clipboard to selection.");
        }
    }
}