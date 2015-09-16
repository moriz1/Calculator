using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CalculatorLogic : MonoBehaviour {

	public const string DEFAULT_INPUTBOX_TEXT = "0";
	public const string DEFAULT_HISTORYBOX_TEXT = "";

	public Text InputBox;
	public Text HistoryBox;

	private Stack<string> SymbolStack;
	private Queue<string> InputQueue;
	private Queue<string> OutputQueue;

	private string currentInputToken;

	private bool equalPressed = false;

	void Start() {
		if (InputBox.gameObject != null) {
			InputBox.text = DEFAULT_INPUTBOX_TEXT;
		}
		else {
			Debug.Log("InputBox object not assigned!");
		}

		if (HistoryBox.gameObject != null) {
			HistoryBox.text = DEFAULT_HISTORYBOX_TEXT;
		}
		else {
			Debug.Log("HistoryBox object not assigned!");
		}

		SymbolStack = new Stack<string> ();
		InputQueue = new Queue<string> ();
		OutputQueue = new Queue<string> ();

		currentInputToken = DEFAULT_INPUTBOX_TEXT;
	}

	//all buttons trigger this function
	public void OnButtonClicked(Text t) {
		if (t.text == "EQUAL") {
			if (InputQueue.Count > 0) {
				InputQueue.Enqueue(currentInputToken);

				RemoveBlanks();

				if (!IsValidInputQueue()) {
					HistoryBox.text = HistoryBox.text + InputBox.text + "\n" + "= NOT VALID INPUT\n";
					InputBox.text = string.Empty + "NOT VALID INPUT";

					InputQueue.Clear();
					OutputQueue.Clear();
					SymbolStack.Clear();
				}
				else {
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
				equalPressed = true;
			}
		}
		else {
			switch (t.text) {

				//handle operators and brackets
			case "+":
			case "SUBSTRACT":
			case "*":
			case "/":
			case "%":
			case "(":
			case ")":
				//push current accumulated input into InputQueue
				InputQueue.Enqueue(currentInputToken);

				//for some reason, Unity doesn't handle "-" very well
				//this logic appends the operator into history, and pushes it into InputQueue
				if (t.text == "SUBSTRACT") {
					HistoryBox.text = HistoryBox.text + currentInputToken + "-";
					InputQueue.Enqueue("-");
				}
				else {
					HistoryBox.text = HistoryBox.text + currentInputToken + t.text;
					InputQueue.Enqueue(t.text);
				}

				//clear currentInputToken
				currentInputToken = string.Empty;
				InputBox.text = DEFAULT_INPUTBOX_TEXT;

				equalPressed = false;
				break;

				//other inputs
			case "+/-":
				currentInputToken = (!(currentInputToken.StartsWith("-"))) 
					? ("-" + currentInputToken) : (currentInputToken.Substring(1));
				break;
			case "D":
				currentInputToken = (!(currentInputToken.Contains(".")))
					? (currentInputToken + ".") : (currentInputToken);
				break;

			case "C":
				currentInputToken = DEFAULT_INPUTBOX_TEXT;
				//InputQueue.Clear();
				InputBox.text = DEFAULT_INPUTBOX_TEXT;
				break;
			case "AC":
				currentInputToken = DEFAULT_INPUTBOX_TEXT;
				InputQueue.Clear();
				InputBox.text = DEFAULT_INPUTBOX_TEXT;
				HistoryBox.text = string.Empty;
				break;

				//numbers
			default:
				if (equalPressed) {
					InputQueue.Clear();
					currentInputToken = DEFAULT_INPUTBOX_TEXT;
					InputBox.text = DEFAULT_INPUTBOX_TEXT;
					equalPressed = false;
				}

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

	//converts input from infix notation to RPN
	private void ToRPN() {
		foreach (string token in InputQueue) {

			if (IsOperator(token)) {
				while ((SymbolStack.Count > 0) && IsOperator(SymbolStack.Peek())) {
					//since all supported operators are left associative, i'm going to skip the associative check
					if (ComparePrecedence(token, SymbolStack.Peek()) <= 0) {
						OutputQueue.Enqueue(SymbolStack.Pop());
						continue;
					}
					break;
				}

				SymbolStack.Push(token);
			}
			else if (token == "(") {
				SymbolStack.Push(token);
			}
			else if (token == ")") {
				while (SymbolStack.Count > 0 && (SymbolStack.Peek() != "(")) {
					OutputQueue.Enqueue(SymbolStack.Pop());
				}

				SymbolStack.Pop();
			}
			else {
				OutputQueue.Enqueue(token);
			}
		}

		while (SymbolStack.Count > 0) {
			OutputQueue.Enqueue(SymbolStack.Pop());
		}
	}

	//my input parser is unfortunately adding a blank token before every bracket
	//if i have more time, i'd go back and clean it up. for now, i'll settle for
	//simply removing them after the fact.
	private void RemoveBlanks() {
		Queue<string> tempQueue = new Queue<string> ();

		//checks each token in InputQueue. If the token is not blank, add to temp
		foreach (string token in InputQueue) {
			if (token.Length > 0) {
				tempQueue.Enqueue(token);
			}
		}

		//replace InputQueue with temp
		InputQueue.Clear ();
		InputQueue = tempQueue;
	}

	//this checks whether the input queue is valid or not. ie, it checks to see if
	//the input is in proper infix notation or not.
	private bool IsValidInputQueue() {
		//this keeps track of brackets
		int currentBracketLevel = 0;
		//temp list for storing a bracket-less set of tokens for further evaluation
		List<string> tempList = new List<string> ();

		foreach (string token in InputQueue) {
			if (token == "(") {
				currentBracketLevel++;
			}
			else if (token == ")") {
				currentBracketLevel--;
			}
			//otherwise, build tempList
			else {
				tempList.Add(token);
			}
		}

		//evaluate brackets. if the level is anything other than 0, we've got a mismatch
		if (currentBracketLevel != 0) {
			return false;
		}

		if ((IsOperator(tempList[0])) || (IsOperator(tempList[tempList.Count-1]))) {
			return false;
		}

		for (int i = 1; i < tempList.Count-1; i++) {
			if ((IsOperator(tempList[i])) && 
			    ((IsOperator(tempList[i-1])) || (IsOperator(tempList[i+1])))) {
				return false;
			}
		}

		return true;
	}

	private double Calculate() {
		Stack<string> calcStack = new Stack<string> ();

		foreach (string token in OutputQueue) {

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

	//helper functions
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

	private int ComparePrecedence(string token, string top) {
		int left = 0;
		int right = 0;

		if (token == "*" || token == "/" || token == "%") {
			left = 3;
		}
		else {
			left = 2;
		}

		if (top == "*" || top == "/" || top == "%") {
			right = 3;
		}
		else {
			right = 2;
		}

		return left - right;
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
