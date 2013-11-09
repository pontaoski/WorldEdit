using System;
using System.IO;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Rotate : WECommand
	{
		private int degrees;

		public Rotate(TSPlayer plr, int degrees)
			: base(0, 0, 0, 0, plr)
		{
			this.degrees = degrees;
		}

		public override void Execute()
		{
			string clipboardPath = Tools.GetClipboardPath(plr.UserAccountName);
			Tile[,] tiles = Tools.LoadWorldData(clipboardPath);
			int lenX = tiles.GetLength(0);
			int lenY = tiles.GetLength(1);

			using (BinaryWriter writer = new BinaryWriter(new FileStream(clipboardPath, FileMode.Create)))
			{
				switch (((degrees / 90) % 4 + 4) % 4)
				{
					case 0:
						writer.Write(lenX);
						writer.Write(lenY);
						for (int i = 0; i < lenX; i++)
						{
							for (int j = 0; j < lenY; j++)
							{
								writer.Write(tiles[i, j]);
							}
						}
						break;
					case 1:
						writer.Write(lenY);
						writer.Write(lenX);
						for (int j = lenY - 1; j >= 0; j--)
						{
							for (int i = 0; i < lenX; i++)
							{
								writer.Write(tiles[i, j]);
							}
						}
						break;
					case 2:
						writer.Write(lenX);
						writer.Write(lenY);
						for (int i = lenX - 1; i >= 0; i--)
						{
							for (int j = lenY - 1; j >= 0; j--)
							{
								writer.Write(tiles[i, j]);
							}
						}
						break;
					case 3:
						writer.Write(lenY);
						writer.Write(lenX);
						for (int j = 0; j < lenY; j++)
						{
							for (int i = lenX - 1; i >= 0; i--)
							{
								writer.Write(tiles[i, j]);
							}
						}
						break;
				}
			}
			plr.SendSuccessMessage("Rotated clipboard {0} degrees.", degrees);
		}
	}
}