using Terraria;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class FixGhosts : WECommand
	{
		public FixGhosts(int x, int y, int x2, int y2, TSPlayer plr)
			: base(x, y, x2, y2, plr)
		{
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int signs = 0;
			int frames = 0;
			int chests = 0;
			foreach (Sign sign in Main.sign)
			{
				if (sign == null) continue;
				ushort type1 = Main.tile[sign.x, sign.y].type;
				if ((type1 != TileID.Signs)
					&& (type1 != TileID.Tombstones)
					&& (type1 != TileID.AnnouncementBox))
				{
					Sign.KillSign(sign.x, sign.y);
					signs++;
				}
			}
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					int ID = TEItemFrame.Find(i, j);
					if (ID == -1) { continue; }
					if (Main.tile[i, j].type != TileID.ItemFrame)
					{
						TEItemFrame.Kill(i, j);
						frames++;
					}
				}
			}
			foreach (Chest chest in Main.chest)
			{
				if (chest == null) continue;
				ushort type = Main.tile[chest.x, chest.y].type;
				if ((type != TileID.Containers)
					&& (type != TileID.Containers2)
					&& (type != TileID.Dressers))
				{
					Chest.DestroyChest(chest.x, chest.y);
					chests++;
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Fixed ghost signs ({0}), item frames ({1}) and chests ({2}).", signs, frames, chests);
		}
	}
}