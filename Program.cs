using System;
using System.Text.RegularExpressions;

namespace RecursiveCalculator
{
	/*
	BIDMAS
	there are conflicting sources of information of what to do about DM and AS, as some sources suggest treating them
	as sharing the same order and doing similar operations from left to right, and some say to prioritise D over M.
	
	1+3 = 4
	-9+2 = -7
	2+4+6+8 = 20
	5-3+4-2 = 4
	6*6 = 36
	6*6+2+4 = 42
	6*6/6 = 6
	6*2/4 = 3
	(3+2)/4 = 1.25
	3+(2/4) = 3.5
	6+(12/2)-9+(6*9)+4=61
	5*((4+3)*2+7)=105
	*/
    class Program
    {
		static readonly bool DebugMode = false;
        static void Main(string[] args)
        {
			bool Running = true;
			while (Running)
			{
				Console.WriteLine("Enter equation (enter 'exit' to exit):");
				string equation = Console.ReadLine().Replace(" ", "");
				if (equation == "exit") 
				{
					Running = false;
				}

				if (Running)
				{
					try
					{
						float result = Calculate(equation);
						Console.WriteLine($"Result: {result}");
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
						Running = false;
					}
				}
			}

			Console.WriteLine("Press any key to exit!");
			Console.ReadKey(true);
        }

		static float Calculate(string toCalc)
		{
			Debug("DEBUG: Calculate() Begin Calculate");
			string theCalc = toCalc;

			//find brackets, calculate them and substitute them into the string.
			for (int i = 0; i < toCalc.Length; i++)
			{
				if (toCalc[i] == '(')
				{
					Debug("DEBUG: Calculate() Found bracket!");
					int endBracketIndex = IndexOfMatchingBracket(toCalc, i);

					if (endBracketIndex == -1) 
					{
						throw new FormatException("Open bracket is missing a closed bracket!");
					}

					Debug($"DEBUG: Calculate() [brackets]: toCalc[endBracketIndex] {toCalc[endBracketIndex]}");
					string bracketCalcString = toCalc.Substring(i + 1, endBracketIndex - i - 1);
					//RECURSION TIME
					//sorry, ahem
					/*
						|--\ |---  /- |   | |--\    ___ _____  /--\  |\   |
						|   ||___ |   |   | |   |  /      |   |    | | \  |
						|  / |    |   |   | |  /   \_     |   |    | |  \ |
						|  \ |___  \_ |___| |  \  ___\  __|__  \__/  |   \|
					*/
					float bracketResult = Calculate(bracketCalcString);
					Debug($"DEBUG: Calculate() [brackets]: result: {bracketResult}, bracket calc string: {bracketCalcString}");
					theCalc = ReplaceFirstOccurrence(theCalc, '(' + bracketCalcString + ')', bracketResult.ToString());
					Debug($"DEBUG: Calculate() [brackets]: replaced calc: {theCalc}");
				}
			}

			//with all the brackets removed, it is now safe to treat it as if there are no brackets. we use 'theCalc' from here.

			//each sub array is a 'priority bracket'. Brackets are treated as top priority anyways. all operators in a bracket
			//will be evaluated left to right in the calc.
			char[][] operators = new char[][] {new char[] {'^'}, new char[] {'/', '*'}, new char[] {'+', '-'}};

			//loop through the calc by order of BIDMAS, and replace found calcs with the result of said calc
			//this should eventually mean the calc will be reduced to the result.
			foreach (char[] currOperators in operators)
			{
				Func<int> len = () => theCalc.Length;
				for (int i = 1; i < len(); i++)
				{
					if (ArrayContains(currOperators, theCalc[i]))
					{
						int? prevNumStartIndex = null;
						int? postNumStartIndex = null;

						//search for starts of numbers by going until you reach another symbol

						for (int i2 = i - 1; i2 >= 0; i2--)
						{
							if (i2 == 0 && theCalc[i2] == '-') 
							{
								continue;
							}

							if (!IsNumberOrDecimalPlace(theCalc[i2])) 
							{
								if (theCalc[i2] == '-' && !IsNumberOrDecimalPlace(theCalc[Math.Clamp(i2 - 1, 0, theCalc.Length)])) 
								{
									prevNumStartIndex = i2;
								}
								else 
								{
									prevNumStartIndex = i2 + 1;
								}
								break;
							}
						}
						//if an index was not found, assume it's at the very start of theCalc
						if (prevNumStartIndex == null) 
						{
							prevNumStartIndex = 0;
						}

						for (int i2 = i + 1; i2 < theCalc.Length; i2++)
						{
							if (!IsNumberOrDecimalPlace(theCalc[i2])) 
							{
								//continue iterating if it's a negative
								if (i2 == i + 1 && theCalc[i2] == '-') 
								{
									continue;
								}
								else 
								{
									postNumStartIndex = i2 - 1;
								}

								break;
							}
						}

						if (postNumStartIndex == null) 
						{
							postNumStartIndex = theCalc.Length - 1;
						}

						Debug($"DEBUG: Calculate() prevNumSI: {prevNumStartIndex}, postNumSI: {postNumStartIndex}, i: {i}");
						string splitCalc = theCalc.Substring((int) prevNumStartIndex, (int) (postNumStartIndex - prevNumStartIndex + 1));
						Debug($"DEBUG: Calculate() splitCalc: {splitCalc}");
						float result = SingleOperationCalculate(splitCalc);
						string strResult = result.ToString();
						Debug($"DEBUG: Calculate() pre replace theCalc: {theCalc}, splitCalc: {splitCalc}, result: {result}, result.ToString(): {result.ToString()}");
						theCalc = ReplaceFirstOccurrence(theCalc, splitCalc, strResult);
						Debug($"DEBUG: Calculate() Replaced calc: {theCalc}");
						//set i to be at the start of the result string
						//i is the index of the operator in the middle of the split out calc
						//set i to the index of the start. we'll go over the result of this loop but it won't do anything
						//so it's slightly inefficient but more reliable.
						i = (int) prevNumStartIndex;
					}
				}
			}
			
			Debug($"DEBUG: Calculate() theCalc: {theCalc}");
			return float.Parse(theCalc);
		}

		//calculate a simple two number one operation calc, e.g 2+2
		static float SingleOperationCalculate(string toCalc)
		{
			Debug("DEBUG: SingleOperationCalculate() Calc: " + toCalc);
			for (int i = 0; i < toCalc.Length; i++)
			{
				char character = toCalc[i];
				if (!IsNumberOrDecimalPlace(character))
				{
					if (character == '-' && i == 0) 
					{
						continue;
					}

					//it is an operation
					float result;
					string[] splitCalc;
					float num1;
					float num2;

					switch (character)
					{
						case '+':
							splitCalc = toCalc.Split('+');
							Debug($"DEBUG: SingleOperationCalculate() splitCalc 0: {splitCalc[0]}, 1: {splitCalc[1]}");
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 + num2;
							break;
						case '-':
							splitCalc = toCalc.Split('-');
							Debug($"DEBUG: SingleOperationCalculate() splitCalc 0: {splitCalc[0]}, 1: {splitCalc[1]}");
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 - num2;
							break;
						case '*':
							splitCalc = toCalc.Split('*');
							Debug($"DEBUG: SingleOperationCalculate() splitCalc 0: {splitCalc[0]}, 1: {splitCalc[1]}");
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 * num2;
							break;
						case '/':
							splitCalc = toCalc.Split('/');
							Debug($"DEBUG: SingleOperationCalculate() splitCalc 0: {splitCalc[0]}, 1: {splitCalc[1]}");
							num1 = float.Parse(splitCalc[0]);
							num2 = float.Parse(splitCalc[1]);
							result = num1 / num2;
							break;
						case '^':
							splitCalc = toCalc.Split('^');
							Debug($"DEBUG: SingleOperationCalculate() splitCalc 0: {splitCalc[0]}, 1: {splitCalc[1]}");
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
					if (calc[i] == '(') 
					{
						depth++;
					}
					else if (calc[i] == ')') 
					{
						depth--;
					}

					if (depth == 0) 
					{
						return i;
					}
				}
			}

			return -1;
		}

		static bool IsNumberOrDecimalPlace(char toCheck)
		{
			int asciiCode = (int) toCheck;
			if (!((asciiCode >= 48 && asciiCode <= 57) || toCheck == '.')) 
			{
				return false;
			}

			return true;
		}

		//from https://stackoverflow.com/a/141076, referenced 28/02/2021
		static string ReplaceFirstOccurrence(string originalString, string toReplace, string replaceWith)
		{
			int pos = originalString.IndexOf(toReplace);
  			if (pos < 0)
			{
				return originalString;
			}
			return originalString.Substring(0, pos) + replaceWith + originalString.Substring(pos + toReplace.Length);  
		}

		static bool ArrayContains<T>(T[] array, T contains)
		{
			foreach (T obj in array)
			{
				if (obj.Equals(contains)) 
				{
					return true;
				}
			}

			return false;
		}

		static void Debug(string message)
		{
			if (Program.DebugMode) 
			{
				Console.WriteLine(message);
			}
		}
    }
}
//VVS, U., 2008. c# - How do I replace the *first instance* of a string in .NET? [online] Stack Overflow. Available at: <https://stackoverflow.com/a/141076> [Accessed 28 Feb. 2021].