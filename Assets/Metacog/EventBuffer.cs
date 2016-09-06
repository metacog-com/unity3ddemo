using System;
using System.Text; 
using UnityEngine;


namespace MetacogSDK
{
	/**
	 * a in-memory buffer to assemble batches of events.
	 * once a batch hits it max size, it won't accept more data
	 * until a call to flush is done.
	 */ 
	public class EventBuffer
	{
		/*max number of characters allowed before requiring a flush*/
		private const int MAX_LENGTH = 1000;
		public int counter{ get; private set;}
		private Metacog mc; 

		StringBuilder buffer;

		public EventBuffer (Metacog mc)
		{
			this.mc = mc; 
			buffer = new StringBuilder (); 
			flush ();
		}

		/*
		 * add a json event to memory.
		 * returns true if can accept more events,
		 * and false if not. 
		 * invoking Add after it returned false will
		 * throw exception.
		 * use flush to reset the buffer and start
		 * accepting new events.
		*/
		public bool Add(string json) {
			if(buffer.Length > MAX_LENGTH) 
				throw new MetacogException("EventBuffer_overflow");
			if (buffer.Length > 0) {
				buffer.Append (",");
			}
			counter++;
			buffer.Append(json);
			return buffer.Length < MAX_LENGTH;
		}

		/**
		 * assemble a json batch of events with the current data
		 * and return it. resets the buffer so it will acept 
		 * events again. 
		 */ 
		public string flush(){
			string content = buffer.ToString();
			buffer = new StringBuilder();
			counter = 0; 

			StringBuilder data = new StringBuilder ();
			data.Append ("{\"session\":");
			data.Append (mc.sessionJson);
			data.Append(", \"events\":[");
			data.Append (content);
			data.Append("]}");
			return data.ToString ();
		}
			

	}
}

