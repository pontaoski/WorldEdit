﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WorldEdit.Expressions
{
	public class XorExpression : Expression
	{
		public XorExpression(Expression left, Expression right)
		{
			Left = left;
			Right = right;
		}

		public override bool Evaluate(int i, int j)
		{
			return Left.Evaluate(i, j) ^ Right.Evaluate(i, j);
		}
	}
}