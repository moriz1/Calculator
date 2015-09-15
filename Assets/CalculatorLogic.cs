using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CalculatorLogic : MonoBehaviour {

	public const string DefaultInputBoxText = "Input Your Stuff";
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
		currentInputToken = string.Empty;
	}

	public void OnButtonClicked(Text t) {
		if (t.text == "=") {

		}
		else {
			switch (t.text) {

			case "+":
			case "-":
			case "*":
			case "/":
			case "%":
				InputQueue.Enqueue(currentInputToken);
				InputQueue.Enqueue(t.text);
				currentInputToken = string.Empty;
				break;
			case "+/-":
				currentInputToken = (!(currentInputToken.StartsWith("-"))) 
					? ("-" + currentInputToken) : (currentInputToken.Substring(1));
				break;
			case "C":
				currentInputToken = string.Empty;
				InputQueue.Clear();
				InputBox.text = string.Empty;
				break;
			case "AC":
				currentInputToken = string.Empty;
				InputQueue.Clear();
				InputBox.text = string.Empty;
				HistoryBox.text = string.Empty;
				break;
			default:
				currentInputToken = currentInputToken + t.text;
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
