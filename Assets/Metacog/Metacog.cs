using UnityEngine;
using System; 
using System.Collections;
#if !UNITY_WEBGL
using Amazon;
using UnityEngine.Networking;
#endif
using System.Text; 

///\mainpage
/// This is the documentation for the Unity3D version of the Metacog Client Library: it allows Unity3D developers to send events to the <a href="//www.metacog.com">Metacog</a> backend and access other services. <br>
/// Please visit the <a href="http://www.metacog.com/developer/examples/unity3d_tutorial" href="_blank">tutorial</a> to learn how to integrate this library into your Unity3D project, 
/// and check the live demo <a href="http://www.metacog.com/developer/examples/unity3d_demo" href="_blank">here</a>.
/// You can learn more about Metacog, the Metacog API and the Client Library in the <a href="https://developer.metacog.com/">Metacog's developers portal. </a>


/// <summary>
/// The MetacogSDK namespace offer classes to make accessible the <a href="">Metacog platform</a> services from Unity3D.
/// </summary>
/// <description>
/// DEPENDENCIES<br>
/// For no webgl builds it requires the Amazon AWS library. Please download it 
/// from xxx and add it to your project. <br>
/// For Webgl it requires the Javascript Metacog Client Library in the container html page. 
/// please see the tutorial for details.   
/// </description>
namespace MetacogSDK {

	/// <summary>
	/// Metacog event required type.
	/// use MODEL for events that represent internal changes, like change on score,
	/// and UI for events triggered by the user, like clicking the help button.
	/// </summary>
	public enum EventType{
		MODEL,
		UI
	}

	/// <summary>
	/// MonoBehavior that should be added to the Unity3D project.
	/// </summary>
	/// <description>
	/// expose to Unity's editor the Metacog authentication credentials and
	/// also triggers the logger mechanism.
	/// </description>
	public class Metacog: MonoBehaviour  {

		public string PublisherID;
		public string ApplicationID;
		public string WidgetID;
		public string LearnerID;
		public string SessionID;
		public string LearnerToken;

		/// <summary>
		/// The mode: production by default. other values: playback
		/// </summary>
		public string Mode;

		/// <summary>
		/// enable the log tab in webgl builds. 
		/// </summary>
		public bool UseLogTab;

		private PlaybackController playbackController; 

		#if !UNITY_WEBGL
		public string apiEndpoint{ get; private set;}

		public API api{ get; private set;}

		/// <summary>
		/// The logger object implements the logic for periodically send batches of events to the backend.
		/// </summary>
		private static Logger logger;

		///<summary>
		/// store a read-only json version of the current session,
		/// to be added in each call to kinesis
		/// </summary>
		public string sessionJson{ get; private set;}  

		#endif

		/// <summary>
		/// Append an event to the queue and eventually, send it to the Metacog platform.
		/// </summary>
		/// <description>
		/// The developer is responsible of implement custom classes
		/// for each one of his events. those classes should be
		/// annotated as serializable. 
		/// @TODO: note that for playback, we will need to deserialize the data.
		/// in C#, the target type should be known before hand. 
		/// it means that we may need to provide some mechanism to register a 
		/// mapping between event-names and C# Classes.. 
		/// </description>
		/// <param name="eventName">descriptive label to identify events. i.e. "GameStarted"</param>
		/// <param name="data">Serializable object</param>
		/// <param name="eventType"><see cref="Metacog.SDK.EventType"/> </param>
		public static void Send(string eventName, object data, EventType eventType){
			#if UNITY_WEBGL
			string json = JsonUtility.ToJson (data);
			Application.ExternalEval("Metacog.Unity3D.send('"+eventName + "', '"+json+"', '"+eventType+"')");
			#else
			if(logger!=null)
				logger.Send (eventName, data, eventType);
			#endif
		}

		/// <summary>
		/// Begins the communication with the metacog platform.
		/// </summary>
		/// <description>
		/// Specific implementation will change according to the target platform:
		/// for WebGL, authentication and logging will be delegated to the external
		/// javascript Metacog Client's Library.
		/// for all the other platforms, the functionality will be executed 
		/// internally, using the Amazon AWS C# library. 
		/// </description>
		public void Start () {
			#if UNITY_WEBGL
			StartJS();
			#else 
			StartAWS();
			#endif
		}

		#if UNITY_WEBGL
		///<summary>
		/// Delegates Metacog's authentication to the external javascript Metacog Client Library. 
		///</summary>
		private void StartJS(){

			StringBuilder buffer = new StringBuilder (); 
			buffer.Append ("{ ");
			buffer.Append ("\"session\":"); buffer.Append (buildSessionJson());
			buffer.Append (",\"mode\":"); buffer.Append (addAttr (Mode));
			buffer.Append (",\"log_tab\":"); buffer.Append (this.UseLogTab ? "true" : "false");
			buffer.Append (" }");
			Application.ExternalEval("Metacog.Unity3D.init('"+buffer.ToString ()+"');");

			//there is a playback controller available?
			this.playbackController = this.GetComponent<PlaybackController>();

		}
		#else

		///<summary>
		///Request credentials and initializes the AWS SDK. Initializes the logging mechanism.
		/// </summary>
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

		/// <summary>
		/// build a string with the JSON representation of the authentication information. 
		/// </summary>
		/// <description>
		/// returns the following stringifyed json:
		/// <code>
		///{
		///	publisher_id: null,
		///	application_id: null,
		///	widget_id: null,
		///	learner_id: null,
		///	learner_token: null,
		///	session_id: null,
		///	auth_token: null
		///}
		///</code>
		/// </description>
		/// <returns>The session json as string</returns>
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


		/// <summary>
		/// Convert a date time object to Unix time representation.
		/// </summary>
		/// <param name="datetime">The datetime object to convert to Unix time stamp.</param>
		/// <returns>Returns a numerical representation (Unix time) of the DateTime object.</returns>
		public static long ConvertToUnixTime(DateTime datetime)
		{
			// source: http://www.fluxbytes.com/csharp/convert-datetime-to-unix-time-in-c/
			DateTime sTime = new DateTime(1970, 1, 1,0,0,0,DateTimeKind.Utc);
			return (long)((datetime - sTime).TotalSeconds * 1000.0f);
		}

		/// <summary>
		/// Convert Unix time value to a DateTime object.
		/// </summary>
		/// <param name="unixtime">The Unix time stamp you want to convert to DateTime.</param>
		/// <returns>Returns a DateTime object that represents value of the Unix time.</returns>
		public static DateTime UnixTimeToDateTime(long unixtime)
		{
			// source: http://www.fluxbytes.com/csharp/convert-datetime-to-unix-time-in-c/
			DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return sTime.AddSeconds(unixtime);
		}


		/// <summary>
		/// Update the logging mechanism (for native platforms).
		/// </summary>
		public void Update(){
			#if !UNITY_WEBGL
			logger.update (Time.deltaTime);
			#endif
		}

		/// <summary>
		/// receives a notification from javascript metacog CL, playback router.
		/// the event is formatted as "event_name@data_as_json" 
		/// so it has to split the parameter, use the name to retrieve the 
		/// matching c# class and do the deserialization. 
		/// </summary>
		/// <param name="evtStr">Evt string.</param>
		public void OnPlaybackEvent(string evtStr){
			Debug.Log ("got message from javascript: " + evtStr);
			//split the string 
			int index = evtStr.IndexOf(":");
			string name = evtStr.Substring (0, index);
			string json = evtStr.Substring (index + 1, evtStr.Length - index - 1);
			Debug.Log (name);
			Debug.Log (json);

			if (this.playbackController != null) {
				this.playbackController.onPlaybackEvent (name, json);
			} else {
				Debug.Log ("no playback controller found!");
			}
						  
		} 
	}
}

