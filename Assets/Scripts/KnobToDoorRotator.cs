using UnityEngine;

public class KnobToDoorRotator : MonoBehaviour
{
    public Transform door;      // Objeto con Rigidbody que gira
    public Transform knob;      // Transform del pomo
    public float torqueMultiplier = 50f;

    private Quaternion lastRotation;
    private bool isGrabbed = false;

    void Start()
    {
        lastRotation = knob.rotation;
    }

    void FixedUpdate()
    {
        if (!isGrabbed) return;

        Quaternion deltaRotation = knob.rotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180) angle -= 360;
        Vector3 torque = axis * angle * torqueMultiplier;

        Rigidbody doorRb = door.GetComponent<Rigidbody>();
        if (doorRb != null)
        {
            doorRb.AddTorque(torque, ForceMode.Force);
        }

        lastRotation = knob.rotation;
    }

    // Estos dos métodos se conectan manualmente desde el Inspector
    public void OnGrabbed()
    {
        isGrabbed = true;
        lastRotation = knob.rotation;
    }

    public void OnReleased()
    {
        isGrabbed = false;
    }
}
