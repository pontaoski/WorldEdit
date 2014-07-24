using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;

namespace WorldEdit.Expressions
{
	public class Token
	{
		public enum Type
		{
			BinaryOperator,
			CloseParentheses,
			OpenParentheses,
			Test,
			UnaryOperator,
		}

		public Token.Type TokenType;
		public object Value;
	}
}
