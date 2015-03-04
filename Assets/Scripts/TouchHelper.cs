using UnityEngine;
using System.Collections;

public class TouchHelper : MonoBehaviour
{
	string touchPhaseString;
	GUIStyle textStyle = new GUIStyle();

	// Use this for initialization
	void Start()
	{
		textStyle.fontSize = Screen.height / 20;
		textStyle.normal.textColor = Color.white;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.touchCount > 0)
		{
			Touch t = Input.GetTouch(0);
			touchPhaseString = t.phase.ToString();
		}
		else
		{
			touchPhaseString = "";
		}
	}

	void OnGUI()
	{
		if (touchPhaseString != "")
		{
			GUILayout.Label(touchPhaseString, textStyle);
		}
	}
}
