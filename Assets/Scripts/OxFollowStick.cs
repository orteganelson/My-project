using UnityEngine;
using UnityEngine.AI;
using System.Collections;
// --- Namespaces Necesarios ---
using Oculus.Haptics;      // Para HapticClip, HapticClipPlayer, Controller
// using Oculus.Interaction; // Ya no son estrictamente necesarios para esta versión
// using Oculus.Interaction.Input; // Ya no son necesarios sin ControllerRef

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))] // AudioSource requerido
public class OxFollowStick : MonoBehaviour
{
    // --- Estados ---
    private enum OxState { Idle, Following, MovingToDestination, RotatingToFinal, Locked }
    private OxState currentState = OxState.Idle;

    // --- Componentes ---
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private HapticClipPlayer hapticPlayer; // Para el clip háptico

    // --- Configuración General ---
    [Header("Setup")]
    public Transform playerTarget;
    public string stickTipTag = "StickTip";
    public string animatorIsWalkingParam = "IsWalking";

    // --- Configuración de Movimiento ---
    [Header("Movement")]
    public float followStoppingDistance = 2.5f;
    public float rotationSpeed = 120f;

    // --- Configuración de Zona Final ---
    [Header("Plowing Zone Setup")]
    public Transform finalDestination;
    public string plowingZoneTag = "PlowingZone";

    // --- Feedback al Tocar ---
    [Header("Feedback al Tocar con Palo")]
    [Tooltip("Clip háptico a reproducir al iniciar seguimiento")]
    public HapticClip touchHapticClip; // Asigna TestClip1 o TestClip2
    [Tooltip("Sonido a reproducir al iniciar seguimiento")]
    public AudioClip touchAudioClip;  // Asigna tu sonido .wav/.mp3

    // --- Variables Internas ---
    private Coroutine rotationCoroutine = null;

    void Awake()
    {
        // Obtener componentes principales
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // --- Validaciones Completas ---
        if (agent == null) { Debug.LogError($"OxFollowStick ({gameObject.name}): NavMeshAgent no encontrado! Desactivando.", this); enabled = false; return; }
        if (animator == null) { Debug.LogError($"OxFollowStick ({gameObject.name}): Animator no encontrado! Desactivando.", this); enabled = false; return; }
        if (audioSource == null) { Debug.LogError($"OxFollowStick ({gameObject.name}): AudioSource no encontrado! Desactivando.", this); enabled = false; return; }

        // Validar Player Target (con fallback opcional a Camera.main)
        if (playerTarget == null)
        {
            Debug.LogWarning($"OxFollowStick ({gameObject.name}): Player Target no asignado! Intentando usar Camera.main.", this);
            if (Camera.main != null)
            {
                playerTarget = Camera.main.transform;
                Debug.Log($"OxFollowStick ({gameObject.name}): Player Target asignado a Camera.main.", this);
            }
            else
            {
                // Considera si la falta de target debe desactivar el script
                Debug.LogError($"OxFollowStick ({gameObject.name}): Player Target no asignado y Camera.main no encontrada! El seguimiento no funcionará.", this);
                // enabled = false; // Podrías desactivarlo si es crítico
                // return;
            }
        }

        // Validar Final Destination (crítico para el estado MovingToDestination)
        if (finalDestination == null)
        {
            Debug.LogError($"OxFollowStick ({gameObject.name}): Final Destination no asignado! El movimiento a la zona final fallará.", this);
            // Podrías desactivar el script si esta mecánica es esencial
            // enabled = false;
            // return;
        }

        // --- Configurar AudioSource ---
        audioSource.playOnAwake = false;

        // --- Crear Haptic Player ---
        SetupHapticPlayer();

        // --- Comprobar AudioClip ---
        if (touchAudioClip == null) { Debug.LogWarning($"Ox {gameObject.name}: No hay 'touchAudioClip' asignado.", this); }
    }

    void SetupHapticPlayer() // Función separada para crear player
    {
        if (touchHapticClip != null)
        {
            try
            {
                hapticPlayer = new HapticClipPlayer(touchHapticClip);
                Debug.Log($"Ox {gameObject.name}: HapticPlayer creado para clip '{touchHapticClip.name}'.");
            }
            catch (System.Exception e) { Debug.LogError($"Ox {gameObject.name}: Error creando HapticClipPlayer: {e.Message}", this); hapticPlayer = null; }
        }
        else { Debug.LogWarning($"Ox {gameObject.name}: No hay 'touchHapticClip' asignado.", this); hapticPlayer = null; }
    }

    void Start()
    {
        currentState = OxState.Idle;
        if (agent != null)
        {
            // Establecer stopping distance inicial para seguir al jugador
            agent.stoppingDistance = followStoppingDistance;
            if (agent.isOnNavMesh) { agent.isStopped = true; } // Asegurar que empieza parado si está en Navmesh
        }
        SetWalkingAnimation(false); // Empezar en Idle visualmente
    }

    void OnDestroy() // Limpiar al destruir o desactivar
    {
        hapticPlayer?.Dispose(); // Liberar recursos del Haptic Player
        if (rotationCoroutine != null) // Detener coroutine si está activa
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return; // Salir si no estamos listos

        // --- LÓGICA BASADA EN ESTADOS ---
        switch (currentState)
        {
            case OxState.Idle:
                EnsureAgentStopped();
                break;
            case OxState.Following:
                HandleFollowingState();
                break;
            case OxState.MovingToDestination:
                HandleMovingToDestinationState();
                break;
            case OxState.RotatingToFinal:
                EnsureAgentStopped(); // La coroutine se encarga de rotar
                break;
            case OxState.Locked:
                EnsureAgentStopped();
                break;
        }
    }
        
    // --- OnTriggerEnter (Feedback + Llamada a StartFollowing) ---
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(stickTipTag))
        {
            Debug.Log($"Ox {gameObject.name}: Toque de {other.name} [Tag:{other.tag}]. Estado: {currentState}");

            // Reaccionar SOLO si estamos en Idle
            if (currentState == OxState.Idle)
            {
                Debug.Log($"Ox {gameObject.name}: Toque válido en Idle. Activando feedback y seguimiento...");

                // 1. Activar Feedback (Audio y Háptica Simplificada)
                PlayTouchSound();
                PlayTouchHapticSimple(); // Intenta vibrar en ambos mandos

                // 2. Iniciar el Seguimiento
                Debug.Log("Ox: Llamando a StartFollowing...");
                StartFollowing();
                Debug.Log($"Ox: Estado DESPUÉS de llamar a StartFollowing: {currentState}"); // Confirmar cambio
            }
            // Detener si estamos siguiendo
            else if (currentState == OxState.Following)
            {
                PlayTouchSound();
                PlayTouchHapticSimple();
                StopFollowing();
            }
            else { Debug.Log($"Ox {gameObject.name}: Toque de palo ignorado. Estado: {currentState}"); }
        }
        // Zona de Arado
        else if (other.CompareTag(plowingZoneTag))
        {
            if (currentState == OxState.Following)
            {
                Debug.Log($"Buey {gameObject.name} entrando en Zona de Arado. Iniciando movimiento a destino final.");
                InitiateMoveToDestination();
            }
        }
    }

    // --- Funciones de Feedback ---
    void PlayTouchSound()
    {
        if (audioSource != null && touchAudioClip != null)
        {
            audioSource.PlayOneShot(touchAudioClip);
            Debug.Log($"Ox {gameObject.name}: Reproduciendo audio '{touchAudioClip.name}'.");
        }
    }

    void PlayTouchHapticSimple()
    {
        if (hapticPlayer != null)
        {
            try
            {
                Debug.Log($"Ox {gameObject.name}: Intentando reproducir haptic '{touchHapticClip.name}' en Controller.Both...");
                hapticPlayer.Play(Oculus.Haptics.Controller.Both); // Intenta en ambos
            }
            catch (System.Exception e) { Debug.LogError($"Ox {gameObject.name}: Error reproduciendo haptic: {e.Message}"); }
        }
        else { Debug.LogWarning($"Ox {gameObject.name}: No se reproduce haptic (Player nulo)."); }
    }


    // --- Funciones de Estado y Movimiento ---

    void HandleFollowingState()
    {
        if (playerTarget == null) { StopFollowing(); return; }
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer > agent.stoppingDistance)
        {
            if (agent.isStopped || agent.destination != playerTarget.position)
            { agent.SetDestination(playerTarget.position); agent.isStopped = false; SetWalkingAnimation(true); }
        }
        else { if (!agent.isStopped) { agent.isStopped = true; SetWalkingAnimation(false); } }
    }

    void HandleMovingToDestinationState()
    {
        if (finalDestination == null) { Debug.LogWarning($"Ox {gameObject.name}: Intentando moverse a destino pero finalDestination es null."); return; }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            Debug.Log($"Buey {gameObject.name}: Llegó cerca del destino final. Iniciando rotación.");
            currentState = OxState.RotatingToFinal;
            EnsureAgentStopped();
            SetWalkingAnimation(false);
            if (rotationCoroutine != null) { StopCoroutine(rotationCoroutine); }
            rotationCoroutine = StartCoroutine(RotateToFinalCoroutine(finalDestination.rotation));
        }
        else if (agent.isStopped && !agent.pathPending)
        { SetWalkingAnimation(true); agent.isStopped = false; }
    }

    IEnumerator RotateToFinalCoroutine(Quaternion targetRotation)
    {
        Debug.Log($"Buey {gameObject.name} - Coroutine: Iniciando rotación suave.");
        Quaternion startRotation = transform.rotation;
        float angleDifference = Quaternion.Angle(startRotation, targetRotation);
        float duration = (rotationSpeed > 0.01f) ? Mathf.Max(0.1f, angleDifference / rotationSpeed) : 0.1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
        currentState = OxState.Locked;
        rotationCoroutine = null;
        Debug.Log($"Buey {gameObject.name} - Coroutine: Rotación final completada. Estado: Locked.");
    }

    void StartFollowing()
    {
        if (currentState != OxState.Idle) { Debug.LogWarning("StartFollowing llamado pero el estado no era Idle."); return; }
        // Ya no necesitamos las otras comprobaciones aquí si asumimos que Awake las hizo
        // y que OnTriggerEnter solo llama a esto si other es válido.
        Debug.Log($"Buey {gameObject.name}: CAMBIANDO A ESTADO FOLLOWING!");
        currentState = OxState.Following;
    }

    void StopFollowing()
    {
        if (currentState != OxState.Following) return;
        Debug.Log($"Buey {gameObject.name}: Dejando de seguir (Idle)!");
        currentState = OxState.Idle;
        EnsureAgentStopped();
        SetWalkingAnimation(false);
    }

    void InitiateMoveToDestination()
    {
        if (finalDestination == null || agent == null || !agent.isOnNavMesh)
        { Debug.LogError($"Ox {gameObject.name}: Falla InitiateMoveToDestination.", this); if (currentState != OxState.MovingToDestination) currentState = OxState.Idle; return; }
        currentState = OxState.MovingToDestination;
        agent.SetDestination(finalDestination.position);
        agent.stoppingDistance = 0.1f; // Asegurarse de que sea pequeño
        agent.isStopped = false;
        SetWalkingAnimation(true);
    }

    void SetWalkingAnimation(bool isWalking) { if (animator != null) { animator.SetBool(animatorIsWalkingParam, isWalking); } }

    void EnsureAgentStopped()
    {
        if (currentState == OxState.RotatingToFinal) return;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
        { agent.isStopped = true; /* agent.ResetPath(); */ }
    }

    // Función auxiliar para encontrar hijos (si fuera necesaria en el futuro)
    // Si no la usas en otro sitio, puedes quitarla.
    private Transform FindDeepChild(Transform parent, string childName)
    {
        Transform result = parent.Find(childName);
        if (result != null)
            return result;
        foreach (Transform child in parent)
        {
            result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

} // Fin de la clase OxFollowStick