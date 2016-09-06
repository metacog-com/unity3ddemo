using UnityEngine;
using MetacogSDK;
using MyEvents;

public class EnemyManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public GameObject enemy;
    public float spawnTime = 3f;
    public Transform[] spawnPoints;


    void Start ()
    {
        InvokeRepeating ("Spawn2", spawnTime, spawnTime);
    }


    void Spawn2 ()
    {
        if(playerHealth.currentHealth <= 0f)
        {
            return;
        }

        int spawnPointIndex = Random.Range (0, spawnPoints.Length);
		Debug.Log ("instantiating " + enemy.name);
        Instantiate (enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
		MetacogSDK.Metacog.Send("enemy_new", new EnemyNew(enemy.name), MetacogSDK.EventType.MODEL);

    }
}
