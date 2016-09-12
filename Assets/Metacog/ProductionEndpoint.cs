﻿#if !UNITY_WEBGL
using System;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using System.IO;
using UnityEngine; 
using System.Text; 
using System.Collections.Generic;

namespace MetacogSDK
{
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

		public void init(){
			mc.api.initKinesis (new API.apiResponse(this.initKinesisResponse));
		}

		/* expected:
		 */ 
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

				//Debug.Log ("sending to kinesis: disabled"); 
				Debug.Log (batch);
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
				//foreach (PutRecordsResultEntry entry in responseObject.Response.Records){
				//}
				busy = false; 
			}, null);
		}

		/*
		 * pick up a batch from the storage and send to kinesis.
		 * if no batches available, try to flush the eventBuffer for remaining events. 
		 */ 
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
