#if !UNITY_WEBGL
using System;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using System.IO;
using UnityEngine; 
using System.Text; 
using System.Collections.Generic;

namespace MetacogSDK
{
	/// <summary>
	/// Send batches of events to Metacog's backend.
	/// </summary>
	public class ProductionEndpoint
	{
		private Metacog mc;
		private AmazonKinesisClient kClient;
		private string streamName;
		private bool busy;

		private const int MAX_BATCHES_PER_REQUEST = 20; 


		public ProductionEndpoint (Metacog mc)
		{
			this.mc = mc; 
			this.busy = false; 
		}

		/// <summary>
		/// Initializes the connection to AWS Kinesis.
		/// </summary>
		public void init(){
			mc.api.initKinesis (new API.apiResponse(this.initKinesisResponse));
		}
		
    /// <summary>
		/// Used as delegate in initKinesis call.
		/// </summary>
		/// <returns>The kinesis response.</returns>
		/// <param name="resCode">Res code.</param>
		/// <param name="resTxt">Res text.</param>
		public void initKinesisResponse(long resCode, string resTxt){
			if (resCode != 200) {
				Debug.LogError ("initKinesis failed with code: "+resCode);
				return; 	
			}
			KinesisResponse kres = JsonUtility.FromJson<KinesisResponse>(resTxt);
			streamName = kres.kinesisStreamName;
			kClient = kres.toAmazonKinesisClient();
			busy = false; 
		}

		/// <summary>
		/// Sends a batch to Metacog's backend.
		/// </summary>
		/// <description>
		/// If sending the batch fails, it will be stored into the passed eventStorage for retry later. 
		/// </description>
		/// <param name="batch">Batch of events, as stringified JSON object.</param>
		/// <param name="eventStorage">The storage where failed batches should be added.</param>
		private void sendBatch(string batch, EventStorage eventStorage){
			using (var memoryStream = new MemoryStream())
			using (var streamWriter = new StreamWriter(memoryStream))
			{
				streamWriter.Write(batch);
				PutRecordRequest req = new PutRecordRequest {
					Data = memoryStream,
					PartitionKey = "partitionKey",
					StreamName = streamName
				};
				kClient.PutRecordAsync(req,
					(responseObject) =>
					{
						if (responseObject.Exception == null)
						{
						}
						else
						{
							//inject the event again for further attempts of sending..
							eventStorage.Add(batch);
						}
						busy = false; 
					}, null
				);
			}
		}

		/// <summary>
		/// try to send a list of batches to Metacog's backend.
		/// </summary>
		/// <param name="batches">list of batches, each one is a stringified JSON object. </param>
		/// <param name="entries">list of requestEntries, to track success of failure of each item</param>
		/// <param name="eventStorage">Storage to add failed events, for later retry.</param>
		private void sendBatches(List<string> batches, List<PutRecordsRequestEntry> entries, EventStorage eventStorage){
			PutRecordsRequest req = new PutRecordsRequest ();
			req.Records = entries;
			req.StreamName = streamName;
			kClient.PutRecordsAsync(req, (responseObject) => {
				for(int i=0; i< responseObject.Response.Records.Count; ++i){
					PutRecordsResultEntry entry = responseObject.Response.Records[i];
					if(entry.ErrorCode != null){
						eventStorage.Add(batches[i]);
					}	
				}
				busy = false; 
			}, null);
		}

		/// <summary>
		/// Implements a cycle of queue retrieval and network sending.
		/// </summary>
		/// <description>
		/// pick up a batch from the storage and send to kinesis.
		///if no batches available, try to flush the eventBuffer for remaining events. 
		/// </description>
		/// <param name="eventBuffer">In memory buffer. if the storage is empty, it will try to flush pending events there.</param>
		/// <param name="eventStorage">Storage to retrieve events from, and reinsert failed events</param>
		public void ProcessQueue(EventBuffer eventBuffer, EventStorage eventStorage){
			if (busy) {
				return; 
			}
			busy = true;
			if (eventStorage.IsEmpty()) {
				if (eventBuffer.counter == 0) {
					busy = false;
					return;
				} else {
					eventStorage.Add(eventBuffer.flush ());
				}
			}
			List<string> batches = new List<string> ();
			List<PutRecordsRequestEntry> entries = new List<PutRecordsRequestEntry> (); 
			int count = 0;
			string batch = null;
			while((batch= eventStorage.Pop()) != null && count++ < MAX_BATCHES_PER_REQUEST){
				MemoryStream memoryStream = new MemoryStream();
				StreamWriter streamWriter = new StreamWriter(memoryStream);
				streamWriter.Write(batch);
				streamWriter.Flush ();
				PutRecordsRequestEntry entry = new PutRecordsRequestEntry();
				entry.PartitionKey = "partitionKey";
				entry.Data = memoryStream; 
				entries.Add(entry);
				batches.Add(batch);
			} 
			sendBatches (batches, entries, eventStorage);
		}

  }
}
#endif
