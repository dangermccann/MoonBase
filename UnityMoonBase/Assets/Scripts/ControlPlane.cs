using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AnalogInputChangedArgs : EventArgs {  
    public AnalogInputChangedArgs(ControlPlane.AnalogEvent _evt) {  
        this.evt =_evt;
    }  
    private ControlPlane.AnalogEvent evt;  
    public ControlPlane.AnalogEvent Event { get { return evt; } }   
}  

public class DigitalInputChangedArgs : EventArgs {  
    public DigitalInputChangedArgs(ControlPlane.InputEvent _evt) {  
        this.evt =_evt;
    }  
    private ControlPlane.InputEvent evt;  
    public ControlPlane.InputEvent Event { get { return evt; } }   
}  


public class ControlPlane : MonoBehaviour {
	public const byte CommandStart1     = 0xF7;
	public const byte CommandStart2     = 0x82;
	public const byte CommandPad        = 0xFF;
	public const int  CommandLength     = 10;
	public const int  CommandOffsetInstruction = 3;
	public const int  CommandOffsetParameter   = 4;

	public const byte CommandLedOff     = 0;
	public const byte CommandLedOn      = 1;
	public const byte CommandLedOffFor  = 2;
	public const byte CommandLedOnFor   = 3;
	public const byte CommandLedBlink   = 4;
	public const byte CommandSetAlpha   = 5;

	public const byte EventStart1       = 0x7F;
	public const byte EventStart2       = 0x28;
	public const byte EventPad          = 0xFF;
	public const int  EventLength       = 8;

	public const int  EventOffsetType   = 3;
	public const int  EventOffsetParameter = 4;

	public const byte EventInputChanged = 0;
	public const byte EventAnalogValue  = 1;

	public const byte JoystickX         = 0xA0;
	public const byte JoystickY         = 0xA1;

	public const byte Off 				= 0;
	public const byte On  				= 1;

	public class Command {
		byte[] bytes = new byte[CommandLength];

		public Command(byte instruction, byte index) {
			bytes[0] = CommandStart1;
			bytes[1] = CommandStart2;
			bytes[CommandOffsetInstruction] = instruction;
			bytes[CommandOffsetParameter] = index;
			for(int i = CommandOffsetParameter + 1; i < CommandLength; i++) {
				bytes[i] = CommandPad;
			}
		}

		public void AppendParameter(int index) {
			bytes[CommandOffsetParameter + 1] = (byte) ((index >> 8) & 0xFF);
			bytes[CommandOffsetParameter + 2] = (byte) (index & 0xFF);
		}

		public void AppendParameter(string str) {
			for(int i = 0; i < Mathf.Min(str.Length, 4); i++) {
				bytes[CommandOffsetParameter + i + 1] = (byte) str[i];
			}
		}

		public byte[] getBytes() {
			return bytes;
		}
	}

	public enum EventTypes {
		InputChanged,
		AnalogValue,
		Undefined
	}

	public enum AnalogInputs {
		JoystickX, 
		JoystickY,
		Undefined
	}

	public enum DigitalInputs {
		Button0 = 0, 
		Button1 = 1,
		Button2 = 2,
		Button3 = 3,
		// etc 
		Undefined = 0xFF
	}

	public abstract class Event {
		public EventTypes Type = EventTypes.Undefined;

		public static Event parse(byte[] bytes) {

			Event evt = null;
			byte eventType = bytes[EventOffsetType];

			switch(eventType) {
				case EventInputChanged:
					evt = new InputEvent(bytes);
					break;

				case EventAnalogValue:
					evt = new AnalogEvent(bytes);
					break;

				default:
					Debug.LogWarning("Invalid event type: " + eventType.ToString("X"));
					break;
			}

			return evt;
		}

		public Event(byte[] bytes) { }
	}

	public class AnalogEvent : Event {
		public AnalogInputs AnalogInput = AnalogInputs.Undefined;
		public int AnalogValue = -1;

		public AnalogEvent(byte[] bytes) : base(bytes) {
			Type = EventTypes.AnalogValue;

			byte device = bytes[EventOffsetParameter];
			switch(device) {
				case JoystickX:
					AnalogInput = AnalogInputs.JoystickX;
					break;

				case JoystickY:
					AnalogInput = AnalogInputs.JoystickY;
					break;

				default:
					Debug.LogWarning("Unknown analog device: " + device.ToString("X"));
					break;

			}

			AnalogValue  =  bytes[EventOffsetParameter + 2];
			AnalogValue |= (bytes[EventOffsetParameter + 1] << 8);
		}
	}

	public class InputEvent : Event {
		public DigitalInputs DigitalInput = DigitalInputs.Undefined;
		public byte value = 0xFF;

		public InputEvent(byte[] bytes) : base(bytes) {
			Type = EventTypes.InputChanged;

			switch(bytes[EventOffsetParameter]) {
				case 0:
					DigitalInput = DigitalInputs.Button0;
					break;

				case 1:
					DigitalInput = DigitalInputs.Button1;
					break;
				case 2:
					DigitalInput = DigitalInputs.Button2;
					break;
				case 3:
					DigitalInput = DigitalInputs.Button3;
					break;
			}

			value = bytes[EventOffsetParameter + 1];

			//Debug.Log("Digital: " + ByteArrayToString(bytes));
		}
	}

	public delegate void AnalogInputEventHandler(object sender, AnalogInputChangedArgs a);  
	public delegate void DigitalInputEventHandler(object sender, DigitalInputChangedArgs a);  

	public event AnalogInputEventHandler AnalogInputEvent;
	public event DigitalInputEventHandler DigitalInputEvent;  


	Queue<byte> incoming = new Queue<byte>();

	wrmhl myDevice = new wrmhl(); 

	[Tooltip("SerialPort of your device.")]
	public string portName = "/dev/tty.SLAB_USBtoUART";

	[Tooltip("Baudrate")]
	public int baudRate = 115200;


	[Tooltip("Timeout")]
	public int ReadTimeout = 20;

	[Tooltip("QueueLength")]
	public int QueueLength = 10;



	public void Enqueue(byte[] bytes) {
		foreach(byte b in bytes) {
			incoming.Enqueue(b);
		}
	}
	
	public Event ProcessQueue() {
		if(incoming.Count == 0)
			return null;

		byte start = incoming.Peek();
		while(start != EventStart1) {
			Debug.LogWarning("Got invalid start " + start.ToString("X")); 
			incoming.Dequeue();
			
			if(incoming.Count == 0)
				return null;

			start = incoming.Peek();
		}

		if(incoming.Count < EventLength)
			return null;

		byte[] eventBuffer = new byte[EventLength];
		for(int i = 0; i < EventLength; i++) {
			eventBuffer[i] = incoming.Dequeue();

			// validate magic number
			if(i == 0 && eventBuffer[0] != EventStart1)
				return null;

			if(i == 1 && eventBuffer[1] != EventStart2)
				return null;
		}

		return Event.parse(eventBuffer);
	}

	public void SendCommand(Command cmd) {
		myDevice.send(cmd.getBytes());
	}

	public static string ByteArrayToString(byte[] ba)
	{
		StringBuilder hex = new StringBuilder(ba.Length * 2);
		foreach (byte b in ba)
			hex.AppendFormat("{0:x2}", b);
		return hex.ToString();
	}


	void Start() {
		myDevice.set(portName, baudRate, ReadTimeout, QueueLength); // This method set the communication with the following vars;
		myDevice.connect(); // This method open the Serial communication with the vars previously given.
	}

	void Update() {
		byte[] bytes = myDevice.readQueue();
		if(bytes != null && bytes.Length > 0) {
			Enqueue(bytes);
		}

		Event evt = ProcessQueue();
		if(evt != null) {

			if(evt.Type == EventTypes.AnalogValue) {
				AnalogEvent analog = evt as AnalogEvent;

				if(AnalogInputEvent != null)
					AnalogInputEvent.Invoke(this, new AnalogInputChangedArgs(analog));
			}

			if(evt.Type == EventTypes.InputChanged) {
				InputEvent input = evt as InputEvent;
				Debug.Log("Digital: " + input.DigitalInput.ToString() + " " + input.value);

				if(DigitalInputEvent != null)
					DigitalInputEvent.Invoke(this, new DigitalInputChangedArgs(input));
			}


			
		}
	}

	void OnApplicationQuit() { // close the Thread and Serial Port
		myDevice.close();
	}
}
