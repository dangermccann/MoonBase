using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DebugUI : MonoBehaviour {

	public ControlPlane control;
	public GameObject inputContainer;
	public GameObject joystickX, joystickY;

	Text joystickXText, joystickYText;

	GameObject panel;

	void Start() {
		control.AnalogInputEvent += HandleAnalogEvent;
		control.DigitalInputEvent += HandleDigitalEvent;

		panel = transform.GetChild(0).gameObject;
		panel.SetActive(false);

		joystickXText = joystickX.GetComponent<Text>();
		joystickYText = joystickY.GetComponent<Text>();

		//IEnumerator coroutine = WaitAndFlash(1.0f);
        //StartCoroutine(coroutine);

	}

	void Update() {
		if(Input.GetKeyDown(KeyCode.Tab)) {
			panel.SetActive(!panel.activeSelf);
		}
	}

	private IEnumerator WaitAndFlash(float waitTime)
    {
        while (true)
        {
			LedOn(2);
            yield return new WaitForSeconds(waitTime);
			LedOff(2);
            yield return new WaitForSeconds(waitTime);
        }
    }

	void HandleAnalogEvent(object sender, AnalogInputChangedArgs args) {
		if(args.Event.AnalogInput == AnalogInputValues.JoystickX)
			joystickXText.text = args.Event.AnalogValue.ToString();

		if(args.Event.AnalogInput == AnalogInputValues.JoystickY)
			joystickYText.text = args.Event.AnalogValue.ToString();
	}

	void HandleDigitalEvent(object sender, DigitalInputChangedArgs args) {

	}

	public void LedOn(int idx) {
		control.Leds[idx].On();
	}

	public void LedOff(int idx) {
		control.Leds[idx].Off();
	}

	public void SetAlpha(string str) {
		Debug.Log(str);
		control.Alphas[0].Text = str;
	}

	public void Close() {
		panel.SetActive(false);
	}
}
