#if !UNITY_WEBGL
using System;
using Amazon.Kinesis;

namespace MetacogSDK
{
	/**
	 * used to deserialize response from api/access/kinesis endpoint
	 *expected:
	 {
	 "apiVersion":"2013-12-02",
	 "accessKeyId":"ASIAJKRDBPZH7DFQ336A",
	 "secretAccessKey":"YQLuFj4wyDb33vVHoEaohPeDgj7YlJa97MT9VHwT",
	 "sessionToken":"FQoDYXdzEB0aDMr0sCruslTHzlY41iKsAToru7AyJybnVjwoqz/wuxPryPEW8eRmZ23q2wPcRURhCuyOZ8bLckoezEqEOUTp3DyU+0tZSr3RqUsBGB0eqdDuh4uTcVuTqICo6uCOGTWCeaAJ1hLbN+RZPYRHmfuPTx3WC34a1EP7K+y1ek0dMmJ8IB73R4vcmXIjNjTnYrAbEJ/C1P/noue7pOvvl0vdcbrWJyN3WyaxUe0ZD1Qf7diXMOH+YyorT4PT7h4ooqmSvgU=",
	 "region":"us-east-1",
	 "kinesisStreamName":"prodmetacog_raw",
	 "httpOptions":{"agent":false},
	 "time":1472500898171
	 }
	 */ 
	[Serializable]
	public class KinesisResponse
	{
		public String apiVersion;
		public String accessKeyId;
		public String secretAccessKey;
		public String sessionToken;
		public String region;
		public String kinesisStreamName;
		public long time;

		public KinesisResponse ()
		{
		}

		public AmazonKinesisClient toAmazonKinesisClient(){
			AmazonKinesisConfig clientConfig = new AmazonKinesisConfig ();
			clientConfig.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
			//clientConfig.RegionEndpointServiceName = kinesisStreamName;

			return new AmazonKinesisClient (
				accessKeyId,
				secretAccessKey,
				sessionToken,
				clientConfig
			);
		}
	}
}
#endif

