using System;

namespace RecursiveCalculator
{
	/*
	BIDMAS
	there are conflicting sources of information of what to do about DM and AS, as some sources suggest treating them
	as sharing the same order and doing similar operations from left to right, and some say to prioritise D over M.
	
	1+3 = 4
	2+4+6+8 = 20
	5-3+4-2 = 4
	6*6 = 36
	6*6+2+4 = 42
	6*6/6 = 6
	6*2/4 = 3
	(3+2)/4 = 1.25
	3+(2/4) = 3.5
	*/
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter equation:");
			string equation = Console.ReadLine();
			Console.WriteLine($"Result: {Calculate(equation)}");


        }

		static float Calculate(string toCalc)
		{
			string theCalc = toCalc;
			float total = 0;
			//find brackets
			for (int i = 0; i < toCalc.Length; i++)
			{
				if (toCalc[i] == '(')
				{
					int endBracketIndex = IndexOfMatchingBracket(toCalc, i);
					if (endBracketIndex == -1) throw new FormatException("Open bracket is missing a closed bracket!");
					string bracketCalcString = toCalc.Substring(i, endBracketIndex - i);
					total += Calculate(toCalc.Substring(i, IndexOfMatchingBracket(toCalc, i) - i));
				}
			}
			

		}

		//calculate a simple two number one operation calc, e.g 2+2
		static float SingleOperationCalculate(string toCalc)
		{
			foreach (char character in toCalc)
			{
				if (!IsNumber(character) && character != '.')
				{
					//it is an operation
					float result;
					string[] splitCalc;
					float num1;
					float num2;

					switch (character)
					{
						case '+':
							splitCalc = toCalc.Split('+');
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 + num2;
							break;
						case '-':
							splitCalc = toCalc.Split('-');
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 - num2;
							break;
						case '*':
							splitCalc = toCalc.Split('*');
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 * num2;
							break;
						case '/':
							splitCalc = toCalc.Split('/');
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 / num2;
							break;
						case '^':
							splitCalc = toCalc.Split('^');
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = (float) Math.Pow(num1, num2);
							break;
						default:
							throw new FormatException($"Unexpected character {character} found in calculation!");
					}
					
					return result;
				}
			}
			//the entire calculation is a number, so we can just return that.
			return float.Parse(toCalc);
		}

		static int IndexOfMatchingBracket(string calc, int openingIndex)
		{
			if (calc[openingIndex] == '(')
			{
				int depth = 1;
				for (int i = openingIndex + 1; i < calc.Length; i++)
				{
					if (calc[i] == '(') depth++;
					else if (calc[i] == ')') 
					{
						depth--;

						if (depth == 0) return i;
					}
				}
			}

			return -1;
		}

		static bool IsNumber(string toCheck)
		{
			foreach (char character in toCheck)
			{
				if (!IsNumber(character) && character != '.') return false;
			}

			return true;
		}

		static bool IsNumber(char toCheck)
		{
			int asciiCode = (int) toCheck;
			if (!(asciiCode >= 48 && asciiCode <= 57)) return false;
			return true;
		}
    }
}
