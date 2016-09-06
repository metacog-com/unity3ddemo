using System;

namespace MetacogSDK
{
	public class MetacogConfig
	{
		public const String VERSION = "0.0.1";

		public String publisherId { 
			get; 
			private set;
		}
		public String applicationId {
			get;
			private set;
		}
		public String widgetId {
			get;
			private set;
		}
		public String learnerId {
			get; 
			private set;
		}
		public String sessionId {
			get;
			private set;
		}

		public String learnerToken {
			get;
			private set;
		}

		public String apiEndpoint {
			get;
			private set;
		}

		private String mode = "production";
		private int queueTickTime = 200;
		private int maxBatchSize = 40*1024;
		private int maxRetries = 10;

		public MetacogConfig (String publisherId, String applicationId, String widgetId,
			String learnerId = null, String sessionId = null)
		{
			this.publisherId = publisherId;
			this.applicationId = applicationId;
			this.widgetId = widgetId;
			this.learnerId = learnerId;
			this.sessionId = sessionId;
			this.learnerToken = null; 

			apiEndpoint = "https://api.metacog.com";

			if (this.learnerId == null) {
				this.learnerId = "learner_" + DateTime.Now+ (new Random().Next());
				//Debug.Log("generated learnerID: "+this.learnerId);
			}
			if (this.sessionId == null) {
				this.sessionId = "session_" + DateTime.Now+ (new Random().Next());
				//Debug.Log("generated SessionID: " + this.sessionId);
			}

		}

		public MetacogConfig(String learnerToken, String learnerId){
			this.learnerId = learnerId;
			this.learnerToken = learnerToken; 
		}

	}
}

