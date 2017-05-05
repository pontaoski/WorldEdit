using System;
using System.IO;
using System.IO.Compression;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace WorldEdit.Commands
{
    public class Cut : WECommand
    {
        private const int BUFFER_SIZE = 1048576;

        public Cut(int x, int y, int x2, int y2, TSPlayer plr)
            : base(x, y, x2, y2, plr)
        {
        }

        public override void Execute()
        {
            foreach (string fileName in Directory.EnumerateFiles("worldedit", String.Format("redo-{0}-*.dat", plr.User.ID)))
                File.Delete(fileName);

            if (WorldEdit.Database.GetSqlType() == SqlType.Mysql)
                WorldEdit.Database.Query("INSERT IGNORE INTO WorldEdit VALUES (@0, -1, -1)", plr.User.ID);
            else
                WorldEdit.Database.Query("INSERT OR IGNORE INTO WorldEdit VALUES (@0, 0, 0)", plr.User.ID);
            WorldEdit.Database.Query("UPDATE WorldEdit SET RedoLevel = -1 WHERE Account = @0", plr.User.ID);
            WorldEdit.Database.Query("UPDATE WorldEdit SET UndoLevel = UndoLevel + 1 WHERE Account = @0", plr.User.ID);

            int undoLevel = 0;
            using (var reader = WorldEdit.Database.QueryReader("SELECT UndoLevel FROM WorldEdit WHERE Account = @0", plr.User.ID))
            {
                if (reader.Read())
                    undoLevel = reader.Get<int>("UndoLevel");
            }

            string Old = Tools.GetClipboardPath(plr.User.ID, false, false);
            string New = Tools.GetClipboardPath(plr.User.ID, false, true);

            string undoPath = Path.Combine("worldedit", String.Format("undo-{0}-{1}.dat", plr.User.ID, undoLevel));
            // GZipStream is already buffered, but it's much faster to have a 1 MB buffer.
            using (var writer =
                new BinaryWriter(
                    new BufferedStream(
                        new GZipStream(File.Open(undoPath, FileMode.Create), CompressionMode.Compress), BUFFER_SIZE)))
            {
                writer.Write(x);
                writer.Write(y);
                writer.Write(x2 - x + 1);
                writer.Write(y2 - y + 1);

                for (int i = x; i <= x2; i++)
                {
                    for (int j = y; j <= y2; j++)
                    {
                        writer.WriteTileNew(Main.tile[i, j], i, j);
                        var tile = Main.tile[i, j];
                        if (((tile.type == Terraria.ID.TileID.Signs)
                            || (tile.type == Terraria.ID.TileID.Tombstones)
                            || (tile.type == Terraria.ID.TileID.AnnouncementBox)
                            || (tile.type == Terraria.ID.TileID.Containers)
                            || (tile.type == Terraria.ID.TileID.Dressers)
                            || (tile.type == Terraria.ID.TileID.ItemFrame))
                            && ((tile.frameX % 36 == 0) && (tile.frameY == 0)))
                        {
                            if ((tile.type == Terraria.ID.TileID.Signs)
                                || (tile.type == Terraria.ID.TileID.Tombstones)
                                || (tile.type == Terraria.ID.TileID.AnnouncementBox))
                            { Sign.KillSign(i, j); }
                            if ((tile.type == Terraria.ID.TileID.Containers)
                                || (tile.type == Terraria.ID.TileID.Dressers))
                            { Chest.DestroyChest(i, j); }
                            if (tile.type == Terraria.ID.TileID.ItemFrame)
                            { Terraria.GameContent.Tile_Entities.TEItemFrame.Kill(i, j); }
                        }
                        Main.tile[i, j] = new Tile();
                    }
                }
            }
            if (File.Exists(Old)) File.Delete(Old);
            if (File.Exists(New)) File.Delete(New);
            File.Copy(undoPath, New);

            ResetSection();
            plr.SendSuccessMessage("Cut selection. ({0})", (x2 - x + 1) * (y2 - y + 1));
        }
    }
}
