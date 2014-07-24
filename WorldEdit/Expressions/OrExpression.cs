using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldEdit.Expressions
{
	public class OrExpression : Expression
	{
		public OrExpression(Expression left, Expression right)
		{
			Left = left;
			Right = right;
		}

		public override bool Evaluate(int i, int j)
		{
			return Left.Evaluate(i, j) || Right.Evaluate(i, j);
		}
	}
}