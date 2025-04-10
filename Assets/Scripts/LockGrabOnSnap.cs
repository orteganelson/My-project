using UnityEngine;
using Oculus.Interaction; // Asegúrate que este namespace es correcto

public class LockGrabOnSnap : MonoBehaviour
{
    // Esta función pública será llamada por el evento "When Select" del SnapInteractable
    public void DisableGrabForInteractor(SnapInteractor snappedInteractor)
    {
        if (snappedInteractor == null)
        {
            Debug.LogWarning("DisableGrabForInteractor recibió un SnapInteractor nulo.");
            return;
        }

        // Busca el componente GrabInteractable en el MISMO GameObject que el SnapInteractor que se acaba de acoplar.
        GrabInteractable grabInteractable = snappedInteractor.GetComponent<GrabInteractable>();

        if (grabInteractable != null)
        {
            Debug.Log($"Objeto '{snappedInteractor.gameObject.name}' acoplado vía SnapInteractor. Deshabilitando GrabInteractable.");
            grabInteractable.enabled = false; // ¡Deshabilita el agarre!
        }
        else
        {
            Debug.LogWarning($"No se encontró GrabInteractable en el objeto '{snappedInteractor.gameObject.name}' que tiene el SnapInteractor.");
        }
    }
}