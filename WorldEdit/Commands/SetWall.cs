using System;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class SetWall : WECommand
	{
		private int wall;

		public SetWall(int x, int y, int x2, int y2, TSPlayer plr, int wall)
			: base(x, y, x2, y2, plr)
		{
			this.wall = wall;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			for (int i = x; i <= x2; i++)
			{
				for (int j = y; j <= y2; j++)
				{
					if (selectFunc(i, j, plr) && Main.tile[i, j].wall != wall)
					{
						Main.tile[i, j].wall = (byte)wall;
						edits++;
					}
				}
			}
			ResetSection();

			string wallName = wall == 0 ? "air" : "wall " + wall;
			plr.SendSuccessMessage("Set walls to {0}. ({1})", wallName, edits);
		}
	}
}