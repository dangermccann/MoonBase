using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour {

	public ControlPlane control;
	public GameObject inputContainer;

	void Start() {
		control.AnalogInputEvent += HandleAnalogEvent;
		control.DigitalInputEvent += HandleDigitalEvent;


		//IEnumerator coroutine = WaitAndFlash(1.0f);
        //StartCoroutine(coroutine);

	}

	private IEnumerator WaitAndFlash(float waitTime)
    {
        while (true)
        {
			LedOn(1);
            yield return new WaitForSeconds(waitTime);
			LedOff(1);
            yield return new WaitForSeconds(waitTime);
        }
    }

	void HandleAnalogEvent(object sender, AnalogInputChangedArgs args) {

	}

	void HandleDigitalEvent(object sender, DigitalInputChangedArgs args) {
		Transform t = inputContainer.transform.GetChild((int) args.Event.DigitalInput);
		Image img = t.GetComponentInChildren<Image>();
		if(args.Event.value == ControlPlane.On)
			img.color = Color.red;
		else
			img.color = Color.gray;
	}

	public void LedOn(int idx) {
		ControlPlane.Command cmd = new ControlPlane.Command(ControlPlane.CommandLedOn, (byte) idx);
		control.SendCommand(cmd);
	}

	public void LedOff(int idx) {
		ControlPlane.Command cmd = new ControlPlane.Command(ControlPlane.CommandLedOff, (byte) idx);
		control.SendCommand(cmd);
	}
}
