using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostSpawner : MonoBehaviour
{

    [SerializeField] private Spawner SpawnerPrefab;
    [SerializeField] private GameObject BoostPrefab;
    [SerializeField] private float InitialSpawnDelay = 8f;
    [SerializeField] private Vector2 MapExtents;

    private Coroutine SpawnBoostCoroutine;
    private Coroutine StartSpawnBoostCoroutine;

    public void StartSpawning()
    {
        StartSpawnBoostCoroutine = StartCoroutine(StartSpawningWithDelay());
    }

    private IEnumerator StartSpawningWithDelay()
    {
        yield return new WaitForSeconds(InitialSpawnDelay);
        SpawnBoostCoroutine = StartCoroutine(SpawnBoosters());
    }

    public void StopSpawning()
    {
        if (SpawnBoostCoroutine != null)
        {
            StopCoroutine(SpawnBoostCoroutine);
        }
        if (StartSpawnBoostCoroutine != null)
        {
             StopCoroutine(StartSpawnBoostCoroutine);
        }
        DestroyBoosters();
    }

    private IEnumerator SpawnBoosters()
    {
        while (true)
        {
            DestroyBoosters();
            Vector3 spawnPosition = new Vector3(Random.Range(-MapExtents.x, MapExtents.x),
                                        1,
                                        Random.Range(-MapExtents.y, MapExtents.y));
            var spawner = Instantiate(SpawnerPrefab);
            spawner.StartCoroutine(spawner.TriggerSpawn(BoostPrefab, 1.5f, spawnPosition));
            Debug.Log("After Spawn");  
            yield return new WaitForSeconds(Random.Range(15f, 20f));
        }
    }

    private void DestroyBoosters()
    {
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.layer == LayerMask.NameToLayer("Boost"))
            {
                BoostPickup boost = obj.GetComponent<BoostPickup>();
                if (boost) boost.Expire();
            }
        } 
    }
}
