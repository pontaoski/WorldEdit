using System;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Redo : WECommand
	{
		string accountName;
		int steps;

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
			if (i == 0)
				plr.SendErrorMessage("Failed to redo any actions.");
			else
				plr.SendSuccessMessage("Redid {0}'s last {1}action{2}.", accountName, i == 1 ? "" : i + " ", i == 1 ? "" : "s");
		}
	}
}
