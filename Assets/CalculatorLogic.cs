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

	private Stack<string> ParserStack;
	private Queue<string> InputQueue;

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

		ParserStack = new Stack<string> ();
		InputQueue = new Queue<string> ();
		currentInputToken = DefaultInputBoxText;
	}

	public void OnButtonClicked(Text t) {
		if (t.text == "EQUAL") {

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

	private void Calculate() {
		string token;

		while (InputQueue.Count > 0) {
			token = InputQueue.Dequeue();

			switch (token) {

			case "+":
			case "-":
			case "*":
			case "/":
			case "%":
				break;
			default:
				ParserStack.Push(token);
				break;
			}
		}
	}
}
