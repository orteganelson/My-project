using UnityEngine;

public class SeedBagInteraction : MonoBehaviour
{
    public GameObject seedPrefab;
    public Transform leftHandAttachPoint;
    public Transform rightHandAttachPoint;

    private bool leftHandInside = false;
    private bool rightHandInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LeftHand"))
            leftHandInside = true;

        if (other.CompareTag("RightHand"))
            rightHandInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LeftHand") && leftHandInside)
        {
            SpawnSeed(leftHandAttachPoint);
            leftHandInside = false;
        }

        if (other.CompareTag("RightHand") && rightHandInside)
        {
            SpawnSeed(rightHandAttachPoint);
            rightHandInside = false;
        }
    }

    void SpawnSeed(Transform handTransform)
    {
        GameObject seed = Instantiate(seedPrefab, handTransform.position, handTransform.rotation);
        seed.transform.SetParent(handTransform);
    }
}
