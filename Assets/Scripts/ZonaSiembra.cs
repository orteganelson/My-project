/*using UnityEngine;

public class ZonaSiembra : MonoBehaviour
{
    public ParticleSystem particleSistemaHermano;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Seed"))
        {
            Debug.Log("<color=green>¡La semilla cayó en la zona correcta!</color>");

            // Detener el sistema de partículas si fue asignado
            if (particleSistemaHermano != null)
            {
                //particleSistemaHermano.Stop();
                particleSistemaHermano.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            // Destruir la semilla
            //Destroy(other.gameObject);
        }
    }
}*/

using UnityEngine;

public class ZonaSiembra : MonoBehaviour
{
    public GameObject prefabTillage; // Arrastra aquí el prefab Tillage_1x1 desde Assets

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Seed"))
        {
            Debug.Log("<color=green> Semilla detectada. Iniciando siembra.</color>");

            // Instanciar el prefab en la posición del objeto Visual
            if (prefabTillage != null)
            {
                Instantiate(prefabTillage, transform.position, transform.rotation);
            }

            // Desactivar el cubo Visual
            gameObject.SetActive(false);

            // Destruir la semilla
            Destroy(other.gameObject);
        }
    }
}

