using UnityEngine;
using UnityEngine.Splines;

using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Haptics;

public class TestPlowSteeringHandle : MonoBehaviour
{
    [Header("Referencias")]
    public SplineAnimate oxenAnimator;
    public Transform handleTransform;

    [Header("Animadores de los Bueyes")]
    public Animator[] oxAnimators;
    public string animatorWalkingParam = "IsWalking";

    [Header("Control")]
    public bool isGrabbed = false;

    [Header("Feedback")]
    public TriggerHapticOnGrab hapticFeedback; // Asigna aquí tu script TriggerHapticOnGrab
    public Handedness triggerHand = Handedness.Right; // Puedes cambiar esto dinámicamente si necesitas

    public void OnGrabbed()
    {
        isGrabbed = true;
        oxenAnimator.Play();
        SetOxenWalking(true);
        Debug.Log("Timón agarrado. Bueyes avanzan por el spline.");
        // Trigger háptico + audio si está asignado
        if (hapticFeedback != null)
        {
            OVRInput.Controller controller = (triggerHand == Handedness.Right) ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
            hapticFeedback.TriggerEffects(controller);
        }
    }

    public void OnReleased()
    {
        isGrabbed = false;
        oxenAnimator.Pause();
        SetOxenWalking(false);
        Debug.Log("Timón soltado. Bueyes detenidos.");
    }

    private void SetOxenWalking(bool isWalking)
    {
        foreach (Animator animator in oxAnimators)
        {
            if (animator != null)
            {
                animator.SetBool(animatorWalkingParam, isWalking);
            }
        }
    }
}

/*
public class TestPlowSteeringHandle : MonoBehaviour
{
    [Header("Referencias")]
    public SplineAnimate oxenAnimator;
    public Transform handleTransform;
    public Animator oxenAnimatorController; //

    [Header("Animación")]
    public string animatorIsWalkingParam = "IsWalking"; //

    [Header("Control")]
    public bool isGrabbed = false;

    public void OnGrabbed()
    {
        isGrabbed = true;
        oxenAnimator.Play();
        if (oxenAnimatorController != null)
        {
            oxenAnimatorController.SetBool(animatorIsWalkingParam, true);
        }
        Debug.Log("Timón agarrado. Bueyes avanzan por el spline.");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        oxenAnimator.Pause();
        if (oxenAnimatorController != null)
        {
            oxenAnimatorController.SetBool(animatorIsWalkingParam, false);
        }
        Debug.Log("Timón soltado. Bueyes detenidos.");
    }
}*/

