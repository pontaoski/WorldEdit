using System;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Undo : WECommand
	{
		string accountName;
		int steps;

		public Undo(TSPlayer plr, string accountName, int steps)
			: base(0, 0, 0, 0, plr)
		{
			this.accountName = accountName;
			this.steps = steps;
		}

		public override void Execute()
		{
			int i = 0;
			for (; i < steps && Tools.Undo(accountName); i++)
			{
			}
			plr.SendSuccessMessage("Undid last {0}action{1}.", i == 1 ? "" : i + " ", i == 1 ? "" : "s");
		}
	}
}