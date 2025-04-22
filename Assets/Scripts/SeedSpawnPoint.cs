using UnityEngine;
using System.Collections; // Necesario para Coroutines

// Este script va en el GameObject que actúa como punto de spawn (la "bolsa").
// NECESITA: Collider con IsTrigger=true, Rigidbody con IsKinematic=true.
public class SeedSpawnPoint : MonoBehaviour
{
    [Header("Prefab y Posición")]
    [Tooltip("Arrastra aquí el Prefab de tu semilla (con SeedItem, Grabbable, etc.)")]
    public GameObject seedPrefab;
    [Tooltip("Punto exacto donde aparecerá la semilla. Si está vacío, usa este objeto.")]
    public Transform spawnLocation;

    [Header("Lógica de Auto-Respawn")]
    [Tooltip("¿Debe intentar reponer la semilla si se cae?")]
    public bool autoRespawnIfEmpty = true;
    [Tooltip("Segundos a esperar antes de reponer si la zona está vacía.")]
    public float respawnDelay = 3.0f;
    [Tooltip("Retraso en segundos antes de generar una nueva semilla después de ser agarrada.")]
    public float grabRespawnDelay = 0.5f;
    // Estado interno
    private bool isSeedInsideZone = false;
    private Coroutine delayedRespawnCoroutine = null;

    void Start()
    {
        if (spawnLocation == null)
        {
            spawnLocation = transform;
        }
        // Estado inicial: asumir que está vacío hasta que la primera semilla entre.
        isSeedInsideZone = false;
        SpawnNewSeed(); // Genera la primera semilla
        Debug.Log("<color=cyan>[SeedSpawnPoint] Iniciado. Intentando generar primera semilla.</color>");
    }

    // --- Lógica de Generación ---

    void SpawnNewSeed()
    {
        if (seedPrefab == null)
        {
            Debug.LogError("<color=red>[SeedSpawnPoint] ¡ERROR! No se ha asignado 'seedPrefab'.</color>");
            return;
        }

        // Antes de generar, cancelamos cualquier intento de respawn pendiente.
        CancelDelayedRespawn();

        GameObject newSeedObject = Instantiate(seedPrefab, spawnLocation.position, spawnLocation.rotation);
        Debug.Log($"<color=cyan>[SeedSpawnPoint] Nueva semilla '{newSeedObject.name}' instanciada.</color>");

        SeedItem seedItem = newSeedObject.GetComponent<SeedItem>();
        if (seedItem != null)
        {
            seedItem.AssignSpawnPoint(this);
            // Asumimos que la nueva semilla está DENTRO al spawnear.
            // OnTriggerEnter lo confirmará si los colliders se superponen correctamente.
            isSeedInsideZone = true;
            Debug.Log($"<color=lightblue>[SeedSpawnPoint] Zona marcada como OCUPADA (por nueva semilla).</color>");
        }
        else
        {
            Debug.LogError($"<color=red>[SeedSpawnPoint] ¡ERROR! Prefab '{seedPrefab.name}' no tiene SeedItem.</color>", newSeedObject);
            // Si no hay SeedItem, no podemos saber si está ocupada, mejor asumir que no.
            isSeedInsideZone = false;
        }
    }

    // Llamado por SeedItem cuando se agarra la semilla
    public void NotifySeedGrabbed()
    {
        Debug.Log("<color=orange>[SeedSpawnPoint] Notificación de AGARRE recibida.</color>");
        // La semilla agarrada ya no está "en la zona" para nosotros.
        isSeedInsideZone = false;
        Debug.Log($"<color=orange>[SeedSpawnPoint] Zona marcada como VACÍA (por agarre).</color>");

        // Cancelamos cualquier respawn automático que pudiera estar en curso.
        CancelDelayedRespawn();

        // Generamos la nueva semilla inmediatamente.
        // Invoke(nameof(SpawnNewSeed), 0f); // Podemos llamarla directamente aquí
        //SpawnNewSeed();
        StartCoroutine(DelayedGrabRespawnCoroutine());
    }

    // --- Lógica de Detección de Presencia (Triggers) ---

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Seed"))
        {
            // Si la semilla que entró es hija de este transform (la acabamos de spawnear),
            // podríamos ignorar este Enter si quisiéramos, pero marcarla como dentro es correcto.
            Debug.Log($"<color=green>[SeedSpawnPoint] OnTriggerEnter: Semilla '{other.gameObject.name}' detectada DENTRO.</color>");
            isSeedInsideZone = true;
            // Si una semilla entra, ya no necesitamos el respawn automático.
            CancelDelayedRespawn();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Seed"))
        {
            Debug.Log($"<color=red>[SeedSpawnPoint] OnTriggerExit: Semilla '{other.gameObject.name}' detectada FUERA.</color>");
            isSeedInsideZone = false; // Marcar que nuestra zona (spawner) está vacía

            // --- INICIO: Notificar a la Semilla que Salió ---
            SeedItem seedItem = other.GetComponentInParent<SeedItem>();
            if (seedItem != null)
            {
                // Llama al nuevo método en la semilla que acaba de salir
                seedItem.NotifyExitedSpawnZone();
            }
            // --- FIN: Notificar a la Semilla que Salió ---

            // Iniciar temporizador del SPAWNER para generar OTRA semilla si autoRespawn está activo
            if (autoRespawnIfEmpty)
            {
                StartDelayedRespawnCheck(); // Esto es para el spawner, no para destruir la que salió
            }
        }
    }


    // --- Lógica del Temporizador de Auto-Respawn ---

    void StartDelayedRespawnCheck()
    {
        // Si ya hay una corutina de espera, la cancelamos antes de empezar una nueva.
        CancelDelayedRespawn();
        Debug.Log($"<color=yellow>[SeedSpawnPoint] Iniciando cuenta atrás de {respawnDelay}s para auto-respawn...</color>");
        delayedRespawnCoroutine = StartCoroutine(CheckAndRespawnCoroutine());
    }

    void CancelDelayedRespawn()
    {
        if (delayedRespawnCoroutine != null)
        {
            Debug.Log("<color=grey>[SeedSpawnPoint] Cancelando respawn automático pendiente.</color>");
            StopCoroutine(delayedRespawnCoroutine);
            delayedRespawnCoroutine = null;
        }
    }

    IEnumerator CheckAndRespawnCoroutine()
    {
        // Espera el tiempo definido.
        yield return new WaitForSeconds(respawnDelay);

        Debug.Log($"<color=yellow>[SeedSpawnPoint] Fin de la espera. Comprobando si la zona sigue vacía...</color>");

        // Después de la espera, verifica si la zona SIGUE vacía.
        // (Podría haberse llenado si el jugador devolvió la semilla o si se agarró otra mientras tanto).
        if (!isSeedInsideZone)
        {
            Debug.Log("<color=yellow>[SeedSpawnPoint] La zona sigue VACÍA. ¡Generando semilla por ausencia!</color>");
            SpawnNewSeed();
        }
        else
        {
            Debug.Log("<color=grey>[SeedSpawnPoint] Comprobación finalizada: La zona ya NO está vacía. No se necesita respawn automático.</color>");
        }

        // La corutina ha terminado su trabajo.
        delayedRespawnCoroutine = null;
    }

    IEnumerator DelayedGrabRespawnCoroutine()
    {
        Debug.Log($"<color=yellow>[SeedSpawnPoint] Esperando {grabRespawnDelay}s para generar nueva semilla (post agarre)...</color>");
        yield return new WaitForSeconds(grabRespawnDelay);
        SpawnNewSeed();
    }
}