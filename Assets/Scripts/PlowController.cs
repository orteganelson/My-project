using UnityEngine;

public class PlowController : MonoBehaviour
{
    [Header("Configuración del Raycast")]
    public Transform tip; // Asigna un objeto vacío en la punta de la herramienta
    public float raycastDistance = 0.2f; // Distancia hacia abajo desde la punta
    public LayerMask groundLayer; // Capa(s) del terreno 'Dirt'

    [Header("Configuración de Colocación")]
    public float spacing = 0.3f; // Distancia mínima entre chunks (ajusta según tamaño del chunk)
    public float verticalOffset = 0.01f; // Pequeño desfase vertical para evitar Z-fighting

    // No necesitamos asignar el pooler aquí si usamos el Singleton
    // public ObjectPooler tillageChunkPooler;

    private Vector3 lastPlacementPosition = Vector3.positiveInfinity; // Posición del último chunk, inicializada a un valor imposible
    private bool isPlowing = false; // Para saber si estamos tocando el suelo

    void Update()
    {
        RaycastHit hit;

        // Lanza un rayo hacia abajo desde la punta
        if (Physics.Raycast(tip.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            isPlowing = true;
            TryPlaceChunk(hit);
        }
        else
        {
            // Si dejamos de tocar el suelo, reseteamos la posición para que el próximo toque coloque un chunk inmediatamente
            if (isPlowing)
            {
                lastPlacementPosition = Vector3.positiveInfinity;
                isPlowing = false;
            }
        }
    }

    void TryPlaceChunk(RaycastHit hit)
    {
        // Calcula la distancia desde la última posición donde pusimos un chunk
        float distanceFromLast = Vector3.Distance(hit.point, lastPlacementPosition);

        // Si es el primer toque (lastPlacementPosition es infinito) O si nos hemos movido lo suficiente
        if (distanceFromLast > spacing)
        {
            // Pide un objeto al pooler usando el Singleton
            GameObject chunk = ObjectPooler.Instance.GetPooledObject();

            if (chunk != null) // Comprueba si el pool nos dio un objeto
            {
                // Calcula la posición de colocación con el pequeño desfase vertical
                Vector3 placementPosition = hit.point + hit.normal * verticalOffset;

                // Calcula la rotación para alinear el chunk con la normal del suelo
                // Asume que el "arriba" del chunk es Vector3.up
                Quaternion placementRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                // Podrías querer combinar esto con la dirección de movimiento para orientar franjas
                // Quaternion forwardRotation = Quaternion.LookRotation(transform.forward); // O la dirección del movimiento
                // placementRotation = Quaternion.LookRotation(forwardRotation * Vector3.forward , hit.normal); // Más complejo

                // Aplica la posición y rotación
                chunk.transform.position = placementPosition;
                chunk.transform.rotation = placementRotation;

                // ¡Activa el chunk para que sea visible!
                chunk.SetActive(true);

                // Actualiza la posición del último chunk colocado
                lastPlacementPosition = hit.point; // Usamos hit.point para la medición de distancia
            }
            // else -> El pool está agotado, no hacemos nada (o mostramos un aviso)
        }
    }
}