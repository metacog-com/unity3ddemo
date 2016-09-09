using UnityEngine;
using System; 
using System.Collections;
#if !UNITY_WEBGL
using Amazon;
using UnityEngine.Networking;
#endif
using System.Text; 

namespace MetacogSDK {

	public enum EventType{
		MODEL,
		UI
	}

	public class Metacog: MonoBehaviour  {

		public string PublisherID;
		public string ApplicationID;
		public string WidgetID;
		public string LearnerID;
		public string SessionID;
		public string LearnerToken;

		#if !UNITY_WEBGL
		public string apiEndpoint{ get; private set;}

		public API api{ get; private set;}

		//static in order to access it from other G.O.'s
		private static Logger logger;

		//store a read-only json version of the current session,
		//to be added in each call to kinesis
		public string sessionJson{ get; private set;}  

		#endif


		/**
		 * append a event to the queue and eventually, send it to the Metacog platform.
		 * the developer is responsible of implement custom classes
		 * for each one of his events. those classes should be
		 * annoated as serializable. 
		 * @TODO: note that for playback, we will need to deserialize the data.
		 * in C#, the target type should be known before hand. 
		 * it means that we may need to provide some mechanism to register a 
		 * mapping between event-names and C# Classes.. 
		 */ 
		public static void Send(string eventName, object data, EventType eventType){
			#if UNITY_WEBGL
			string json = JsonUtility.ToJson (data);
			Application.ExternalEval("sendMetacog('"+eventName + "', '"+json+"', '"+eventType+"')");
			#else
			if(logger!=null)
				logger.Send (eventName, data, eventType);
			#endif
		}


		public void Start () {
			Debug.Log ("START HAD BEEN CALLED");
			#if UNITY_WEBGL
			StartJS();
			#else 
			StartAWS();
			#endif
		}

		#if UNITY_WEBGL
		private void StartJS(){
			Debug.Log ("startJS");
			Application.ExternalEval("initMetacog('"+buildSessionJson()+"');");
		}
		#else
		private void StartAWS(){
			UnityInitializer.AttachToGameObject(this.gameObject);
			apiEndpoint = //"http://localhost:3000"; 
				//"https://localhost:1337";
				"https://testapi.metacog.com";

			if (LearnerID.Length == 0) {
				LearnerID = "Learner_"+ DateTime.Now.ToString("hhmmss")+(new System.Random().Next());
			}
			if (SessionID.Length == 0) {
				SessionID = "Session_"+ DateTime.Now.ToString("hhmmss")+(new System.Random().Next());
			}

			sessionJson = buildSessionJson ();

			api = new API (this); 
			logger = new Logger(this);
		}
		#endif

		/*
		 * returns the following stringifyed json:
		    {
		    publisher_id: null,
		    application_id: null,
    		widget_id: null,
    		learner_id: null,
    		learner_token: null,
    		session_id: null,
    		auth_token: null
			}
		 */
		private string buildSessionJson(){


			StringBuilder buffer = new StringBuilder (); 
			buffer.Append ("{");
			buffer.Append ("\"publisher_id\":"); buffer.Append (addAttr(PublisherID));
			buffer.Append (",\"widget_id\":"); buffer.Append (addAttr(WidgetID));
			buffer.Append (",\"learner_id\":"); buffer.Append (addAttr(LearnerID));


			buffer.Append (",\"application_id\":"); buffer.Append (addAttr(ApplicationID));

			if (LearnerToken.Length > 0) {
				buffer.Append (",\"learner_token\":"); buffer.Append (addAttr(LearnerToken));
			}

			buffer.Append (",\"session_id\":"); buffer.Append (addAttr(SessionID));
			buffer.Append ("}");
			return buffer.ToString ();
		}

		private string addAttr(string val){
			if (val == null) {
				return "null";
			} else {
				return "\"" + val + "\"";
			}
		}


		/// source: http://www.fluxbytes.com/csharp/convert-datetime-to-unix-time-in-c/
		/// <summary>
		/// Convert a date time object to Unix time representation.
		/// </summary>
		/// <param name="datetime">The datetime object to convert to Unix time stamp.</param>
		/// <returns>Returns a numerical representation (Unix time) of the DateTime object.</returns>
		public static long ConvertToUnixTime(DateTime datetime)
		{
			DateTime sTime = new DateTime(1970, 1, 1,0,0,0,DateTimeKind.Utc);
			return (long)((datetime - sTime).TotalSeconds * 1000.0f);
		}

		/// source: http://www.fluxbytes.com/csharp/convert-datetime-to-unix-time-in-c/
		/// <summary>
		/// Convert Unix time value to a DateTime object.
		/// </summary>
		/// <param name="unixtime">The Unix time stamp you want to convert to DateTime.</param>
		/// <returns>Returns a DateTime object that represents value of the Unix time.</returns>
		public static DateTime UnixTimeToDateTime(long unixtime)
		{
			DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return sTime.AddSeconds(unixtime);
		}

		/**
		 * 
		 */ 
		//public void Reset(){
		//}




		/**
		 * 
		 */
		//public static void Start(){
		//	logger.Start();
		//}

		/**
		 * 
		 
		public static void Stop(){
			logger.Stop();
		}
*/

		/**
		 * 
		 */
		public void Update(){
			#if !UNITY_WEBGL
			logger.update (Time.deltaTime);
			#endif
		}
	}
}

