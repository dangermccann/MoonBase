using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchUI : MonoBehaviour {

	public Color ActivatedColor = new Color(0.15f, 1, 0, 1);
	public Color DeactivatedColor = new Color(0.3f, 0.3f, 0.3f, 1);
	public Color LedOnColor = new Color(1, 0, 0, 1);
	public Color LedOffColor = new Color(0.6f, 0.6f, 0.6f, 1);

	public DigitalInputValues InputIndex;
	public int LedIndex = -1;

	Led led = null;
	DigitalInput input;
	Image image, knobImage;

	void Start () {
		image = GetComponent<Image>();
		knobImage = transform.GetChild(0).gameObject.GetComponent<Image>();

		ControlPlane control = ControlPlane.Get();
		
		input = control.DigitalInputs[InputIndex];
		
		if(LedIndex >= 0) {
			led = control.Leds[LedIndex];
		}
		else {
			knobImage.color = LedOffColor;
		}
	}
	
	void Update () {
		if(led != null) {
			if(led.State == ControlPlane.On)
				knobImage.color = LedOnColor;
			else
				knobImage.color = LedOffColor;
		}

		if(input.State == ControlPlane.On) {
			image.color = ActivatedColor;
			knobImage.transform.localPosition = new Vector3(21, knobImage.transform.localPosition.y, knobImage.transform.localPosition.z);
		}
		else {
			image.color = DeactivatedColor;
			knobImage.transform.localPosition = new Vector3(-21, knobImage.transform.localPosition.y, knobImage.transform.localPosition.z);
		}
		
	}

	public void KnobClicked() {
		if(led != null)
			led.Toggle();
	}
}
