using System.Collections;
using UnityEngine;
using TMPro;

public class VoiceOverManager : MonoBehaviour
{
    [Header("Configuración")]
    public SubtitleTrack track;
    public TextMeshProUGUI subtitleText;
    public AudioSource audioSource;

    private Coroutine subtitleCoroutine;

    public void PlayVoiceOver()
    {
        if (track == null || track.voiceClip == null)
        {
            Debug.LogWarning("No hay pista de subtítulos o clip de audio asignado.");
            return;
        }

        audioSource.clip = track.voiceClip;
        audioSource.Play();

        if (subtitleCoroutine != null)
            StopCoroutine(subtitleCoroutine);

        subtitleCoroutine = StartCoroutine(ShowSubtitles());
    }

    private IEnumerator ShowSubtitles()
    {
        subtitleText.text = "";

        foreach (var entry in track.subtitles)
        {
            yield return new WaitUntil(() => audioSource.time >= entry.startTime);
            subtitleText.text = entry.text;
            yield return new WaitUntil(() => audioSource.time >= entry.endTime);
            subtitleText.text = "";
        }
    }

    void Start()
    {
        PlayVoiceOver();
    }
}

