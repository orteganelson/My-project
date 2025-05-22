using UnityEngine;

public class ZonaSiembra : MonoBehaviour
{
    public ParticleSystem particleSistemaHermano;
    public Material materialTierraArada; // Material nuevo para la tierra arada

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Seed"))
        {
            Debug.Log("<color=green> Semilla detectada. Iniciando siembra.</color>");

            // Detener partículas
            if (particleSistemaHermano != null)
            {
                particleSistemaHermano.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            // Cambiar material de Tillage1
            Transform tillage1 = transform.Find("Tillage1");
            if (tillage1 != null)
            {
                MeshRenderer renderer = tillage1.GetComponent<MeshRenderer>();
                if (renderer != null && materialTierraArada != null)
                {
                    renderer.material = materialTierraArada;
                    Debug.Log("<color=cyan>Material de Tillage1 cambiado correctamente.</color>");
                }
                else
                {
                    Debug.LogWarning("No se encontró el MeshRenderer en Tillage1 o el material no está asignado.");
                }
            }
            else
            {
                Debug.LogWarning("No se encontró el hijo llamado 'Tillage1'.");
            }

            // Activar el objeto hijo llamado "SemillaVisual"
            Transform semillaVisual = transform.Find("SemillaVisual");
            if (semillaVisual != null)
            {
                semillaVisual.gameObject.SetActive(true);
                Debug.Log("<color=yellow>Semilla visual activada.</color>");
            }
            else
            {
                Debug.LogWarning("No se encontró el hijo llamado 'SemillaVisual'.");
            }

            // (Opcional) destruir la semilla original que colisionó
            // Destroy(other.gameObject);

            // (Opcional) desactivar esta zona de siembra
            // gameObject.SetActive(false);
        }
    }
}