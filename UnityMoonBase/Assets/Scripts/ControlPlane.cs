using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

public enum AnalogInputValues {
	JoystickX = 0xA0, 
	JoystickY = 0xA1
}

public enum DigitalInputValues {
	Button0 = 0, 
	Button1 = 1,
	Button2 = 2,
	Button3 = 3
}

public class Led {
	ControlPlane control;

	public byte Index;
	public byte State = ControlPlane.Off;
	public bool Blinking = false;

	public Led(ControlPlane control, byte index) {
		this.control = control;
		this.Index = index;
	}

	public void On() {
		control.SendCommand(new ControlPlane.Command(ControlPlane.CommandLedOn, Index));
		State = ControlPlane.On;
		Blinking = false;
	}

	public void Off() {
		control.SendCommand(new ControlPlane.Command(ControlPlane.CommandLedOff, Index));
		State = ControlPlane.Off;
		Blinking = false;
	}

	public void StartBlinking(int duration) {
		ControlPlane.Command cmd = new ControlPlane.Command(ControlPlane.CommandLedBlink, Index);
		cmd.AppendParameter(duration);
		control.SendCommand(cmd);
		State = ControlPlane.On;
		Blinking = true;
	}

	public void OnFor(int duration) {
		ControlPlane.Command cmd = new ControlPlane.Command(ControlPlane.CommandLedOnFor, Index);
		cmd.AppendParameter(duration);
		control.SendCommand(cmd);
		State = ControlPlane.Off; // On temporarily
		Blinking = false;
	}

	public void OffFor(int duration) {
		ControlPlane.Command cmd = new ControlPlane.Command(ControlPlane.CommandLedOffFor, Index);
		cmd.AppendParameter(duration);
		control.SendCommand(cmd);
		State = ControlPlane.On; // Off temporarily
		Blinking = false;
	}

	public void Toggle() {
		if(State == ControlPlane.On)
			Off();
		else 
			On();
	}
}

public class Alpha {
	ControlPlane control;

	public byte Index;

	string text;
	public string Text {
		get{ return text; }
		set {
			text = TrimAndPad(value);

			ControlPlane.Command cmd = new ControlPlane.Command(ControlPlane.CommandSetAlpha, Index);
			cmd.AppendParameter(text);
			control.SendCommand(cmd);
		}
	}

	public Alpha(ControlPlane control, byte index) {
		this.control = control;
		this.Index = index;
	}

	string TrimAndPad(string str) {
		if(str == null)
			str = "";

		if(str.Length > 4)
			str = str.Substring(str.Length - 5);
		
		while(str.Length < 4) {
			str = " " + str;
		}

		return str;
	}
}

public class DigitalInput {
	public byte State;
	public DigitalInputValues Input;

	public DigitalInput(DigitalInputValues input) {
		this.Input = input;
	}

	public byte Index {
		get { return (byte) Input; }
	}
}

public class AnalogInput {
	public int Value;	// 0 to 1023
	public AnalogInputValues Input;

	public AnalogInput(AnalogInputValues input) {
		this.Input = input;
	}

	public byte Index {
		get { return (byte) Input; }
	}
}



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

	public const byte Off 				= 0;
	public const byte On  				= 1;

	public const int  LedCount			= 28;
	public const int  AlphaCount		= 1;

	public const string Tag				= "ControlPlane";

	public static ControlPlane Get() {
		return GameObject.FindGameObjectWithTag(ControlPlane.Tag).GetComponent<ControlPlane>();
	}

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
		public AnalogInputValues AnalogInput;
		public int AnalogValue = -1;

		public AnalogEvent(byte[] bytes) : base(bytes) {
			Type = EventTypes.AnalogValue;
			AnalogInput = (AnalogInputValues) bytes[EventOffsetParameter];
			AnalogValue  =  bytes[EventOffsetParameter + 2];
			AnalogValue |= (bytes[EventOffsetParameter + 1] << 8);
		}
	}

	public class InputEvent : Event {
		public DigitalInputValues DigitalInput;
		public byte State = 0xFF;

		public InputEvent(byte[] bytes) : base(bytes) {
			Type = EventTypes.InputChanged;
			DigitalInput = (DigitalInputValues) bytes[EventOffsetParameter];
			State = bytes[EventOffsetParameter + 1];
		}
	}

	public delegate void AnalogInputEventHandler(object sender, AnalogInputChangedArgs a);  
	public delegate void DigitalInputEventHandler(object sender, DigitalInputChangedArgs a);  

	public event AnalogInputEventHandler AnalogInputEvent;
	public event DigitalInputEventHandler DigitalInputEvent;  


	private Queue<byte> incoming = new Queue<byte>();
	private wrmhl myDevice = new wrmhl(); 

	[HideInInspector]
	public List<Led> Leds = new List<Led>();

	[HideInInspector]
	public List<Alpha> Alphas = new List<Alpha>();

	[HideInInspector]
	public Dictionary<DigitalInputValues, DigitalInput> DigitalInputs = new Dictionary<DigitalInputValues, DigitalInput>();

	[HideInInspector]
	public Dictionary<AnalogInputValues, AnalogInput> AnalogInputs = new Dictionary<AnalogInputValues, AnalogInput>();
	

	[Tooltip("SerialPort of your device.")]
	public string portName = "/dev/tty.SLAB_USBtoUART";

	[Tooltip("Baudrate")]
	public int baudRate = 115200;


	[Tooltip("Timeout")]
	public int ReadTimeout = 20;

	[Tooltip("QueueLength")]
	public int QueueLength = 10;

	public void SendCommand(Command cmd) {
		myDevice.send(cmd.getBytes());
	}
	void Awake() {
		for(int i = 0; i < LedCount; i++) {
			Led led = new Led(this, (byte) i);
			Leds.Add(led);
		}

		for(int i = 0; i < AlphaCount; i++) {
			Alpha alpha = new Alpha(this, (byte) i);
			Alphas.Add(alpha);
		}

		IEnumerable<DigitalInputValues> digitalValues = Enum.GetValues(typeof(DigitalInputValues)).Cast<DigitalInputValues>();
		foreach(DigitalInputValues di in digitalValues) {
			DigitalInputs.Add(di, new DigitalInput(di));
		}

		IEnumerable<AnalogInputValues> analogValues = Enum.GetValues(typeof(AnalogInputValues)).Cast<AnalogInputValues>();
		foreach(AnalogInputValues ai in analogValues) {
			AnalogInputs.Add(ai, new AnalogInput(ai));
		}
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

				AnalogInputs[analog.AnalogInput].Value = analog.AnalogValue;

				if(AnalogInputEvent != null)
					AnalogInputEvent.Invoke(this, new AnalogInputChangedArgs(analog));
			}

			if(evt.Type == EventTypes.InputChanged) {
				InputEvent input = evt as InputEvent;
				Debug.Log("Digital: " + input.DigitalInput.ToString() + " " + input.State);

				DigitalInputs[input.DigitalInput].State = input.State;

				if(DigitalInputEvent != null)
					DigitalInputEvent.Invoke(this, new DigitalInputChangedArgs(input));
			}


			
		}
	}

	void Enqueue(byte[] bytes) {
		foreach(byte b in bytes) {
			incoming.Enqueue(b);
		}
	}
	
	Event ProcessQueue() {
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

	void OnApplicationQuit() { // close the Thread and Serial Port
		myDevice.close();
	}



	public static string ByteArrayToString(byte[] ba)
	{
		StringBuilder hex = new StringBuilder(ba.Length * 2);
		foreach (byte b in ba)
			hex.AppendFormat("{0:x2}", b);
		return hex.ToString();
	}
}
