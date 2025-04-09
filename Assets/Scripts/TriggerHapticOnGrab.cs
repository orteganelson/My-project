using Oculus.Interaction;
using System.Collections;
using UnityEngine;
using Oculus.Interaction.Input;
using Oculus.Haptics;
// using OVR; // Solo si usas OVRInput

// Asegúrate de tener un AudioSource en el objeto o asignado
[RequireComponent(typeof(AudioSource))] // Añadimos esto para asegurar que exista o añadirlo automáticamente
public class TriggerHapticOnGrab : MonoBehaviour
{
    [Header("Archivos a Reproducir")]
    [Tooltip("Arrastra aquí el archivo .haptic (TestClip1 o stick_grab)")]
    public HapticClip hapticClip;
    [Tooltip("Arrastra aquí el archivo de audio (.wav, .mp3) para el sonido de agarre")]
    public AudioClip audioClip; // <-- NUEVO CAMPO PARA AUDIO

    [Header("Configuración Vibración Manual (Fallback)")]
    [Range(0, 2.5f)]
    public float fallbackDuration = 0.1f;
    [Range(0, 1)]
    public float fallbackAmplitude = 0.5f; // Bajar un poco por defecto
    [Range(0, 1)]
    public float fallbackFrequency = 0.5f; // Bajar un poco por defecto

    [Header("Referencias de Interacción")]
    [Tooltip("Asigna el componente GrabInteractable de este objeto")]
    public GrabInteractable grabInteractable; // Ya lo tenías

    // --- Componentes privados ---
    private HapticClipPlayer clipPlayer;
    private AudioSource audioSource; // <-- NUEVA VARIABLE PARA AUDIO SOURCE

    void Awake() // Cambiado a Awake para asegurar que AudioSource esté listo antes de Start
    {
        // Obtener el AudioSource (RequireComponent asegura que exista)
        audioSource = GetComponent<AudioSource>();
        // Configurar AudioSource (opcional, pero bueno para efectos cortos)
        audioSource.playOnAwake = false; // No reproducir al inicio

        // --- Comprobación del GrabInteractable ---
        if (grabInteractable == null)
        {
            //Debug.LogError("Error: El campo 'grabInteractable' no está asignado en el Inspector!", this);
            enabled = false;
            return;
        }

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
                //Debug.LogError($"Awake: Error al crear HapticClipPlayer: {e.Message}.", this);
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

        // Suscribirse al evento (mejor en OnEnable/OnDisable)
        grabInteractable.WhenSelectingInteractorAdded.Action += WhenSelectingInteractorAdded_Action;
    }

    private void OnDestroy() // O OnDisable si prefieres
    {
        // Desuscribirse siempre
        if (grabInteractable != null)
        {
            grabInteractable.WhenSelectingInteractorAdded.Action -= WhenSelectingInteractorAdded_Action;
        }
        clipPlayer?.Dispose(); // Limpiar el player
    }

    // Cuando se agarra el objeto
    private void WhenSelectingInteractorAdded_Action(GrabInteractor obj)
    {
        //Debug.Log($"WhenSelectingInteractorAdded_Action: Agarrado por {obj.name}");
        ControllerRef controllerRef = obj.GetComponentInParent<ControllerRef>();
        if (controllerRef)
        {
            //Debug.Log($"ControllerRef encontrado. Handedness: {controllerRef.Handedness}");
            OVRInput.Controller ovrController = (controllerRef.Handedness == Handedness.Right) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
            TriggerEffects(ovrController); // Llamar a la función que dispara ambos efectos
        }
        else
        {
            //Debug.LogWarning($"No se encontró ControllerRef en el interactor {obj.name} o sus padres. No se puede determinar la mano para la háptica.", obj);
            // Podríamos intentar reproducir el audio sin saber la mano
            PlayGrabSound();
            // Pero no la háptica específica
        }
    }

    // --- NUEVA FUNCIÓN: Dispara ambos efectos ---
    public void TriggerEffects(OVRInput.Controller controller)
    {
        PlayGrabSound(); // Llama a la función para reproducir audio
        TriggerHaptics(controller); // Llama a la función para reproducir háptica
    }


    // --- FUNCIÓN PARA REPRODUCIR AUDIO ---
    private void PlayGrabSound()
    {
        if (audioClip != null && audioSource != null)
        {
            //Debug.Log($"Reproduciendo AudioClip: {audioClip.name}");
            // Usamos PlayOneShot para efectos cortos, no interrumpe otros sonidos
            // y no necesita asignar el clip al source cada vez.
            audioSource.PlayOneShot(audioClip);
        }
        else
        {
            // Log si falta algo (ya se avisó en Awake también)
            // Debug.Log("No se puede reproducir sonido: falta AudioClip o AudioSource.");
        }
    }


    // --- FUNCIÓN PARA REPRODUCIR HÁPTICA (Modificada ligeramente) ---
    public void TriggerHaptics(OVRInput.Controller controller)
    {
        // Logs de depuración (como estaban)
        //Debug.Log($"TriggerHaptics llamado para controller: {controller}. Verificando hapticClip...");
        //Debug.Log($"Valor de 'hapticClip' AHORA MISMO: {(hapticClip != null ? hapticClip.name : "NULL")}");
        //Debug.Log($"Valor de 'clipPlayer' AHORA MISMO: {(clipPlayer != null ? "Existe" : "NULL")}");

        // Reproducir HapticClip si está disponible
        if (hapticClip != null && clipPlayer != null)
        {
            //Debug.Log("Intentando reproducir Haptic Clip...");
            bool controllerFound = false;
            Oculus.Haptics.Controller targetHand = Oculus.Haptics.Controller.Left;

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
        // Si no hay HapticClip, ¿quieres el fallback o no? Podrías quitar este else.
        else
        {
            //Debug.LogWarning("hapticClip o clipPlayer es NULL. Ejecutando fallback TriggerHapticsRoutine (vibración manual)...");
            StartCoroutine(TriggerHapticsRoutine(controller));
        }
    }

    // Coroutine de fallback (como estaba)
    public IEnumerator TriggerHapticsRoutine(OVRInput.Controller controller)
    {
        //Debug.Log($"Executing TriggerHapticsRoutine para {controller} con Freq:{fallbackFrequency}, Amp:{fallbackAmplitude}, Dur:{fallbackDuration}");
        OVRInput.SetControllerVibration(fallbackFrequency, fallbackAmplitude, controller);
        yield return new WaitForSeconds(fallbackDuration);
        OVRInput.SetControllerVibration(0, 0, controller);
        //Debug.Log($"Finalizado TriggerHapticsRoutine para {controller}");
    }

    // Función auxiliar (como estaba)
    private T GetComponentInParent<T>(GrabInteractor obj) where T : Component { /* ... */ return null; }
    // Implementación real de FindDeepChild (necesaria si se usa antes)
    private Transform FindDeepChild(Transform parent, string childName) { Transform result = parent.Find(childName); if (result != null) return result; foreach (Transform child in parent) { result = FindDeepChild(child, childName); if (result != null) return result; } return null; }

}