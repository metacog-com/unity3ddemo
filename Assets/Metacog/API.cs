#if !UNITY_WEBGL
using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Networking;

namespace MetacogSDK
{

	/// <summary>
	/// Wrapper for network calls to the Metacog RESTFul API.    
	/// </summary>
	/// <description>
	/// All the services of the Metacog Platform are available through its RESTful API.<br>
	/// Check the API documentation at <a href="https://developer.metacog.com/sandbox">https://developer.metacog.com/sandbox</a>
	/// </description>
	public class API
	{

		/// <summary>
		/// handle the response for all RESTFul calls
		/// </summary>
		public delegate void apiResponse(long resCode, string resTxt);

		private Metacog mc;

		/// <param name="mc">Reference to the Metacog object</param>
		public API(Metacog mc){
			this.mc = mc; 
		}

		/// <summary>
		/// Obtains credentials for the digesting endpoint.
		/// </summary>
		/// <summary>
		/// Logging to Metacog is done through the Kinesis AWS service.<br>
		/// Provisional credentials for this services should be obtained through 
		/// the /access/ set of endpoints at Metacog's API. <br>
		/// </summary>
		/// <param name="resp">callback</param>
		public void initKinesis(apiResponse resp){
			string url = mc.apiEndpoint; 
			//byte[] bytes = System.Text.Encoding.UTF8.GetBytes(postdata);
			bool hasToken = mc.LearnerToken != null && mc.LearnerToken != "";
			url += hasToken ? "/access/logger" : "/access/kinesis";
			UnityWebRequest request = UnityWebRequest.Get(url);

			request.SetRequestHeader("Content-Type", "application/json");
			if (hasToken) {
				request.SetRequestHeader("learner_token", mc.LearnerToken);
				request.SetRequestHeader("learner_id", mc.LearnerID);
			} else {
				request.SetRequestHeader ("application_id", mc.ApplicationID);
			}
			request.SetRequestHeader ("publisher_id", mc.PublisherID);
			request.downloadHandler = new DownloadHandlerBuffer ();
			executeRequest(request, resp);
		}

		private void executeRequest(UnityWebRequest request, apiResponse resp){
			mc.StartCoroutine(WaitForRequest(request, resp));
		}

		private IEnumerator WaitForRequest(UnityWebRequest request, apiResponse resp){
			yield return request.Send(); 
			resp (request.responseCode, request.downloadHandler.text);
		}
	

	}
}
#endif
