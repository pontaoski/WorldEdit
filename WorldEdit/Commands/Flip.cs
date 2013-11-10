using System;
using System.IO;
using System.IO.Compression;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Flip : WECommand
	{
		const int buffer = 2097152;
		byte direction;

		public Flip(TSPlayer plr, byte direction)
			: base(0, 0, 0, 0, plr)
		{
			this.direction = direction;
		}

		public override void Execute()
		{
			string clipboardPath = Tools.GetClipboardPath(plr.UserAccountName);
			Tile[,] tiles = Tools.LoadWorldData(clipboardPath);

			int width = tiles.GetLength(0);
			int height = tiles.GetLength(1);
			bool flipX = (direction & 1) == 1;
			bool flipY = (direction & 2) == 2;
			int endX = flipX ? -1 : width;
			int endY = flipY ? -1 : height;
			int incX = flipX ? -1 : 1;
			int incY = flipY ? -1 : 1;

			// GZipStream is already buffered, but it's much faster to have a 2 MB buffer.
			using (var writer =
				new BinaryWriter(
					new BufferedStream(
						new GZipStream(File.Open(clipboardPath, FileMode.Create), CompressionMode.Compress), buffer)))
			{
				writer.Write(0);
				writer.Write(0);
				writer.Write(width);
				writer.Write(height);

				for (int i = flipX ? width - 1 : 0; i != endX; i += incX)
				{
					for (int j = flipY ? height - 1 : 0; j != endY; j += incY)
						writer.Write(tiles[i, j]);
				}
			}

			plr.SendSuccessMessage("Flipped clipboard.");
		}
	}
}