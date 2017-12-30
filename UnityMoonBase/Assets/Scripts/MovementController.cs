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
		val = (val - 512f) / 512f;

		if(analog.AnalogInput == ControlPlane.AnalogInputs.JoystickX) {
			m_HorizontalVirtualAxis.Update(val * -1);
		}

		if(analog.AnalogInput == ControlPlane.AnalogInputs.JoystickY) {
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
