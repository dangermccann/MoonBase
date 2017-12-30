using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MovementController : MonoBehaviour {
	public ControlPlane control;
	
	CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;
	CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;
	
	
	void Start () {
		control.AnalogInputEvent += HandleAnalogEvent;
	}

	void HandleAnalogEvent(object sender, AnalogInputChangedArgs args) {
		ControlPlane.AnalogEvent analog = args.Event;

		//Debug.Log(analog.AnalogInput.ToString() + ": 0x" + analog.AnalogValue.ToString());
		float val = analog.AnalogValue;
		val = (val - 511.5f) / 511.5f;
		if(Mathf.Abs(val) < 0.1f)
			val = 0;

		if(analog.AnalogInput == AnalogInputValues.JoystickX) {
			m_HorizontalVirtualAxis.Update(val);
		}

		if(analog.AnalogInput == AnalogInputValues.JoystickY) {
			m_VerticalVirtualAxis.Update(val);
		}
	}

	void OnEnable()
	{
		m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis("Mouse X", false);
		m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis("Vertical", false);
		CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
		CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
	}

	void OnDisable() {
		m_HorizontalVirtualAxis.Remove();
		m_VerticalVirtualAxis.Remove();
	}

	

}
