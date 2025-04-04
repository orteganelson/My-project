using UnityEngine;
using Oculus.Interaction; // O Meta.Interaction
using Oculus.Interaction.Input;
using Oculus.Haptics; // Para el tipo Controller

[RequireComponent(typeof(GrabInteractable))] // Asegúrate que el tipo coincida con el interactable de tu palo
public class StickInfo : MonoBehaviour
{
    [Tooltip("Referencia al componente Interactable principal de este palo")]
    [SerializeField]
    private GrabInteractable _grabInteractable;

    private Oculus.Haptics.Controller? _currentHoldingController = null;
    public Oculus.Haptics.Controller? CurrentHoldingController => _currentHoldingController;

    void Awake()
    {
        if (_grabInteractable == null) { _grabInteractable = GetComponent<GrabInteractable>(); }
        if (_grabInteractable == null)
        {
            Debug.LogError("StickInfo: No se encontró o asignó el GrabInteractable en el palo.", this);
            enabled = false;
            return;
        }
        // Suscribirse a eventos DEL PALO
        _grabInteractable.WhenSelectingInteractorAdded.Action += HandleGrabbed;
        _grabInteractable.WhenSelectingInteractorRemoved.Action += HandleReleased;
    }

    void OnDestroy()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.WhenSelectingInteractorAdded.Action -= HandleGrabbed;
            _grabInteractable.WhenSelectingInteractorRemoved.Action -= HandleReleased;
        }
    }

    private void HandleGrabbed(GrabInteractor interactor)
    {
        // Este es el método que SÍ funciona para encontrar la mano
        ControllerRef controllerRef = interactor.GetComponentInParent<ControllerRef>();
        if (controllerRef != null)
        {
            _currentHoldingController = (controllerRef.Handedness == Handedness.Right)
                                        ? Oculus.Haptics.Controller.Right
                                        : Oculus.Haptics.Controller.Left;
            Debug.Log($"StickInfo: Palo agarrado por {_currentHoldingController.Value}");
        }
        else
        {
            Debug.LogWarning($"StickInfo: No se encontró ControllerRef en el interactor {interactor.name} al agarrar.", interactor);
            _currentHoldingController = null;
        }
    }

    private void HandleReleased(GrabInteractor interactor)
    {
        Debug.Log($"StickInfo: Palo soltado por {interactor.name}");
        _currentHoldingController = null;
    }
}