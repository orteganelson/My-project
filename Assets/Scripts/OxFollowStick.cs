using UnityEngine;
using UnityEngine.AI; // Necesario para NavMeshAgent
using System.Collections; // <-- AÑADIR para Coroutines

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class OxFollowStick : MonoBehaviour
{
    private enum OxState
    {
        Idle,               // Quieto, esperando interacción
        Following,          // Siguiendo al jugador
        MovingToDestination,// Moviéndose automáticamente al punto final
        RotatingToFinal, // <-- NUEVO
        Locked              // Ha llegado al punto final y está bloqueado
    }
    private OxState currentState = OxState.Idle;

    private NavMeshAgent agent;
    private Animator animator;

    [Header("Setup")]
    public Transform playerTarget;
    public string stickTipTag = "StickTip";
    public string animatorIsWalkingParam = "IsWalking";

    [Header("Movement")]
    public float followStoppingDistance = 2.5f;
    public float rotationSpeed = 120f; // Grados por segundo para la rotación final

    // --- NUEVAS VARIABLES ---
    [Header("Plowing Zone Setup")]
    public Transform finalDestination; // Asigna aquí el GameObject "OxFinalDestination"
    public string plowingZoneTag = "PlowingZone"; // Tag de la Zona de Arado

    // --- Variables Internas ---
    private Coroutine rotationCoroutine = null; // Para guardar referencia a la coroutine de rotación

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent == null) { Debug.LogError("OxFollowStick: ¡NavMeshAgent no encontrado!", this); }
        if (animator == null) { Debug.LogError("OxFollowStick: ¡Animator no encontrado en este objeto!", this); }
        if (playerTarget == null) {/*... Código de fallback para playerTarget ...*/} // Mantener validación

        // --- NUEVA VALIDACIÓN ---
        if (finalDestination == null)
        {
            Debug.LogError($"OxFollowStick: ¡Final Destination no asignado en {gameObject.name}!", this);
        }
    }

    void Start()
    {
        currentState = OxState.Idle;
        if (agent != null) { agent.stoppingDistance = followStoppingDistance; }
        SetWalkingAnimation(false); // Empezar en Idle
        if (agent != null && agent.isOnNavMesh) { agent.isStopped = true; } // Asegurar que empieza parado si está en Navmesh
    }

    void Update()
    {
        // Salir si no estamos listos
        if (agent == null || !agent.isOnNavMesh) return;

        // --- LÓGICA BASADA EN ESTADOS ---
        switch (currentState)
        {
            case OxState.Idle:
                // No hacer nada activamente, esperar interacción en OnTriggerEnter
                // Asegurarse de que esté parado si llegó a Idle desde otro estado
                EnsureAgentStopped();
                break;

            case OxState.Following:
                HandleFollowingState();
                break;

            case OxState.MovingToDestination:
                HandleMovingToDestinationState();
                break;

            case OxState.RotatingToFinal:
                // La coroutine está haciendo el trabajo de rotación.
                // Podríamos añadir lógica aquí si es necesario, pero usualmente no hace falta.
                // Nos aseguramos de que el agente siga detenido.
                EnsureAgentStopped();
                break;

            case OxState.Locked:
                // El buey simplemente se queda quieto.
                // Asegurarse de que esté parado
                EnsureAgentStopped();
                break;
        }
    }

    // --- Lógica específica de cada estado ---

    void HandleFollowingState()
    {
        if (playerTarget == null) // Si perdemos el target mientras seguimos
        {
            StopFollowing(); // Volver a Idle
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > agent.stoppingDistance)
        {
            if (agent.isStopped || agent.destination != playerTarget.position)
            {
                agent.SetDestination(playerTarget.position);
                agent.isStopped = false;
                SetWalkingAnimation(true);
            }
        }
        else
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                SetWalkingAnimation(false);
            }
        }
    }


    void HandleMovingToDestinationState()
    {
        if (finalDestination == null) return;

        // ¿Hemos llegado?
        // Usamos una distancia un poco mayor que stoppingDistance para asegurarnos de que entre en el rango
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f) // Umbral ligeramente mayor
        {
            Debug.Log($"Buey {gameObject.name} ha llegado cerca del destino final. Iniciando rotación.");

            // --- CAMBIOS AQUÍ ---
            currentState = OxState.RotatingToFinal; // Cambiar al estado de rotación
            EnsureAgentStopped(); // Detener agente (ya debería estar parado, pero por si acaso)
            SetWalkingAnimation(false); // Animación Idle

            // Detener cualquier coroutine de rotación anterior si existiera (seguridad)
            if (rotationCoroutine != null) { StopCoroutine(rotationCoroutine); }

            // Iniciar la nueva coroutine de rotación
            rotationCoroutine = StartCoroutine(RotateToFinalCoroutine(finalDestination.rotation));
        }
        else if (agent.isStopped) // Si se detuvo antes, reactivar
        {
            SetWalkingAnimation(true);
            agent.isStopped = false;
        }
    }

    // --- NUEVA COROUTINE para Rotación Suave ---
    IEnumerator RotateToFinalCoroutine(Quaternion targetRotation)
    {
        Debug.Log($"Buey {gameObject.name} - Coroutine: Iniciando rotación suave.");
        Quaternion startRotation = transform.rotation;
        float angleDifference = Quaternion.Angle(startRotation, targetRotation);
        float duration = angleDifference / rotationSpeed; // Calcular duración basada en ángulo y velocidad
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Interpolar suavemente (Slerp es bueno para rotaciones)
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // Esperar al siguiente frame
        }

        // Asegurar la rotación final exacta y cambiar al estado bloqueado
        transform.rotation = targetRotation;
        currentState = OxState.Locked;
        rotationCoroutine = null; // Limpiar referencia a la coroutine
        Debug.Log($"Buey {gameObject.name} - Coroutine: Rotación final completada. Estado: Locked.");
    }

    // --- Detección de Colisiones/Triggers ---

    void OnTriggerEnter(Collider other)
    {
        // --- Interacción con el Palo ---
        if (other.CompareTag(stickTipTag))
        {
            // SOLO reaccionar al palo si estamos en Idle o Following
            if (currentState == OxState.Idle)
            {
                StartFollowing();
            }
            else if (currentState == OxState.Following)
            {
                StopFollowing();
            }
            // Si está en MovingToDestination o Locked, ignorar el palo
        }
        // --- Interacción con la Zona de Arado ---
        else if (other.CompareTag(plowingZoneTag))
        {
            // SOLO reaccionar a la zona si estamos SIGUIENDO al jugador
            if (currentState == OxState.Following)
            {
                Debug.Log($"Buey {gameObject.name} entrando en Zona de Arado mientras seguía. Iniciando movimiento a destino final.");
                InitiateMoveToDestination();
            }
        }
    }

    // --- Control de Estados (Modificados/Nuevos) ---

    void StartFollowing()
    {
        // Solo empezar a seguir si estamos en Idle
        if (currentState != OxState.Idle) return;
        if (playerTarget == null || agent == null || !agent.isOnNavMesh) return; // Validaciones básicas

        Debug.Log($"Buey {gameObject.name}: ¡Empezando a seguir!");
        currentState = OxState.Following;
        // La lógica de movimiento/animación la maneja Update
    }

    void StopFollowing()
    {
        // Solo detener si estábamos siguiendo
        if (currentState != OxState.Following) return;

        Debug.Log($"Buey {gameObject.name}: ¡Dejando de seguir (vuelve a Idle)!");
        currentState = OxState.Idle; // Volver al estado Idle normal
        EnsureAgentStopped();
        SetWalkingAnimation(false);
    }

    // --- NUEVA FUNCIÓN ---
    void InitiateMoveToDestination()
    {
        if (finalDestination == null || agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError($"Buey {gameObject.name}: No se puede iniciar movimiento a destino final (destino nulo o agente no listo).", this);
            // Considerar volver a Idle si falla? O quedarse en Following? Por ahora, cambiamos estado pero logueamos error.
            if (currentState != OxState.MovingToDestination) currentState = OxState.Idle; // Si falla, volver a Idle
            return;
        }

        currentState = OxState.MovingToDestination; // Cambiar estado
        agent.SetDestination(finalDestination.position); // Establecer destino
        // Usaremos el stopping distance del agente para la llegada inicial,
        // la coroutine afinará la posición si es necesario (aunque aquí solo rotamos)
        agent.stoppingDistance = 0.1f;// O un valor más pequeño si se prefiere
        agent.isStopped = false; // Asegurar que se mueva
        SetWalkingAnimation(true); // Activar animación de caminar
    }

    // --- Función Auxiliar para Animación ---
    void SetWalkingAnimation(bool isWalking)
    {
        if (animator != null) { animator.SetBool(animatorIsWalkingParam, isWalking); }
    }

    // --- NUEVA FUNCIÓN Auxiliar para detener agente ---
    void EnsureAgentStopped()
    {
        // Comprobación añadida: no intentar detener si ya está en proceso de detenerse/rotar
        if (currentState == OxState.RotatingToFinal) return;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
        {
            agent.isStopped = true;
            // Considera si ResetPath es necesario aquí. Puede serlo si quieres cancelar cualquier ajuste fino de posición.
            // agent.ResetPath();
        }
    }
}