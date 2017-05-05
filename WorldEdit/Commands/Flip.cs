using System;
using System.IO;
using System.IO.Compression;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
    public class Flip : WECommand
    {
        private const int BUFFER_SIZE = 1048576;
        private bool flipX;
        private bool flipY;

        public Flip(TSPlayer plr, bool flipX, bool flipY)
            : base(0, 0, 0, 0, plr)
        {
            this.flipX = flipX;
            this.flipY = flipY;
        }

        public override void Execute()
        {
            string clipboardPath = Tools.GetClipboardPath(plr.User.ID);

            if (Tools.NewClipboardStruct(clipboardPath))
            {
                Tuple<Tile, string, Item, Item[]>[,] tiles = Tools.LoadWorldDataNew(clipboardPath);

                int width = tiles.GetLength(0);
                int height = tiles.GetLength(1);
                int endX = flipX ? -1 : width;
                int endY = flipY ? -1 : height;
                int incX = flipX ? -1 : 1;
                int incY = flipY ? -1 : 1;

                // GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
                using (var writer =
                    new BinaryWriter(
                        new BufferedStream(
                            new GZipStream(File.Open(clipboardPath, FileMode.Create), CompressionMode.Compress), BUFFER_SIZE)))
                {
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(width);
                    writer.Write(height);

                    for (int i = flipX ? width - 1 : 0; i != endX; i += incX)
                    {
                        for (int j = flipY ? height - 1 : 0; j != endY; j += incY)
                        {
                            if (tiles[i, j].Item1.slope() == 0)
                                writer.WriteTileNew(tiles[i, j]);
                            else if (tiles[i, j].Item1.slope() == 1)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].Item1.slope(4);
                                else if (flipX)
                                    tiles[i, j].Item1.slope(2);
                                else if (flipY)
                                    tiles[i, j].Item1.slope(3);
                                writer.WriteTileNew(tiles[i, j]);
                            }
                            else if (tiles[i, j].Item1.slope() == 2)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].Item1.slope(3);
                                else if (flipX)
                                    tiles[i, j].Item1.slope(1);
                                else if (flipY)
                                    tiles[i, j].Item1.slope(4);
                                writer.WriteTileNew(tiles[i, j]);
                            }
                            else if (tiles[i, j].Item1.slope() == 3)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].Item1.slope(2);
                                else if (flipX)
                                    tiles[i, j].Item1.slope(4);
                                else if (flipY)
                                    tiles[i, j].Item1.slope(1);
                                writer.WriteTileNew(tiles[i, j]);
                            }
                            else if (tiles[i, j].Item1.slope() == 4)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].Item1.slope(1);
                                else if (flipX)
                                    tiles[i, j].Item1.slope(3);
                                else if (flipY)
                                    tiles[i, j].Item1.slope(2);
                                writer.WriteTileNew(tiles[i, j]);
                            }
                        }
                    }
                }
            }
            else
            {
                Tile[,] tiles = Tools.LoadWorldDataOld(clipboardPath);

                int width = tiles.GetLength(0);
                int height = tiles.GetLength(1);
                int endX = flipX ? -1 : width;
                int endY = flipY ? -1 : height;
                int incX = flipX ? -1 : 1;
                int incY = flipY ? -1 : 1;

                // GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
                using (var writer =
                    new BinaryWriter(
                        new BufferedStream(
                            new GZipStream(File.Open(clipboardPath, FileMode.Create), CompressionMode.Compress), BUFFER_SIZE)))
                {
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(width);
                    writer.Write(height);

                    for (int i = flipX ? width - 1 : 0; i != endX; i += incX)
                    {
                        for (int j = flipY ? height - 1 : 0; j != endY; j += incY)
                        {
                            if (tiles[i, j].slope() == 0)
                                writer.WriteTileOld(tiles[i, j]);
                            else if (tiles[i, j].slope() == 1)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].slope(4);
                                else if (flipX)
                                    tiles[i, j].slope(2);
                                else if (flipY)
                                    tiles[i, j].slope(3);
                                writer.WriteTileOld(tiles[i, j]);
                            }
                            else if (tiles[i, j].slope() == 2)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].slope(3);
                                else if (flipX)
                                    tiles[i, j].slope(1);
                                else if (flipY)
                                    tiles[i, j].slope(4);
                                writer.WriteTileOld(tiles[i, j]);
                            }
                            else if (tiles[i, j].slope() == 3)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].slope(2);
                                else if (flipX)
                                    tiles[i, j].slope(4);
                                else if (flipY)
                                    tiles[i, j].slope(1);
                                writer.WriteTileOld(tiles[i, j]);
                            }
                            else if (tiles[i, j].slope() == 4)
                            {
                                if (flipX && flipY)
                                    tiles[i, j].slope(1);
                                else if (flipX)
                                    tiles[i, j].slope(3);
                                else if (flipY)
                                    tiles[i, j].slope(2);
                                writer.WriteTileOld(tiles[i, j]);
                            }
                        }
                    }
                }
            }

            plr.SendSuccessMessage("Flipped clipboard.");
        }
    }
}