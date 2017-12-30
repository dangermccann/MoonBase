using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LedUI : MonoBehaviour {

	public int Index;
	public Color OnColor = new Color(1, 0, 0, 1f);
	public Color OffColor = new Color(0.5f, 0, 0, 1f); 

	Led led;
	Image image;

	
	void Start() {
		led = ControlPlane.Get().Leds[Index];
		image = GetComponent<Image>();
	}

	void Update() {
		if(led.State == ControlPlane.On) {
			image.color = OnColor;
		}
		else {
			image.color = OffColor;
		}
	}

	public void Clicked() {
		led.Toggle();
	}
	
	
}
