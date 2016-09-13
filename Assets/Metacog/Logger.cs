#if !UNITY_WEBGL
using System;
using UnityEngine;

namespace MetacogSDK
{

	enum Status{
		IDLE,
		STARTING,
		RUNNING,
		STOPPING
	}

	/// <summary>
	/// Continous pushing of events to the Metacog platform.
	/// </summary>
	/// <description>
	/// This class receive events and store them into batches, 
	/// that are sent periodically to the Metacog platform. 
	/// </description>
	public class Logger
	{
		/// <summary>
		/// Minimum wait time between polling cycles.
		/// </summary>
		private const float TICK_TIME = 2.0f; 

		private Metacog mc; 
		private ProductionEndpoint endpoint;
		private EventBuffer eventBuffer;
		private EventStorage eventStorage;

		private Status status;
		private int index;
		private float elapsedTime;

		/// <summary>
		/// Constructor. initializes event buffer and production endpoint.  
		/// </summary>
		/// <param name="mc">Metacog instance.</param>
		public Logger(Metacog mc)
		{
			status = Status.IDLE;

			eventBuffer = new EventBuffer (mc);
			eventStorage = new EventStorage ();
			endpoint = new ProductionEndpoint (mc);
			endpoint.init ();

		}

		/// <summary>
		/// Store event for later sending to Metacog.
		/// </summary>
		/// A timestamp and a incremental index are added at queue-insertion time.<br>
		/// The event is serialized into a json string, then pushed into 
		/// the eventBuffer (memory). if the buffer is full, it will
		/// flush into the eventStorage. 
		/// <param name="eventName">Event name.</param>
		/// <param name="data">Serializable object</param>
		/// <param name="eventType">Event type.</param>
		public void Send(string eventName, object data, EventType eventType){

			long timestamp = Metacog.ConvertToUnixTime (DateTime.Now);
			string json = "{\"event\":\"" + eventName 
				+ "\",\"timestamp\":"+ timestamp
				+ ",\"index\":"+ (index++)
				+ ", \"data\":" + JsonUtility.ToJson (data)
				+ ", \"type\": \"" + eventType + "\"}";
			if (!eventBuffer.Add (json)) {
				string batch = eventBuffer.flush ();
				eventStorage.Add (batch);
			}
		}

		/// <summary>
		/// reset timers and set running status.
		/// </summary>
		public void Start(){
			status = Status.RUNNING; 
			index = 0; 
			elapsedTime = 0.0f;
		}


		public void Stop(){
			
		}

		/// <summary>
		/// update timer and process the queue of events. 
		/// </summary>
		/// <param name="dt">Delta time in seconds</param>
		public void update(float dt){
			elapsedTime += dt; 
			if (elapsedTime > TICK_TIME) {
				endpoint.ProcessQueue (eventBuffer, eventStorage);
				elapsedTime = 0; 
			}
		}

	}
}
#endif
