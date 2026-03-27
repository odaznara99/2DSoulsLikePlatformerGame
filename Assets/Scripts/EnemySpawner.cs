using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("Array of enemy prefabs to randomly select from when spawning.")]
    public GameObject[] enemyPrefabs;
    public float spawnRate = 2f;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private float timer;

    void Update()
    {
        if (!EnemyManager.Instance.CanSpawn()) return;

        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            timer = 0f;
            SpawnRandomEnemy();
        }
    }

    void SpawnRandomEnemy()
    {
        // Choose a random enemy from the list
        GameObject randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instantiate enemy
        GameObject newEnemy = Instantiate(randomEnemy, spawnPoint.position, Quaternion.identity);

        // Register to manager
        Bandit enemyScript = newEnemy.GetComponent<Bandit>();
        if (enemyScript != null)
        {
            enemyScript.SetAsSpawned();
        }

        EnemyManager.Instance.RegisterEnemy();
    }
}
