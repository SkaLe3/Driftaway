using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

    [SerializeField] private Spawner SpawnerPrefab;
    [SerializeField] private List<EnemySpawnConfiguration> EnemySpawnConfigs;
    [SerializeField] private float InitialSpawnDelay = 2f;

    private Coroutine SpawnEnemiesCoroutine;
    private Coroutine StartSpawningCoroutine;

    public void StartSpawning()
    {
        StartSpawningCoroutine = StartCoroutine(StartSpawningWithDelay());
    }

    private IEnumerator StartSpawningWithDelay()
    {
        yield return new WaitForSeconds(InitialSpawnDelay);
        SpawnEnemiesCoroutine = StartCoroutine(SpawnEnemies());
    }

    public void StopSpawning()
    {
        if (SpawnEnemiesCoroutine != null)
        {
            StopCoroutine(SpawnEnemiesCoroutine);
        }
        if (StartSpawningCoroutine != null)
        {
            StopCoroutine(StartSpawningCoroutine);
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            var config = EnemySpawnConfigs.Where(sc => sc.MinimumTimeDifficultySecs <= GameMode.Instance.SurvivalTime).OrderBy(_ => Random.value).First();
            foreach (EnemySpawnPosition enemySpawnPosition in config.EnemySpawnPositions)
            {
                var spawner = Instantiate(SpawnerPrefab);
                spawner.StartCoroutine(spawner.TriggerSpawn(enemySpawnPosition.EnemyPrefab.gameObject, 1.5f, enemySpawnPosition.Position));
            }
            yield return new WaitForSeconds(Random.Range(3f, 6f));
        }
    }
}
