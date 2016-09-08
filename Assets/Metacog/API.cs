#if !UNITY_WEBGL
using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Networking;

namespace MetacogSDK
{

	public class API
	{

		public delegate void apiResponse(long resCode, string resTxt);

		private Metacog mc;

		public API(Metacog mc){
			this.mc = mc; 
		}

		/**
		 * 
		 */ 
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

		/**
		 * starts an async http request using unity objects.
		 * it can only be done in the context of a GameObject, that is why
		 * we pass the Metacog reference 
		 */ 
		private void executeRequest(UnityWebRequest request, apiResponse resp){
			mc.StartCoroutine(WaitForRequest(request, resp));
		}

		private IEnumerator WaitForRequest(UnityWebRequest request, apiResponse resp){
			yield return request.Send(); 
			Debug.Log ("got an answer from the API!");
			//if (request.error == null) {
			Debug.Log ("data: " + request.downloadHandler.text);
			//} else {
			Debug.Log ("error: " + request.error);
			Debug.Log ("responseCode: " + request.responseCode);
			resp (request.responseCode, request.downloadHandler.text);
			//}
		}
	

	}
}
#endif
