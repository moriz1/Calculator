using UnityEngine;
using System;
//using System.Text;
//using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CalculatorLogic : MonoBehaviour {

	public const string DefaultInputBoxText = "0";
	public const string DefaultHistoryBoxText = "";

	public Text InputBox;
	public Text HistoryBox;

	private Stack<string> SymbolStack;
	private Queue<string> InputQueue;
	private Queue<string> OutputQueue;

	private string currentInputToken;

	void Start() {
		if (InputBox.gameObject != null) {
			InputBox.text = DefaultInputBoxText;
		}
		else {
			Debug.Log("InputBox object not assigned!");
		}

		if (HistoryBox.gameObject != null) {
			HistoryBox.text = DefaultHistoryBoxText;
		}
		else {
			Debug.Log("HistoryBox object not assigned!");
		}

		SymbolStack = new Stack<string> ();
		InputQueue = new Queue<string> ();
		OutputQueue = new Queue<string> ();

		currentInputToken = DefaultInputBoxText;
	}

	public void OnButtonClicked(Text t) {
		if (t.text == "EQUAL") {
			if (InputQueue.Count > 0) {
				InputQueue.Enqueue(currentInputToken);

				ToRPN();

				double answer = Calculate();
				HistoryBox.text = HistoryBox.text + InputBox.text + "\n" + "= " + answer + "\n";
				InputBox.text = string.Empty + answer;

				InputQueue.Clear();
				OutputQueue.Clear();
				SymbolStack.Clear();

				currentInputToken = answer.ToString();
				InputQueue.Enqueue(currentInputToken);
			}
		}
		else {
			switch (t.text) {

			case "+":
			case "SUBSTRACT":
			case "*":
			case "/":
			case "%":
			case "(":
			case ")":
				if (t.text == "SUBSTRACT") {
					t.text = "-";
				}

				HistoryBox.text = HistoryBox.text + currentInputToken + t.text;

				InputQueue.Enqueue(currentInputToken);
				InputQueue.Enqueue(t.text);
				currentInputToken = string.Empty;
				break;

			case "+/-":
				currentInputToken = (!(currentInputToken.StartsWith("-"))) 
					? ("-" + currentInputToken) : (currentInputToken.Substring(1));
				break;
			case "D":
				currentInputToken = (!(currentInputToken.Contains(".")))
					? (currentInputToken + ".") : (currentInputToken);
				break;

			case "C":
				currentInputToken = DefaultInputBoxText;
				InputQueue.Clear();
				InputBox.text = DefaultInputBoxText;
				break;
			case "AC":
				currentInputToken = DefaultInputBoxText;
				InputQueue.Clear();
				InputBox.text = DefaultInputBoxText;
				HistoryBox.text = string.Empty;
				break;
			default:
				if ((currentInputToken.Length == 1) && (currentInputToken.StartsWith("0"))) {
					currentInputToken = t.text;
				}
				else {
					currentInputToken = currentInputToken + t.text;
				}
				break;
			}

			InputBox.text = currentInputToken;
		}
	}

	private void ToRPN() {
		string token;
		double number;

		while (InputQueue.Count > 0) {
			token = InputQueue.Dequeue();

			if (double.TryParse(token, out number)) {
				OutputQueue.Enqueue(token);
			}

			if (IsOperator(token)) {
				while ((SymbolStack.Count > 0) && IsOperator(SymbolStack.Peek())) {
					//since all supported operators are left associative, i'm going to skip this check
					if (ComparePrecedence(token, SymbolStack.Peek()) <= 0) {
						OutputQueue.Enqueue(SymbolStack.Pop());
						break;
					}
				}

				SymbolStack.Push(token);
			}

			if (token == "(") {
				SymbolStack.Push(token);
			}

			if (token == ")") {
				while ((SymbolStack.Count > 0) && (SymbolStack.Peek() != "(")) {
					OutputQueue.Enqueue(SymbolStack.Pop());
				}

				SymbolStack.Pop();
			}
		}

		while (SymbolStack.Count > 0) {
			OutputQueue.Enqueue(SymbolStack.Pop());
		}
	}

	private double Calculate() {
		Stack<string> calcStack = new Stack<string> ();
		string token;

		while (OutputQueue.Count > 0) {
			token = OutputQueue.Dequeue();

			if (!IsOperator(token)) {
				calcStack.Push(token);
			}
			else {
				//pop top two tokens
				double d2 = ConvertToDouble(calcStack.Pop());
				double d1 = ConvertToDouble(calcStack.Pop());

				double result = token.CompareTo("+") == 0 ? d1 + d2 :
								token.CompareTo("-") == 0 ? d1 - d2 :
								token.CompareTo("*") == 0 ? d1 * d2 :
								token.CompareTo("/") == 0 ? d1 / d2 :
															d1 % d2;

				calcStack.Push(result.ToString());
			}
		}

		return ConvertToDouble(calcStack.Pop());
	}

	private bool IsOperator(string token) {
		switch (token) {
			
		case "+":
		case "-":
		case "*":
		case "/":
		case "%":
			return true;
		default:
			return false;
		}
	}

	private bool IsLessThanOrEqualPrecedence() {
		if ((SymbolStack.Peek () == "*") || (SymbolStack.Peek () == "/")
			|| (SymbolStack.Peek () == "%")) {

			return true;
		}

		return false;
	}

	private int ComparePrecedence(string token, string top) {
		int left = 0;
		int right = 0;

		if (token == "*" || token == "/" || token == "%") {
			left = 0;
		}
		else {
			left = 1;
		}

		if (top == "*" || top == "/" || top == "%") {
			right = 0;
		}
		else {
			right = 1;
		}

		return right - left;
	}

	private double ConvertToDouble(string token) {
		double result;

		try {
			result = Convert.ToDouble(token);
		}
		catch (FormatException) {
			Debug.Log("Incorrect token type passed to ConvertToDouble!");
			result = 0;
		}

		return result;
	}
}
