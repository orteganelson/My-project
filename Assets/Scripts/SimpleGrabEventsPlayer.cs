using System.Collections;
using UnityEngine;
using Oculus.Haptics;

[RequireComponent(typeof(AudioSource))]
public class SimpleGrabEventsPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip grabClip;
    public AudioClip releaseClip;

    [Header("Haptic Clips")]
    public HapticClip grabHaptic;
    public HapticClip releaseHaptic;

    [Header("Fallback Settings")]
    public float fallbackDuration = 0.1f;
    public float fallbackAmplitude = 0.5f;
    public float fallbackFrequency = 0.5f;

    private AudioSource audioSource;

    private HapticClipPlayer grabHapticLeft;
    private HapticClipPlayer grabHapticRight;
    private HapticClipPlayer releaseHapticLeft;
    private HapticClipPlayer releaseHapticRight;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (grabHaptic != null)
        {
            grabHapticLeft = new HapticClipPlayer(grabHaptic);
            grabHapticRight = new HapticClipPlayer(grabHaptic);
        }

        if (releaseHaptic != null)
        {
            releaseHapticLeft = new HapticClipPlayer(releaseHaptic);
            releaseHapticRight = new HapticClipPlayer(releaseHaptic);
        }
    }

    public void OnGrab()
    {
        PlayClip(grabClip);
        PlayHaptics(grabHapticLeft, grabHapticRight);
    }

    public void OnRelease()
    {
        PlayClip(releaseClip);
        PlayHaptics(releaseHapticLeft, releaseHapticRight);
    }

    void PlayClip(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    void PlayHaptics(HapticClipPlayer left, HapticClipPlayer right)
    {
        bool played = false;

        if (left != null)
        {
            left.Play(Oculus.Haptics.Controller.Left);
            played = true;
        }

        if (right != null)
        {
            right.Play(Oculus.Haptics.Controller.Right);
            played = true;
        }

        if (!played)
        {
            StartCoroutine(HapticsFallback());
        }
    }

    IEnumerator HapticsFallback()
    {
        OVRInput.SetControllerVibration(fallbackFrequency, fallbackAmplitude, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(fallbackFrequency, fallbackAmplitude, OVRInput.Controller.RTouch);
        yield return new WaitForSeconds(fallbackDuration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }

    private void OnDestroy()
    {
        grabHapticLeft?.Dispose();
        grabHapticRight?.Dispose();
        releaseHapticLeft?.Dispose();
        releaseHapticRight?.Dispose();
    }
}
