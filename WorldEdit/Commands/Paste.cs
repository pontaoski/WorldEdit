using System;
using System.IO;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Paste : WECommand
	{
		public Paste(int x, int y, TSPlayer plr)
			: base(x, y, 0, 0, plr)
		{
			string clipboardPath = Tools.GetClipboardPath(plr);
			using (BinaryReader reader = new BinaryReader(new FileStream(clipboardPath, FileMode.Open)))
			{
				x2 = x + reader.ReadInt32() - 1;
				y2 = y + reader.ReadInt32() - 1;
			}
		}

		public override void Execute()
		{
			string clipboardPath = Tools.GetClipboardPath(plr);
			using (BinaryReader reader = new BinaryReader(new FileStream(clipboardPath, FileMode.Open)))
			{
				reader.ReadInt64();
				Tools.PrepareUndo(x, y, x2, y2, plr);
				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
					{
						Main.tile[i, j] = Tools.ReadTile(reader);
					}
				}
			}
			ResetSection();
			plr.SendSuccessMessage("Pasted clipboard to selection.");
		}
	}
}