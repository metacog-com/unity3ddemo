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

	public class Logger
	{
		private const float TICK_TIME = 2.0f; 

		private Metacog mc; 
		private ProductionEndpoint endpoint;
		private EventBuffer eventBuffer;
		private EventStorage eventStorage;

		private Status status;
		private int index;
		private float elapsedTime;


		public Logger(Metacog mc)
		{
			status = Status.IDLE;

			eventBuffer = new EventBuffer (mc);
			eventStorage = new EventStorage ();
			endpoint = new ProductionEndpoint (mc);
			endpoint.init ();

		}

		/*
		 * event is serialized into a json string, then pushed into 
		 * the eventBuffer (memory). if the buffer is full, it will
		 * flush into the eventStorage. 
		*/
		public void Send(string eventName, object data, EventType eventType){

			long timestamp = Metacog.ConvertToUnixTime (DateTime.Now);
			string json = "{\"event\":\"" + eventName 
				+ "\",\"timestamp\":"+ timestamp
				+ ",\"index\":"+ (index++)
				+ ", \"data\":" + JsonUtility.ToJson (data)
				+ ", \"type\": \"" + eventType + "\"}";
			//Debug.Log (json);
			if (!eventBuffer.Add (json)) {
				string batch = eventBuffer.flush ();
				eventStorage.Add (batch);
			}
		}


		public void Start(){
			status = Status.RUNNING; 
			index = 0; 
			elapsedTime = 0.0f;
		}


		public void Stop(){
			
		}

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
