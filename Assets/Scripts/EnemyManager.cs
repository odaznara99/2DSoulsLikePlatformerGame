using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public int maxEnemies = 5;
    private int currentEnemyCount = 0;
    private int totalSpawned = 0;

    public bool allEnemiesCleared = false;

    public delegate void AllEnemiesClearedEvent();
    public event AllEnemiesClearedEvent OnAllEnemiesCleared;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanSpawn()
    {
        return totalSpawned < maxEnemies;
    }

    public void RegisterEnemy()
    {
        currentEnemyCount++;
        totalSpawned++;
    }

    public void UnregisterEnemy()
    {
        currentEnemyCount--;
        currentEnemyCount = Mathf.Max(0, currentEnemyCount);

        CheckClearCondition();
    }

    void CheckClearCondition()
    {
        if (totalSpawned >= maxEnemies && currentEnemyCount == 0)
        {
            if (!allEnemiesCleared)
            {
                allEnemiesCleared = true;
                Debug.Log("All enemies cleared!");

                if (OnAllEnemiesCleared != null)
                    OnAllEnemiesCleared.Invoke();
            }
        }
    }
}
