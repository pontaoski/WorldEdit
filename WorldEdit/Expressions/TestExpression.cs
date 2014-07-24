using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WorldEdit.Expressions
{
	public delegate bool Test(int i, int j);

	public sealed class TestExpression : Expression
	{
		public Test Test;

		public TestExpression(Test test)
		{
			Test = test;
		}

		public override bool Evaluate(int i, int j)
		{
			return Test(i, j);
		}
	}
}
