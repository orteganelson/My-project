using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    public string targetTag = "Tierra"; // ¡Asegúrate que este nombre coincida EXACTAMENTE con tu Tag!

    void Start()
    {
        // Comprobación inicial
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"ERROR en {gameObject.name}: ¡No se encontró ningún Collider en este objeto!", this);
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"AVISO en {gameObject.name}: El Collider NO está marcado como 'Is Trigger'. Los eventos OnTrigger no funcionarán.", this);
        }

        if (GetComponentInParent<Rigidbody>() == null)
        {
            Debug.LogError($"ERROR en {gameObject.name}: ¡No se encontró Rigidbody en este objeto o sus padres! Es necesario para Triggers.", this);
        }

        Debug.Log($"CollisionTest iniciado en {gameObject.name}. Esperando colisión Trigger con objetos de Tag '{targetTag}'.");
    }

    // Esta función se llama AUTOMÁTICAMENTE por Unity cuando otro Collider ENTRA en este Trigger
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter: '{this.gameObject.name}' detectó entrada de '{other.gameObject.name}' (Tag: '{other.tag}')");

        // Comprobamos si el objeto que entró tiene el Tag que buscamos
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"<color=green>¡ÉXITO! Colisión TRIGGER detectada con el objeto '{other.gameObject.name}' que tiene el tag correcto '{targetTag}'.</color>");
        }
        else
        {
            Debug.Log($"Colisión con '{other.gameObject.name}', pero su tag '{other.tag}' no es el buscado ('{targetTag}').");
        }
    }

    // Esta función se llama AUTOMÁTICAMENTE por Unity cuando otro Collider SALE de este Trigger
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit: '{this.gameObject.name}' detectó salida de '{other.gameObject.name}' (Tag: '{other.tag}')");

        if (other.CompareTag(targetTag))
        {
            Debug.Log($"<color=orange>Salida de colisión TRIGGER con el objeto '{other.gameObject.name}' (Tag: '{targetTag}').</color>");
        }
    }
}