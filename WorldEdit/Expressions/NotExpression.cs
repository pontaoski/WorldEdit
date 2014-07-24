using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldEdit.Expressions
{
	public class NotExpression : Expression
	{
		public NotExpression(Expression expression)
		{
			Left = expression;
		}

		public override bool Evaluate(int i, int j)
		{
			return !Left.Evaluate(i, j);
		}
	}
}