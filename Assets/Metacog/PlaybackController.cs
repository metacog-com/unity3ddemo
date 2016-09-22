using System;

namespace MetacogSDK
{
	/// <summary>
	/// </summary>
	public interface PlaybackController
	{
		/// <summary>
		/// receive a event instance from the metacog playback mechanism
		/// </summary>
		/// <param name="eventName">Event name.</param>
		/// <param name="jsonData">Event data as json string.</param>
		void onPlaybackEvent(string eventName, string jsonData);

	}
}

