using UnityEngine;

public class AradoController : MonoBehaviour
{
    public Transform puntoDeEnganche; // El objeto fijo adelante donde está enganchado
    public Transform timon; // El timón que mueve el jugador
    public float velocidadAvance = 2f; // Velocidad hacia adelante
    public float limiteLateral = 1f; // Máximo movimiento lateral permitido (izq-der)
    public float suavizadoMovimiento = 5f; // Qué tan suave sigue el timón

    private Vector3 offsetInicial;

    void Start()
    {
        // Calculamos la diferencia inicial entre el timón y el enganche
        offsetInicial = transform.position - puntoDeEnganche.position;
    }

    void Update()
    {
        // Movimiento lateral controlado por el timón
        Vector3 direccionLateral = timon.position - transform.position;
        float desplazamientoX = Mathf.Clamp(direccionLateral.x, -limiteLateral, limiteLateral);

        // Nueva posición del arado
        Vector3 nuevaPosicion = puntoDeEnganche.position + offsetInicial;

        // Aplicamos el avance en z
        nuevaPosicion += transform.forward * velocidadAvance * Time.deltaTime;

        // Aplicamos el movimiento lateral
        nuevaPosicion += transform.right * desplazamientoX;

        // Suavizamos el movimiento
        transform.position = Vector3.Lerp(transform.position, nuevaPosicion, Time.deltaTime * suavizadoMovimiento);

        // Opcional: mantener siempre la misma rotación en Y (evitar que rote el arado)
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = 0;
        eulerAngles.z = 0;
        transform.eulerAngles = eulerAngles;
    }
}
