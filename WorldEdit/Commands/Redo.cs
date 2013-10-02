using System;
using TShockAPI;

namespace WorldEdit.Commands
{
	public class Redo : WECommand
	{
		private int steps;

		public Redo(TSPlayer plr, int steps)
			: base(0, 0, 0, 0, plr)
		{
			this.steps = steps;
		}

		public override void Execute()
		{
			int i = 0;
			for (; WorldEdit.GetPlayerInfo(plr).redoLevel != -1 && i < steps; i++)
			{
				Tools.Redo(plr);
			}
			plr.SendSuccessMessage("Redid last {0}action{1}.", i == 1 ? "" : i + " ", i == 1 ? "" : "s");
		}
	}
}
