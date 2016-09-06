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
							Debug.Log(string.Format("Successfully put record with sequence number '{0}'.", responseObject.Response.SequenceNumber));
						}
						else
						{
							Debug.Log(responseObject.Exception);
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
						Debug.Log("error: " + entry.ErrorCode + " , " + entry.ErrorMessage); 
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
				Debug.Log ("busy! ignore");
				return; 
			}
			busy = true;

			if (eventStorage.IsEmpty()) {
				if (eventBuffer.counter == 0) {
					Debug.Log ("no events. skip");
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
				Debug.Log ("batch.Length " + batch.Length);
				streamWriter.Write(batch);

				//Debug.Log ("1: " + memoryStream.Length); 
				streamWriter.Flush ();
				//Debug.Log ("2: " + memoryStream.Length); 
				//streamWriter.Close();
					
				//Debug.Log ("3: " + memoryStream.Length); 

				PutRecordsRequestEntry entry = new PutRecordsRequestEntry();
				entry.PartitionKey = "partitionKey";
				entry.Data = memoryStream; 
				//Debug.Log ("entry.data.length:"+entry.Data.Length);
				entries.Add(entry);
				batches.Add(batch);

				//Debug.Log ("4: " + memoryStream.Length); 
				Debug.Log(batch);
			} 
			Debug.Log ("sending batches " + batches.Count);
			sendBatches (batches, entries, eventStorage);
		}

  }
}

