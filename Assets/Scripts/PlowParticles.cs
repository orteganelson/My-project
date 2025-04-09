using UnityEngine;

public class PlowParticles : MonoBehaviour
{
    [Header("Referencias")]
    public ParticleSystem dirtParticleSystem;
    public ParticleSystem dustParticleSystem;
    public Rigidbody plowRigidbody; // Sigue necesitando la referencia para saber QUÉ objeto medir

    [Header("Configuración")]
    public string groundTag = "Tierra";
    public float minSpeedToEmit = 0.1f;

    private bool isTouchingGround = false;
    private bool particlesPlaying = false;

    // --- Variables para calcular velocidad manual ---
    private Vector3 lastPosition;
    private bool isFirstFrame = true; // Para inicializar correctamente la primera vez
    // ---------------------------------------------

    void Start()
    {
        if (dirtParticleSystem == null || plowRigidbody == null)
        {
            Debug.LogError("Faltan referencias en el script PlowParticles!", this);
            enabled = false;
            return;
        }
        dirtParticleSystem.Stop();
        dustParticleSystem.Stop();
        particlesPlaying = false;

        // Inicializar la posición para el cálculo manual
        lastPosition = plowRigidbody.transform.position; // Usamos la posición del transform del Rigidbody
        isFirstFrame = false; // Ya no es el primer frame
        Debug.Log($"[PlowParticles] Start: Posición inicial para cálculo manual: {lastPosition}");
    }

    void Update()
    {
        if (plowRigidbody == null) return; // Seguridad por si algo falla

        // --- Calcular Velocidad Manualmente ---
        Vector3 currentPosition = plowRigidbody.transform.position;
        float currentSpeed = 0f;

        // Evitar cálculo en el primer frame o si el tiempo se detiene
        if (!isFirstFrame && Time.deltaTime > 0.0001f) // Time.deltaTime puede ser 0 a veces
        {
            Vector3 displacement = currentPosition - lastPosition; // Vector de desplazamiento
            currentSpeed = displacement.magnitude / Time.deltaTime; // Velocidad = distancia / tiempo
        }
        // Actualizar para el próximo frame
        lastPosition = currentPosition;
        isFirstFrame = false; // Asegurarse de que no sea el primer frame después de Start()
        // ------------------------------------

        if (isTouchingGround)
        {
            // Usamos la velocidad calculada manualmente
            Debug.Log($"[PlowParticles] Tocando Tierra. Velocidad (Calculada Manual): {currentSpeed} (Mínima requerida: {minSpeedToEmit})");

            if (currentSpeed >= minSpeedToEmit)
            {
                PlayParticles();
            }
            else
            {
                StopParticles(); // Detener si tocamos pero vamos muy lento
            }
        }
        else
        {
            StopParticles(); // Detener si no tocamos
        }
    }

    // --- OnTriggerEnter, OnTriggerExit, PlayParticles, StopParticles ---
    //      (Mantenlas como estaban en el paso anterior,
    //       incluyendo los Debug.Log si quieres)
    // -------------------------------------------------------------------

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(groundTag))
        {
            Debug.Log($"[PlowParticles] OnTriggerEnter con {other.name} (Tag: {groundTag})");
            isTouchingGround = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(groundTag))
        {
            Debug.Log($"[PlowParticles] OnTriggerExit con {other.name} (Tag: {groundTag})");
            isTouchingGround = false;
            StopParticles(); // Detenemos al salir
        }
    }

    void PlayParticles()
    {
        if (!particlesPlaying && dirtParticleSystem != null)
        {
            Debug.Log("<color=cyan>[PlowParticles] Iniciando partículas (manual speed)...</color>");
            dirtParticleSystem.Play();
            dustParticleSystem.Play();
            particlesPlaying = true;
        }
    }

    void StopParticles()
    {
        if (particlesPlaying && dirtParticleSystem != null)
        {
            Debug.Log("<color=yellow>[PlowParticles] Deteniendo partículas (manual speed)...</color>");
            dirtParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            dustParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            particlesPlaying = false;
        }
    }
}