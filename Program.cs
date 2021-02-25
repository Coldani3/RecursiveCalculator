using System;
using System.Text.RegularExpressions;

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
	6+(12/2)-9+(6*9)+4=61
	*/
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter equation:");
			string equation = Console.ReadLine();
			try
			{
				float result = Calculate(equation);
				Console.WriteLine($"Result: {result}");
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			Console.ReadKey(true);
        }

		static float Calculate(string toCalc)
		{
			string theCalc = toCalc;
			float total = 0;
			//first, find brackets, calculate them and substitute them into the string.
			for (int i = 0; i < toCalc.Length; i++)
			{
				if (toCalc[i] == '(')
				{
					int endBracketIndex = IndexOfMatchingBracket(toCalc, i);
					if (endBracketIndex == -1) throw new FormatException("Open bracket is missing a closed bracket!");
					string bracketCalcString = toCalc.Substring(i, endBracketIndex - i);
					//RECURSION TIME
					float bracketResult = Calculate(bracketCalcString);
					ReplaceFirstOccurrence(theCalc, bracketCalcString, bracketResult.ToString());
					//theCalc.Replace(bracketCalcString, bracketResult.ToString());
				}
			}

			//with all the brackets removed, it is now safe to treat it as if there are no brackets. we use 'theCalc' from here.
			char[] operators = new char[] {'^', '/', '*', '+', '-'};

			//loop through the calc by order of BIDMAS, and replace found calcs with the result of said calc
			//this should eventually mean the calc will be reduced to the result.

			foreach (char currOperator in operators)
			{
				for (int i = 0; i < theCalc.Length; i++)
				{
					if (theCalc[i] == currOperator)
					{
						int? prevNumStartIndex = null;
						int? postNumStartIndex = null;

						for (int i2 = i; i2 >= 0; i2--)
						{
							if (!IsNumberOrDecimalPlace(theCalc[i2])) 
							{
								prevNumStartIndex = i2 + 1;
								break;
							}
						}
						//if an index was not found, assume it's at the very start of theCalc
						if (prevNumStartIndex == null) prevNumStartIndex = 0;

						for (int i2 = i; i2 < theCalc.Length; i2++)
						{
							if (!IsNumberOrDecimalPlace(theCalc[i2])) 
							{
								postNumStartIndex = i2 - 1;
								break;
							}
						}

						if (postNumStartIndex == null) postNumStartIndex = theCalc.Length - 1;

						string splitCalc = theCalc.Substring((int) prevNumStartIndex, (int) (postNumStartIndex - prevNumStartIndex));
						float result = SingleOperationCalculate(splitCalc);
						theCalc = ReplaceFirstOccurrence(theCalc, splitCalc, result.ToString());
						//set i to be at the start of the result string
						//i is the index of the operator in the middle of the split out calc
						//set i to the index of the start. we'll go over the result of this loop but it won't do anything
						//so it's slightly inefficient but more reliable.
						i = (int) prevNumStartIndex;
						//i += result.ToString().Length - 1;
						//i -= splitCalc.Length - result.ToString().Length;
					}
				}
			}
			
			return float.Parse(theCalc);
		}

		//calculate a simple two number one operation calc, e.g 2+2
		static float SingleOperationCalculate(string toCalc)
		{
			foreach (char character in toCalc)
			{
				if (!IsNumberOrDecimalPlace(character))
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
					else if (calc[i] == ')') depth--;

					if (depth == 0) return i;
				}
			}

			return -1;
		}

		static bool IsNumber(string toCheck)
		{
			foreach (char character in toCheck)
			{
				if (!IsNumberOrDecimalPlace(character) && character != '.') return false;
			}

			return true;
		}

		static bool IsNumberOrDecimalPlace(char toCheck)
		{
			int asciiCode = (int) toCheck;
			if (!((asciiCode >= 48 && asciiCode <= 57) || toCheck == '.')) return false;
			return true;
		}

		//from https://stackoverflow.com/a/146747, referenced 25/02/2021
		static string ReplaceFirstOccurrence(string originalString, string toReplace, string replaceWith)
		{
			Regex regex = new Regex("foo");
			string result = regex.Replace("foo1 foo2 foo3 foo4", "bar", 1);  
			return result;
		}
    }
}
