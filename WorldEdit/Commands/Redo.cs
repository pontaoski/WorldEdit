using System;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Redo : WECommand
	{
		string accountName;
		private int steps;

		public Redo(TSPlayer plr, string accountName, int steps)
			: base(0, 0, 0, 0, plr)
		{
			this.accountName = accountName;
			this.steps = steps;
		}

		public override void Execute()
		{
			int i = 0;
			for (; i < steps && Tools.Redo(accountName); i++)
			{
			}
			plr.SendSuccessMessage("Redid last {0}action{1}.", i == 1 ? "" : i + " ", i == 1 ? "" : "s");
		}
	}
}
