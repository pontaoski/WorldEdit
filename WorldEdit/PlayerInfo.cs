using System;
using System.Collections.Generic;
using Terraria;

namespace WorldEdit
{
	public class PlayerInfo
	{
		private int _x = -1;
		private int _x2 = -1;
		private int _y = -1;
		private int _y2 = -1;

        public const string Key = "WorldEdit_Data";

        public Dictionary<string, WorldSectionData> LoadedSchematics = new Dictionary<string, WorldSectionData>();
        public int Point = 0;
		public Selection Select = null;
		public int X
		{
			get => _x;
			set => _x = Math.Max(0, value);
		}
		public int X2
		{
			get => _x2;
			set => _x2 = Math.Min(value, Main.maxTilesX - 1);
		}
		public int Y
		{
			get => _y;
			set => _y = Math.Max(0, value);
		}
		public int Y2
		{
			get => _y2;
			set => _y2 = Math.Min(value, Main.maxTilesY - 1);
		}
	}
}