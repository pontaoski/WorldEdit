using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Scale : WECommand
	{
		private int scale;
		public Scale(TSPlayer plr, int scale)
			: base(0, 0, 0, 0, plr)
		{
			this.scale = scale;
		}

		public override void Execute()
		{
			string clipboardPath = Tools.GetClipboardPath(plr.User.ID);
			
			Tuple<Tile, string, Item, Item[]>[,] tiles = Tools.LoadWorldDataNew(clipboardPath);
			int width = tiles.GetLength(0);
			int hight = tiles.GetLength(1);
			List<Tile[,]> newtiles = new List<Tile[,]>();

			using (var writer =
				new BinaryWriter(
					new BufferedStream(
						new GZipStream(File.Open(clipboardPath, FileMode.Create), CompressionMode.Compress), 1048576)))
			{
				writer.Write(0);
				writer.Write(0);
				writer.Write(width * scale);
				writer.Write(hight * scale);

				List<Tuple<Tile, string, Item, Item[]>> R = new List<Tuple<Tile, string, Item, Item[]>>();
				// TODO: Decreased scaling
				for (int i = 0; i < width; i++)
				{
					for (int j = 0; j < hight; j++)
					{
						for (int a = 0; a < scale; a++)
						{ writer.Write(tiles[i, j]); }
						R.Add(tiles[i, j]);
						if (j == (hight - 1))
						{
							for (int a = 0; a < (scale - 1); a++)
							{
								foreach (Tuple<Tile, string, Item, Item[]> t in R)
								{
									for (int b = 0; b < scale; b++)
									{ writer.Write(t); }
								}
							}
							R = new List<Tuple<Tile, string, Item, Item[]>>();
						}
					}
				}
			}

			plr.SendSuccessMessage("Scaled clipboard.");
		}
	}
}