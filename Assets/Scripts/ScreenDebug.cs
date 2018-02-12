using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenDebug : MonoBehaviour {

	public static Text debugText;

	void Start () {
		debugText = this.gameObject.GetComponent<Text>();
	}

	public static void DebugToScreen(string msg) {
		debugText.text = msg;
	}
}
