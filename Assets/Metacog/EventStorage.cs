#if !UNITY_WEBGL
using System;
using UnityEngine; 
using System.Collections.Generic;

namespace MetacogSDK
{
	public class EventStorage
	{
		private Queue<string> batches;

		public EventStorage ()
		{
			batches = new Queue<string> ();
		}

		public bool IsEmpty(){
			return batches.Count == 0; 
		}

		public void Add(string json_batch){
			batches.Enqueue (json_batch);
		}

		/**
		 * retrieve the oldest added batch.
		 * null if container is empty.
		 */ 
		public string Pop(){
			string batch = null; 
			if (batches.Count > 0) {
				batch = batches.Dequeue ();
			}
			return batch;
		}
	}
}
#endif
