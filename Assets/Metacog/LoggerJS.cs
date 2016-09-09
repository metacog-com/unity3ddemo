using System;

namespace MetacogSDK
{
	public class LoggerJS
	{
		private const float TICK_TIME = 1.0f; 
		private float elapsedTime;

		public LoggerJS ()
		{
			elapsedTime = 0.0f;
		}

		public void update(float dt){
			elapsedTime += dt; 
			if (elapsedTime > TICK_TIME) {
				//endpoint.ProcessQueue (eventBuffer, eventStorage);
				elapsedTime = 0; 
			}
		}

	
	}

}

