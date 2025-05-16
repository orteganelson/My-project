using UnityEngine;
using UnityEngine.Splines;

public class TestPlowSteeringHandle : MonoBehaviour
{
    [Header("Referencias")]
    public SplineAnimate oxenAnimator;
    public Transform handleTransform;

    [Header("Control")]
    public bool isGrabbed = false;

    public void OnGrabbed()
    {
        isGrabbed = true;
        oxenAnimator.Play();
        Debug.Log("Timón agarrado. Bueyes avanzan por el spline.");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        oxenAnimator.Pause();
        Debug.Log("Timón soltado. Bueyes detenidos.");
    }
}
