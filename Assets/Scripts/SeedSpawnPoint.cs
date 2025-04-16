using UnityEngine;

public class SeedSpawnPoint : MonoBehaviour
{
    [Header("Prefab y posición de spawn")]
    public GameObject seedPrefab;
    public Transform spawnLocation;

    private GameObject currentSeed;
    private bool seedInZone = false;

    void Start()
    {
        SpawnNewSeed();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[SeedSpawnPoint] Algo salió del trigger: {other.gameObject.name}");

        // Si el objeto que salió del trigger tiene la etiqueta "Seed"
        if (other.CompareTag("Seed"))
        {
            Debug.Log("[SeedSpawnPoint] Una semilla salió del área de spawn");

            seedInZone = false;
            currentSeed = null;

            Invoke(nameof(SpawnNewSeed), 0.2f);
        }
    }

    void SpawnNewSeed()
    {
        if (currentSeed == null && !seedInZone)
        {
            currentSeed = Instantiate(seedPrefab, spawnLocation.position, spawnLocation.rotation);
            currentSeed.tag = "Seed"; // Asegurarse de que la semilla instanciada tenga la etiqueta
            seedInZone = true;

            Debug.Log("[SeedSpawnPoint] Nueva semilla instanciada");
        }
    }
}
