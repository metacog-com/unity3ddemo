using System;
using UnityEngine;

namespace MyEvents
{
	[Serializable]
	public class ShootEvent
	{
		public Vector3 playerPos;
		public Vector3 dir;

		public ShootEvent (Vector3 playerPos, Vector3 dir)
		{
			this.playerPos = playerPos;
			this.dir = dir; 
		}
	}

	[Serializable]
	public class TakeDamage
	{
		public int damage;
		public int healt;
		public TakeDamage(int amount, int currentHealt){
			damage = amount;
			healt = currentHealt; 
		}
	}

	[Serializable]
	public class Death
	{
		public bool isDeath;
		public Death(){
			isDeath = true; 
		}
	}


	[Serializable]
	public class EnemyNew
	{
		public string name;
		public EnemyNew(string name){
			this.name = name; 
		}
	}

	[Serializable]
	public class EnemyAttackEvt
	{
		public int damage;
		public EnemyAttackEvt(int damage){
			this.damage = damage; 
		}
	}

	[Serializable]
	public class EnemyDead
	{
		
	}

}

