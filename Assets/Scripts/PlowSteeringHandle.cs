using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class PlowSteeringHandle : MonoBehaviour
{
    [Header("Referencias")]
    public List<OxFollowStick> oxen;
    public Transform directionIndicator;

    [Header("Movimiento")]
    public float moveSpeed = 1.5f;
    public float turnSensitivity = 0.5f;

    [Header("Control")]
    public bool isGrabbed = false;

    private void Update()
    {
        if (!isGrabbed) return;

        Vector3 moveDir = directionIndicator.forward;
        moveDir.y = 0;
        moveDir.Normalize();

        foreach (OxFollowStick ox in oxen)
        {
            if (ox == null) continue;

            NavMeshAgent agent = ox.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                // Mover directamente en la dirección deseada
                Vector3 velocity = moveDir * moveSpeed;
                agent.Move(velocity * Time.deltaTime); // Movimiento directo
                agent.isStopped = false;

                ox.ForceWalkAnimation(true);
            }
        }
    }

    public void OnGrabbed()
    {
        isGrabbed = true;
        Debug.Log("Timón agarrado. Bueyes comenzarán a moverse.");
    }

    public void OnReleased()
    {
        isGrabbed = false;
        Debug.Log("Timón soltado. Bueyes se detienen.");
        foreach (OxFollowStick ox in oxen)
        {
            if (ox == null) continue;
            ox.ForceStop();
        }
    }
}
