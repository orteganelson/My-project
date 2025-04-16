using UnityEngine;

public class BagTriggerTest : MonoBehaviour
{
    // Prefab de la semilla que se instanciará
    public GameObject seedPrefab;

    // Punto de agarre en la mano (LeftHandSeedAttach)
    public Transform handGrabAnchor;

    // Escala a asignar a la semilla
    public Vector3 spawnScale = new Vector3(0.1f, 0.1f, 0.1f);

    // Bandera para evitar instanciar múltiples semillas en una misma interacción
    private bool seedSpawned = false;

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("[Trigger EXIT] detectado con: " + other.name);

        if (!seedSpawned)
        {
            // Instanciar la semilla en la posición y rotación del handGrabAnchor
            GameObject seed = Instantiate(seedPrefab, handGrabAnchor.position, handGrabAnchor.rotation);

            // Ajustar la escala de la semilla
            seed.transform.localScale = spawnScale;

            // Parentizar la semilla al handGrabAnchor
            seed.transform.SetParent(handGrabAnchor);

            seedSpawned = true;
            Debug.Log("Semilla instanciada y parentizada a: " + handGrabAnchor.name);
        }
    }

    // Opcional: reiniciar la bandera al entrar al trigger
    private void OnTriggerEnter(Collider other)
    {
        seedSpawned = false;
    }
}
