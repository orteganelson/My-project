using UnityEngine;
using System.Collections;
using Oculus.Interaction;

// Script en el PREFAB de la Semilla
[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(Rigidbody))]
public class SeedItem : MonoBehaviour
{
    [Header("Drop Logic")]
    [Tooltip("Segundos a esperar ANTES de comprobar si destruir (al soltar o salir de spawn).")]
    public float destroyCheckDelay = 1.5f; // Puedes ajustar este valor

    // Referencias y Estado
    private SeedSpawnPoint originalSpawnPoint;
    private bool hasNotifiedSpawn = false;
    private bool isInTargetZone = false;
    private const string TARGET_ZONE_TAG = "SeedTargetZone";
    private bool isCurrentlyGrabbed = false;

    // Referencia a la corutina de chequeo para poder detenerla
    private Coroutine checkCoroutine = null;

    // Asignación inicial (sin cambios)
    public void AssignSpawnPoint(SeedSpawnPoint spawner)
    {
        originalSpawnPoint = spawner;
        hasNotifiedSpawn = false;
        isCurrentlyGrabbed = false;
        isInTargetZone = false;
    }

    // --- Llamado por "When Select" (Agarrar) ---
    public void HandleGrabbed()
    {
        isCurrentlyGrabbed = true;
        StopCheckCoroutine(); // Detener chequeo si lo había
        Debug.Log($"<color=grey>[SeedItem] AGARRADO. Estado Grabbed={isCurrentlyGrabbed}. Chequeo detenido.</color>");
        if (!hasNotifiedSpawn && originalSpawnPoint != null)
        {
            originalSpawnPoint.NotifySeedGrabbed();
            hasNotifiedSpawn = true;
        }
    }

    // --- Llamado por "When Unselect" (Soltar) ---
    public void HandleReleased()
    {
        isCurrentlyGrabbed = false;
        Debug.Log($"<color=orange>[SeedItem] SOLTADO. Estado Grabbed={isCurrentlyGrabbed}. Iniciando chequeo...</color>");
        StartCheckCoroutine(); // Iniciar chequeo porque se soltó
    }

    // --- NUEVO: Llamado por SeedSpawnPoint cuando sale de su zona ---
    public void NotifyExitedSpawnZone()
    {
        Debug.Log($"<color=purple>[SeedItem] Notificación: SALIÓ de SpawnZone. ¿Agarrada? {isCurrentlyGrabbed}</color>");
        // Solo iniciar el chequeo si NO está siendo agarrada en este momento
        if (!isCurrentlyGrabbed)
        {
            Debug.Log($"<color=purple>[SeedItem] NO agarrada al salir de SpawnZone. Iniciando chequeo...</color>");
            StartCheckCoroutine(); // Iniciar chequeo porque se cayó/salió sola
        }
        // Si está agarrada, HandleReleased se encargará cuando se suelte.
    }

    // --- Métodos Helper para gestionar la Corutina ---
    private void StartCheckCoroutine()
    {
        // Detener cualquier chequeo anterior antes de iniciar uno nuevo
        StopCheckCoroutine();
        // Iniciar la corutina y guardar la referencia
        checkCoroutine = StartCoroutine(CheckZoneAfterDelayCoroutine());
    }

    private void StopCheckCoroutine()
    {
        // Si hay una corutina de chequeo en ejecución, detenerla
        if (checkCoroutine != null)
        {
            Debug.Log($"<color=grey>[SeedItem] Deteniendo corutina de chequeo de destrucción.</color>");
            StopCoroutine(checkCoroutine);
            checkCoroutine = null; // Limpiar la referencia
        }
    }

    // --- Coroutine de Chequeo Retardado (sin cambios en su lógica interna) ---
    private IEnumerator CheckZoneAfterDelayCoroutine()
    {
        Debug.Log($"<color=yellow>[SeedItem Coroutine] Iniciando espera de {destroyCheckDelay}s para {gameObject.name}</color>");
        yield return new WaitForSeconds(destroyCheckDelay);
        Debug.Log($"<color=yellow>[SeedItem Coroutine] Fin espera para {gameObject.name}. ¿Agarrada? {isCurrentlyGrabbed}. ¿En Zona? {isInTargetZone}</color>");

        // CHECK 1: ¿Agarrada de nuevo?
        if (isCurrentlyGrabbed)
        {
            Debug.Log($"<color=grey>[SeedItem Coroutine] Chequeo cancelado: {gameObject.name} está agarrado.</color>");
            checkCoroutine = null;
            yield break;
        }

        // CHECK 2: ¿En zona objetivo? (Si no fue agarrada)
        if (!isInTargetZone)
        {
            Debug.Log($"<color=red>[SeedItem Coroutine] {gameObject.name} FUERA de zona y NO agarrado. Destruyendo...</color>");
            Destroy(gameObject);
            // No hace falta limpiar checkCoroutine aquí, el objeto desaparece.
        }
        else
        {
            Debug.Log($"<color=lime>[SeedItem Coroutine] {gameObject.name} DENTRO de zona y NO agarrado. Permanece.</color>");
            checkCoroutine = null; // Limpiar referencia si la corutina termina con éxito
                                   // Aquí lógica de éxito
        }
        // Asegurarse de limpiar si no entró en los otros 'break' o 'Destroy'
        if (checkCoroutine != null) checkCoroutine = null;
    }

    // --- Detección de Triggers ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TARGET_ZONE_TAG))
        {
            isInTargetZone = true;
            Debug.Log($"<color=cyan>[SeedItem] ENTRÓ en zona '{TARGET_ZONE_TAG}'. Deteniendo chequeo (si existe).</color>");
            // Si entra en la zona segura, ya no debe ser destruida por abandono.
            StopCheckCoroutine();
        }
        // Podríamos añadir lógica similar si re-entra al SeedSpawnPoint,
        // aunque la lógica actual del spawner ya debería manejarlo.
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"<color=blue>[DEBUG] OnTriggerExit con {other.gameObject.name}</color>");
        if (other.CompareTag(TARGET_ZONE_TAG))
        {
            isInTargetZone = false;
            Debug.Log($"<color=magenta>[SeedItem] SALIÓ de zona '{TARGET_ZONE_TAG}'.</color>");

            if (!isCurrentlyGrabbed)
            {
                Debug.Log($"<color=purple>[SeedItem] Salió de zona segura y no está agarrada. Iniciando chequeo...</color>");
                StartCheckCoroutine();
            }
        }
    }
}