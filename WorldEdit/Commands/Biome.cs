using System;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Biome : WECommand
	{
		private byte biome1;
		private byte biome2;

		public Biome(int x, int y, int x2, int y2, TSPlayer plr, byte biome1, byte biome2)
			: base(x, y, x2, y2, plr)
		{
			this.biome1 = biome1;
			this.biome2 = biome2;
		}

		public override void Execute()
		{
			Tools.PrepareUndo(x, y, x2, y2, plr);
			int edits = 0;
			if (biome1 != biome2)
			{
				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
					{
						if (selectFunc(i, j, plr) && Main.tile[i, j].active())
						{
							for (int k = 0; k < WorldEdit.BiomeConversions[biome1].Length; k++)
							{
								if (Main.tile[i, j].type == WorldEdit.BiomeConversions[biome1][k])
								{
									SetTile(i, j, WorldEdit.BiomeConversions[biome2][k]);
									edits++;
									break;
								}
							}
						}
					}
				}
				ResetSection();
			}
			plr.SendSuccessMessage("Converted {0} to {1}. ({2})", WorldEdit.BiomeNames[biome1], WorldEdit.BiomeNames[biome2], edits);
		}
	}
}
