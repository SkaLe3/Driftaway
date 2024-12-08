using UnityEngine;
using System.Collections;
using System.Linq;

public class Spawner : MonoBehaviour
{
    [SerializeField] private AudioSource SpawnSound;
    [SerializeField] private GameObject SpawnEffect;

    public IEnumerator TriggerSpawn(GameObject objectPrefab, float delay, Vector3 position)
    {
        position.y = transform.position.y;
        transform.position = position;

        yield return new WaitForSeconds(delay);

        // Don't spawn objects if round ended after warning but before object spawned
        if (GameMode.Instance.CurrentState == EGameState.InProgress)
        {
            Instantiate(objectPrefab, position, Quaternion.identity);

            AudioManager.Instance.PlaySoundOneShot(SpawnSound.clip, SpawnSound.volume, Random.Range(0.9f, 1.1f));
            if (SpawnEffect != null)
            {
                var part = Instantiate(SpawnEffect, transform.position, Quaternion.identity);
                ParticleSystem ps = part.GetComponent<ParticleSystem>();
                Destroy(ps.gameObject, ps.main.duration);
            }
        }
        Destroy(gameObject);
    }
        
}
