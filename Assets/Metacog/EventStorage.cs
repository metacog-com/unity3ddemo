#if !UNITY_WEBGL
using System;
using UnityEngine; 
using System.Collections.Generic;

namespace MetacogSDK
{
	/// <summary>
	/// Keeps an in-memory buffer of event batches.
	/// </summary>
	public class EventStorage
	{
		private Queue<string> batches;

		public EventStorage ()
		{
			batches = new Queue<string> ();
		}

		/// <summary>
		/// Determines whether this instance is empty.
		/// </summary>
		/// <returns><c>true</c> if this instance is empty; otherwise, <c>false</c>.</returns>
		public bool IsEmpty(){
			return batches.Count == 0; 
		}

		/// <summary>
		/// Add the specified json_batch.
		/// </summary>
		/// <param name="json_batch">Json batch.</param>
		public void Add(string json_batch){
			batches.Enqueue (json_batch);
		}

		/// <summary>
		/// removes the oldest events batch in the queue.
		/// </summary>
		/// <returns>batch as json string, null if the storage is empty.</returns>
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
