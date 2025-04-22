using UnityEngine;

public class SeedSpawnPointDetector : MonoBehaviour
{
    // === CONFIGURACIÓN NECESARIA EN EL EDITOR ===
    // 1. Este GameObject debe tener un Collider (Box, Sphere, etc.) con "Is Trigger" MARCADO.
    // 2. Este GameObject debe tener un Rigidbody con "Is Kinematic" MARCADO.
    // =============================================

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entró tiene la etiqueta "Seed"
        if (other.CompareTag("Seed"))
        {
            // Mensaje de entrada de SEMILLA en VERDE
            Debug.Log($"<color=green>[Detector] ¡SEMILLA ENTRÓ! Nombre: {other.gameObject.name}</color>");
        }
        else
        {
            // Mensaje de entrada de OTRO objeto en GRIS (opcional, para depurar)
            Debug.Log($"<color=grey>[Detector] Entró algo (NO semilla): {other.gameObject.name}, Tag: '{other.tag}'</color>");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verifica si el objeto que salió tiene la etiqueta "Seed"
        if (other.CompareTag("Seed"))
        {
            // Mensaje de salida de SEMILLA en ROJO
            Debug.Log($"<color=red>[Detector] ¡SEMILLA SALIÓ! Nombre: {other.gameObject.name}</color>");
            // Alternativa en amarillo:
            // Debug.Log($"<color=yellow>[Detector] ¡SEMILLA SALIÓ! Nombre: {other.gameObject.name}</color>");
        }
        else
        {
            // Mensaje de salida de OTRO objeto en GRIS (opcional, para depurar)
            Debug.Log($"<color=grey>[Detector] Salió algo (NO semilla): {other.gameObject.name}, Tag: '{other.tag}'</color>");
        }
    }
}