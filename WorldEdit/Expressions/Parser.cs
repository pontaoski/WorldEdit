using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace WorldEdit.Expressions
{
	public static class Parser
	{
		public static Expression ParseExpression(IEnumerable<Token> postfix)
		{
			var stack = new Stack<Expression>();
			foreach (var token in postfix)
			{
				switch (token.TokenType)
				{
					case Token.Type.BinaryOperator:
						switch ((OperatorType)token.Value)
						{
							case OperatorType.And:
								stack.Push(new AndExpression(stack.Pop(), stack.Pop()));
								continue;
							case OperatorType.Or:
								stack.Push(new OrExpression(stack.Pop(), stack.Pop()));
								continue;
							case OperatorType.Xor:
								stack.Push(new XorExpression(stack.Pop(), stack.Pop()));
								continue;
							default:
								return null;
						}
					case Token.Type.Test:
						stack.Push(new TestExpression((Test)token.Value));
						continue;
					case Token.Type.UnaryOperator:
						switch ((OperatorType)token.Value)
						{
							case OperatorType.Not:
								stack.Push(new NotExpression(stack.Pop()));
								continue;
							default:
								return null;
						}
					default:
						return null;
				}
			}
			return stack.Pop();
		}
		public static List<Token> ParseInfix(string str)
		{
			str = str.Replace(" ", "").ToLower();
			var tokens = new List<Token>();

			for (int i = 0; i < str.Length; i++)
			{
				switch (str[i])
				{
					case '&':
						tokens.Add(new Token { TokenType = Token.Type.BinaryOperator, Value = OperatorType.And });
						continue;
					case '!':
						tokens.Add(new Token { TokenType = Token.Type.UnaryOperator, Value = OperatorType.Not });
						continue;
					case '|':
						tokens.Add(new Token { TokenType = Token.Type.BinaryOperator, Value = OperatorType.Or });
						continue;
					case '^':
						tokens.Add(new Token { TokenType = Token.Type.BinaryOperator, Value = OperatorType.Xor });
						continue;
					case '(':
						tokens.Add(new Token { TokenType = Token.Type.OpenParentheses });
						continue;
					case ')':
						tokens.Add(new Token { TokenType = Token.Type.CloseParentheses });
						continue;
				}

				var test = new StringBuilder();
				while (i < str.Length && (Char.IsLetterOrDigit(str[i]) || str[i] == '!' || str[i] == '='))
					test.Append(str[i++]);
				i--;

				string[] expression = test.ToString().Split('=');
				string lhs = expression[0];
				string rhs = "";
				bool negated = false;

				if (expression.Length > 1)
				{
					if (lhs[lhs.Length - 1] == '!')
					{
						lhs = lhs.Substring(0, lhs.Length - 1);
						negated = true;
					}
					rhs = expression[1];
				}
				tokens.Add(new Token { TokenType = Token.Type.Test, Value = ParseTest(lhs, rhs, negated) });
			}
			return tokens;
		}
		public static List<Token> ParsePostfix(List<Token> infix)
		{
			var queue = new Queue<Token>();
			var stack = new Stack<Token>();

			foreach (var token in infix)
			{
				switch (token.TokenType)
				{
					case Token.Type.BinaryOperator:
					case Token.Type.OpenParentheses:
					case Token.Type.UnaryOperator:
						stack.Push(token);
						break;
					case Token.Type.CloseParentheses:
						while (stack.Peek().TokenType != Token.Type.OpenParentheses)
							queue.Enqueue(stack.Pop());
						stack.Pop();

						if (stack.Count > 0 && stack.Peek().TokenType == Token.Type.UnaryOperator)
							queue.Enqueue(stack.Pop());
						break;
					case Token.Type.Test:
						queue.Enqueue(token);
						break;
				}
			}

			while (stack.Count > 0)
				queue.Enqueue(stack.Pop());

			return queue.ToList();
		}
		public static Test ParseTest(string lhs, string rhs, bool negated)
		{
			Test test;
			switch (lhs)
			{
				case "honey":
					return test = (i, j) => Main.tile[i, j].liquid > 0 && Main.tile[i, j].liquidType() == 2;
				case "lava":
					return test = (i, j) => Main.tile[i, j].liquid > 0 && Main.tile[i, j].liquidType() == 1;
				case "tile":
					if (String.IsNullOrEmpty(rhs))
						return test = (i, j) => Main.tile[i, j].active();

					List<int> tiles = Tools.GetTileID(rhs);
					if (tiles.Count == 0)
						throw new ArgumentException("No tile matched.");
					if (tiles.Count > 1)
						throw new ArgumentException("More than one tile matched.");
					return test = (i, j) => (Main.tile[i, j].active() && Main.tile[i, j].type == tiles[0]) != negated;
				case "tilepaint":
					{
						if (String.IsNullOrEmpty(rhs))
							return test = (i, j) => Main.tile[i, j].active() && Main.tile[i, j].color() != 0;

						List<int> colors = Tools.GetColorID(rhs);
						if (colors.Count == 0)
							throw new ArgumentException("No color matched.");
						if (colors.Count > 1)
							throw new ArgumentException("More than one color matched.");
						return test = (i, j) => (Main.tile[i, j].active() && Main.tile[i, j].color() == colors[0]) != negated;
					}
				case "wall":
					if (String.IsNullOrEmpty(rhs))
						return test = (i, j) => Main.tile[i, j].wall != 0;

					List<int> walls = Tools.GetTileID(rhs);
					if (walls.Count == 0)
						throw new ArgumentException("No wall matched.");
					if (walls.Count > 1)
						throw new ArgumentException("More than one wall matched.");
					return test = (i, j) => (Main.tile[i, j].wall == walls[0]) != negated;
				case "wallpaint":
					{
						if (String.IsNullOrEmpty(rhs))
							return test = (i, j) => Main.tile[i, j].wall > 0 && Main.tile[i, j].wallColor() != 0;

						List<int> colors = Tools.GetColorID(rhs);
						if (colors.Count == 0)
							throw new ArgumentException("No color matched.");
						if (colors.Count > 1)
							throw new ArgumentException("More than one color matched.");
						return test = (i, j) => (Main.tile[i, j].wall > 0 && Main.tile[i, j].wallColor() == colors[0]) != negated;
					}
				case "water":
					return test = (i, j) => Main.tile[i, j].liquid > 0 && Main.tile[i, j].liquidType() == 0;
				case "wire":
					return test = (i, j) => Main.tile[i, j].wire();
				case "wire2":
					return test = (i, j) => Main.tile[i, j].wire2();
				case "wire3":
					return test = (i, j) => Main.tile[i, j].wire3();
				default:
					throw new ArgumentException("Invalid test.");
			}
		}
		public static bool TryCreateExpression(IEnumerable<string> parameters, out Expression expression)
		{
			expression = null;
			if (parameters.FirstOrDefault() != "=>")
				return false;

			try
			{
				expression = ParseExpression(ParsePostfix(ParseInfix(String.Join(" ", parameters.Skip(1)))));
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
