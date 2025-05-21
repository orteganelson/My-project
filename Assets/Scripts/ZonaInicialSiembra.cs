using UnityEngine;

public class ZonaInicialSiembra : MonoBehaviour
{
    private bool yaActivado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!yaActivado && other.CompareTag("Buey"))
        {
            Debug.Log("<color=green>¡Iniciando Proceso de Siembra!</color>");
            yaActivado = true;

            // Activar el objeto hermano llamado "PuntosSiembra"
            Transform padre = transform.parent;
            if (padre != null)
            {
                Transform puntosSiembra = padre.Find("PuntosSiembra");
                if (puntosSiembra != null)
                {
                    puntosSiembra.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("No se encontró un objeto hermano llamado 'PuntosSiembra'.");
                }
            }
            else
            {
                Debug.LogWarning("Este objeto no tiene un padre en la jerarquía.");
            }
        }
    }
}
