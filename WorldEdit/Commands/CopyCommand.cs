using System;
using System.IO;
using Terraria;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class CopyCommand : WECommand
	{
		public CopyCommand(int x, int y, int x2, int y2, TSPlayer plr)
			: base(x, y, x2, y2, plr)
		{
		}

		public override void Execute()
		{
			string clipboardPath = Tools.GetClipboardPath(plr);
			using (BinaryWriter writer = new BinaryWriter(new FileStream(clipboardPath, FileMode.Create)))
			{
				writer.Write(x2 - x + 1);
				writer.Write(y2 - y + 1);
				for (int i = x; i <= x2; i++)
				{
					for (int j = y; j <= y2; j++)
					{
						Tools.WriteTile(writer, Main.tile[i, j]);
					}
				}
			}
			plr.SendSuccessMessage(String.Format("Copied selection to clipboard."));
		}
	}
}