using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;

namespace WorldEdit.Expressions
{
	public abstract class Expression
	{
		public Expression Left;
		public Expression Right;

		public abstract bool Evaluate(int i, int j);
	}
}
