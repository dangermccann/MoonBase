

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;



/*
This script is used to read all the data coming from the device. For instance,
If arduino send ->
								{"1",
								"2",
								"3",}
readQueue() will return ->
								"1", for the first call
								"2", for the second call
								"3", for the thirst call

This is the perfect script for integration that need to avoid data loose.
If you need speed and low latency take a look to wrmhlReadLatest.
*/

public class wrmhlRead : MonoBehaviour {

	wrmhl myDevice = new wrmhl(); // wrmhl is the bridge beetwen your computer and hardware.

	[Tooltip("SerialPort of your device.")]
	public string portName = "COM8";

	[Tooltip("Baudrate")]
	public int baudRate = 250000;


	[Tooltip("Timeout")]
	public int ReadTimeout = 20;

	[Tooltip("QueueLenght")]
	public int QueueLenght = 1;

	CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis;
	CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis;

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

	void Start () {
		myDevice.set (portName, baudRate, ReadTimeout, QueueLenght); // This method set the communication with the following vars;
		//                              Serial Port, Baud Rates, Read Timeout and QueueLenght.
		myDevice.connect (); // This method open the Serial communication with the vars previously given.
	}

	// Update is called once per frame
	void Update () {
		
		//print ( ); // myDevice.read() return the data coming from the device using thread.
		string str = myDevice.readQueue();
		
		while(str != null) {
		//Debug.Log(str);

		

			string[] splits = str.Split(':');
			if(splits.Length == 2) {
				float val = float.Parse(splits[1]);
				val = (val - 512f) / 512f;

				if(splits[0] == "X") {
					val *= -1;
					m_HorizontalVirtualAxis.Update(val);
				}
				else if(splits[0] == "Y") {
					m_VerticalVirtualAxis.Update(val);
				}
			}

			str = myDevice.readQueue();

		}
		

		//Debug.Log(val);

		
	}

	void OnApplicationQuit() { // close the Thread and Serial Port
		myDevice.close();
	}
}
