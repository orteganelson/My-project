using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Haptics;
using System.Collections;
using UnityEngine;

// Asegúrate de tener un AudioSource en el objeto o asignado
[RequireComponent(typeof(AudioSource))]
public class TriggerHapticOnGrab : MonoBehaviour
{
    [Header("Archivos a Reproducir")]
    [Tooltip("Arrastra aquí el archivo .haptic (TestClip1 o stick_grab)")]
    public HapticClip hapticClip;
    [Tooltip("Arrastra aquí el archivo de audio (.wav, .mp3) para el sonido de agarre")]
    public AudioClip audioClip;

    [Header("Configuración Vibración Manual (Fallback)")]
    [Range(0, 2.5f)]
    public float fallbackDuration = 0.1f;
    [Range(0, 1)]
    public float fallbackAmplitude = 0.5f;
    [Range(0, 1)]
    public float fallbackFrequency = 0.5f;

    [Header("Referencias de Interacción")]
    [Tooltip("Asigna el componente GrabInteractable de este objeto (para agarre directo)")]
    public GrabInteractable grabInteractable;
    [Tooltip("Asigna el componente DistanceGrabInteractable de este objeto (para agarre a distancia)")]
    public DistanceGrabInteractable distanceGrabInteractable;


    // --- Componentes privados ---
    private HapticClipPlayer clipPlayer;
    private AudioSource audioSource;

    void Awake()
    {
        // Obtener el AudioSource (RequireComponent asegura que exista)
        audioSource = GetComponent<AudioSource>();
        // Configurar AudioSource (opcional, pero bueno para efectos cortos)
        audioSource.playOnAwake = false; // No reproducir al inicio

        // --- Comprobación de los Interactables ---
        if (grabInteractable == null && distanceGrabInteractable == null)
        {
            Debug.LogError($"Error: Debes asignar al menos 'grabInteractable' o 'distanceGrabInteractable' en el Inspector!", this);
            enabled = false; // Desactivar el script si no hay nada que escuchar
            return;
        }
        // Aviso si alguno falta (opcional)
        if (grabInteractable == null) Debug.LogWarning("No hay 'grabInteractable' asignado. El agarre directo no activará efectos desde este script.", this);
        if (distanceGrabInteractable == null) Debug.LogWarning("No hay 'distanceGrabInteractable' asignado. El agarre a distancia no activará efectos desde este script.", this);


        // --- Comprobación y Creación del ClipPlayer ---
        //Debug.Log($"Awake: Configurando Haptics. hapticClip asignado es: {(hapticClip != null ? hapticClip.name : "NULL")}", this);
        if (hapticClip != null)
        {
            try
            {
                clipPlayer = new HapticClipPlayer(hapticClip);
                //Debug.Log($"Awake: HapticClipPlayer creado para '{hapticClip.name}'.", this);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Awake: Error al crear HapticClipPlayer: {e.Message}.", this);
                clipPlayer = null;
            }
        }
        else
        {
            //Debug.LogWarning("Awake: No hay Haptic Clip asignado. Se usará la vibración manual OVRInput si se activa.", this);
            clipPlayer = null;
        }

        // --- Comprobación del AudioClip ---
        if (audioClip == null)
        {
            //Debug.LogWarning("Awake: No hay AudioClip asignado en el Inspector. No habrá sonido de agarre.", this);
        }
    }

    // Es mejor suscribirse a eventos en OnEnable
    private void OnEnable()
    {
        // Suscribirse al evento de agarre directo si existe
        if (grabInteractable != null)
        {
            grabInteractable.WhenSelectingInteractorAdded.Action += HandleGrabInteractorAdded;
            // Podrías suscribirte a otros eventos si los necesitas, como WhenSelectingInteractorRemoved
        }

        // Suscribirse al evento de agarre a distancia si existe
        if (distanceGrabInteractable != null)
        {
            distanceGrabInteractable.WhenSelectingInteractorAdded.Action += HandleDistanceGrabInteractorAdded;
            // Podrías suscribirte a otros eventos si los necesitas
        }
    }

    // Y desuscribirse en OnDisable para evitar memory leaks o errores
    private void OnDisable()
    {
        // Desuscribirse siempre, comprobando si no son null
        if (grabInteractable != null)
        {
            grabInteractable.WhenSelectingInteractorAdded.Action -= HandleGrabInteractorAdded;
        }
        if (distanceGrabInteractable != null)
        {
            distanceGrabInteractable.WhenSelectingInteractorAdded.Action -= HandleDistanceGrabInteractorAdded;
        }
    }

    private void OnDestroy()
    {
        clipPlayer?.Dispose(); // Limpiar el player de háptica al destruir el objeto
    }

    // --- Manejadores de eventos específicos que llaman al manejador genérico ---

    // Se llama cuando GrabInteractable (directo) añade un interactor
    private void HandleGrabInteractorAdded(GrabInteractor interactor)
    {
        //Debug.Log($"HandleGrabInteractorAdded: Agarrado directamente por {interactor.name}");
        HandleInteractorAdded(interactor); // Llama al manejador común
    }

    // Se llama cuando DistanceGrabInteractable (a distancia) añade un interactor
    private void HandleDistanceGrabInteractorAdded(DistanceGrabInteractor interactor)
    {
        // Debug.Log($"HandleDistanceGrabInteractorAdded: Agarrado a distancia por {interactor.name}");
        HandleInteractorAdded(interactor); // Llama al manejador común
    }

    // --- NUEVO MÉTODO GENÉRICO: Maneja la lógica común para cualquier interactor ---
    private void HandleInteractorAdded(Component interactor) // Usamos Component como tipo base común
    {
        //Debug.Log($"HandleInteractorAdded: Procesando interactor {interactor.name} de tipo {interactor.GetType()}");

        // Intentamos encontrar el ControllerRef para saber qué mano es
        ControllerRef controllerRef = interactor.GetComponentInParent<ControllerRef>();
        if (controllerRef != null)
        {
            //Debug.Log($"ControllerRef encontrado. Handedness: {controllerRef.Handedness}");
            // Determinar el controlador OVR basado en la lateralidad (Handedness)
            OVRInput.Controller ovrController = (controllerRef.Handedness == Handedness.Right)
                                                ? OVRInput.Controller.RTouch
                                                : OVRInput.Controller.LTouch;
            TriggerEffects(ovrController); // Llamar a la función que dispara ambos efectos
        }
        else
        {
            //Debug.LogWarning($"No se encontró ControllerRef en el interactor {interactor.name} o sus padres. No se puede determinar la mano para la háptica.", interactor);
            // Si no sabemos la mano, al menos podemos intentar reproducir el sonido
            PlayGrabSound();
            // Podríamos decidir activar la háptica en ambas manos o ninguna si no se encuentra ControllerRef
            // Por ahora, solo se reproduce el sonido en este caso.
        }
    }


    // --- FUNCIÓN: Dispara ambos efectos (Sin cambios) ---
    public void TriggerEffects(OVRInput.Controller controller)
    {
        PlayGrabSound(); // Llama a la función para reproducir audio
        TriggerHaptics(controller); // Llama a la función para reproducir háptica
    }


    // --- FUNCIÓN PARA REPRODUCIR AUDIO (Sin cambios) ---
    private void PlayGrabSound()
    {
        if (audioClip != null && audioSource != null)
        {
            //Debug.Log($"Reproduciendo AudioClip: {audioClip.name}");
            audioSource.PlayOneShot(audioClip);
        }
        else
        {
            // Log si falta algo (ya se avisó en Awake también)
            // Debug.Log("No se puede reproducir sonido: falta AudioClip o AudioSource.");
        }
    }


    // --- FUNCIÓN PARA REPRODUCIR HÁPTICA (Sin cambios estructurales) ---
    public void TriggerHaptics(OVRInput.Controller controller)
    {
        // Logs de depuración
        //Debug.Log($"TriggerHaptics llamado para controller: {controller}. Verificando hapticClip...");
        //Debug.Log($"Valor de 'hapticClip' AHORA MISMO: {(hapticClip != null ? hapticClip.name : "NULL")}");
        //Debug.Log($"Valor de 'clipPlayer' AHORA MISMO: {(clipPlayer != null ? "Existe" : "NULL")}");

        // Reproducir HapticClip si está disponible
        if (hapticClip != null && clipPlayer != null)
        {
            // Debug.Log("Intentando reproducir Haptic Clip...");
            bool controllerFound = false;
            Oculus.Haptics.Controller targetHand = Oculus.Haptics.Controller.Left; // Default

            if (controller == OVRInput.Controller.RTouch)
            {
                targetHand = Oculus.Haptics.Controller.Right;
                controllerFound = true;
            }
            else if (controller == OVRInput.Controller.LTouch)
            {
                targetHand = Oculus.Haptics.Controller.Left;
                controllerFound = true;
            }
            // Podrías añadir casos para OVRInput.Controller.Touch, .LTrackedRemote, .RTrackedRemote si fueran relevantes

            if (controllerFound)
            {
                try
                {
                    clipPlayer.Play(targetHand);
                    //Debug.Log($"clipPlayer.Play({targetHand}) llamado.");
                }
                catch (System.Exception e) { Debug.LogError($"Error al llamar a clipPlayer.Play: {e.Message}", this); }
            }
            else { Debug.LogWarning($"No se pudo mapear OVRInput.Controller ({controller}) a Oculus.Haptics.Controller.", this); }
        }
        // Si no hay HapticClip definido, usar el fallback manual
        else
        {
            // Debug.LogWarning("hapticClip o clipPlayer es NULL. Ejecutando fallback TriggerHapticsRoutine (vibración manual)...");
            StartCoroutine(TriggerHapticsRoutine(controller));
        }
    }

    // Coroutine de fallback (Sin cambios)
    public IEnumerator TriggerHapticsRoutine(OVRInput.Controller controller)
    {
        // Debug.Log($"Executing TriggerHapticsRoutine para {controller} con Freq:{fallbackFrequency}, Amp:{fallbackAmplitude}, Dur:{fallbackDuration}");
        OVRInput.SetControllerVibration(fallbackFrequency, fallbackAmplitude, controller);
        yield return new WaitForSeconds(fallbackDuration);
        OVRInput.SetControllerVibration(0, 0, controller);
        // Debug.Log($"Finalizado TriggerHapticsRoutine para {controller}");
    }

    // Ya no necesitamos las funciones auxiliares GetComponentInParent o FindDeepChild si no las usas en otra parte
    // private Transform FindDeepChild(Transform parent, string childName) { /* ... */ }

}