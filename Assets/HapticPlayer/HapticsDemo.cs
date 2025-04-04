using Oculus.Haptics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HapticsDemo : MonoBehaviour
{
    [SerializeField] private List<HapticNAudio> hapticAndAudioClips;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] protected TextMeshProUGUI hapticClipName;

    private HapticClipPlayer hapticClipPlayer;

    private int currentClip;
    private int totalClips;

    public bool leftController;
    public bool rightController;

    private void Start()
    {
        totalClips = hapticAndAudioClips.Count;
        SetupPlayer(0);
    }

    private void SetupPlayer(int currentClip)
    {
        HapticNAudio clip = hapticAndAudioClips[currentClip];
        hapticClipName.text = clip.hapticClip.name;
        audioSource.clip = clip.audioClip;

        if (hapticClipPlayer == null)
            hapticClipPlayer = new HapticClipPlayer(clip.hapticClip);
        else
            hapticClipPlayer.clip = clip.hapticClip;
    }

    public void Play()
    {
        audioSource.Play();
        if (!leftController && !rightController)
            hapticClipPlayer.Play(Controller.Both);
        else if (leftController)
            hapticClipPlayer.Play(Controller.Left);
        else
            hapticClipPlayer.Play(Controller.Right);
    }

    public void Stop()
    {
        audioSource.Stop();
        hapticClipPlayer.Stop();
    }

    public void PlayNextClip()
    {
        currentClip++;
        if (currentClip > totalClips - 1)
        {
            currentClip = 0;
        }

        SetupPlayer(currentClip);

        Play();
    }

    public void PlayPreviousClip()
    {
        currentClip--;
        if (currentClip < 0)
        {
            currentClip = totalClips - 1;
        }

        SetupPlayer(currentClip);

        Play();
    }

    public void ToggleLeftController(bool value)
    {
        leftController = value;
    }

    public void ToggleRightController(bool value)
    {
        rightController = value;
    }
}

[System.Serializable]
public struct HapticNAudio
{
    public AudioClip audioClip;
    public HapticClip hapticClip;
}
