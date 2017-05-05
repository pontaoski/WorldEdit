using System;
using System.IO;
using System.IO.Compression;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using TShockAPI;
using WorldEdit.Expressions;

namespace WorldEdit.Commands
{
    public class Paste : WECommand
    {
        private int alignment;
        private Expression expression;
        private bool mode_MainBlocks;

        public Paste(int x, int y, TSPlayer plr, int alignment, Expression expression, bool mode_MainBlocks)
            : base(x, y, Int32.MaxValue, Int32.MaxValue, plr)
        {
            this.alignment = alignment;
            this.expression = expression;
            this.mode_MainBlocks = mode_MainBlocks;
        }

        public override void Execute()
        {
            string clipboardPath = Tools.GetClipboardPath(plr.User.ID);
            bool NewStruct = Tools.NewClipboardStruct(clipboardPath);
            using (var reader = new BinaryReader(new GZipStream(new FileStream(clipboardPath, FileMode.Open), CompressionMode.Decompress)))
            {
                reader.ReadInt32();
                reader.ReadInt32();

                int width = reader.ReadInt32() - 1;
                int height = reader.ReadInt32() - 1;

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

                Tools.PrepareUndo(x, y, x2, y2, plr);
                if (NewStruct)
                {
                    for (int i = x; i <= x2; i++)
                    {
                        for (int j = y; j <= y2; j++)
                        {
                            var Tile = reader.ReadTileNew();
                            if (i >= 0 && j >= 0 && i < Main.maxTilesX && j < Main.maxTilesY && (expression == null || expression.Evaluate((mode_MainBlocks) ? Main.tile[i, j] : Tile.Item1)))
                            {
                                if ((Tile.Item2 != null) || (Tile.Item3 != null)
                                || (Tile.Item4 != null))
                                {
                                    Main.tile[i, j] = new Tile();
                                    Main.tile[i + 1, j] = new Tile();
                                    Main.tile[i, j + 1] = new Tile();
                                    Main.tile[i + 1, j + 1] = new Tile();
                                }
                                Main.tile[i, j] = Tile.Item1;
                                if (Tile.Item2 != null)
                                {
                                    int SignID = Sign.ReadSign(i, j);
                                    string Text = Tile.Item2;
                                    if (SignID != -1)
                                    { Sign.TextSign(SignID, Text); }
                                }
                                if (Tile.Item3 != null)
                                {
                                    int FrameID = TEItemFrame.Place(i, j);
                                    if (FrameID != -1)
                                    {
                                        WorldGen.PlaceObject(i, j, Terraria.ID.TileID.ItemFrame);
                                        TEItemFrame frame = (TEItemFrame)TileEntity.ByID[FrameID];
                                        frame.item = Tile.Item3;
                                    }
                                }
                                if (Tile.Item4 != null)
                                {
                                    int ChestID = Chest.CreateChest(i, j);
                                    if (ChestID != -1)
                                    {
                                        WorldGen.PlaceChest(i, j);
                                        for (int a = 0; a < 40; a++)
                                        { Main.chest[ChestID].item[a] = Tile.Item4[a]; }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = x; i <= x2; i++)
                    {
                        for (int j = y; j <= y2; j++)
                        {
                            Tile tile = reader.ReadTileOld();
                            if (i >= 0 && j >= 0 && i < Main.maxTilesX && j < Main.maxTilesY && (expression == null || expression.Evaluate((mode_MainBlocks) ? Main.tile[i, j] : tile)))
                                Main.tile[i, j] = tile;
                        }
                    }
                }
            }
            ResetSection();
            plr.SendSuccessMessage("Pasted clipboard to selection.");
        }
    }
}