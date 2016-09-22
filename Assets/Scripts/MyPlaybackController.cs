using System;
using UnityEngine;
using MetacogSDK;

namespace MyEvents
{
	public class MyPlaybackController: MonoBehaviour, PlaybackController 
	{
		public MyPlaybackController ()
		{
		}
			
		public void onPlaybackEvent(string name, string eventData){
			if (name == "enemy_new") {
				EnemyNew enemyNew = JsonUtility.FromJson<MyEvents.EnemyNew> (eventData);
				Debug.Log ("New Enemy!" + enemyNew.name);
			} else if (name == "enemy_attack") {
				MyEvents.EnemyAttackEvt attackEvt = JsonUtility.FromJson<MyEvents.EnemyAttackEvt> (eventData);
				Debug.Log ("attack from enemy! damage: " + attackEvt.damage); 
			} else {
				Debug.LogError ("unknown event: " + name);
			}
		}

	}
}

