using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnItem
    {
        public GameObject prefab;
        public Transform spawnPoint;
        public float spawnTime;
    }

    public List<SpawnItem> objectsToSpawn = new List<SpawnItem>();

    private void Start()
    {
        foreach (SpawnItem item in objectsToSpawn)
        {
            StartCoroutine(SpawnWithDelay(item));
        }
    }

    private IEnumerator SpawnWithDelay(SpawnItem item)
    {
        yield return new WaitForSeconds(item.spawnTime);

        if (item.prefab != null && item.spawnPoint != null)
        {
            Instantiate(item.prefab, item.spawnPoint.position, item.spawnPoint.rotation);
        }
    }
}
