/*using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using TMPro;


public class SubtitleController : MonoBehaviour
{

    [SerializeField] private TextAsset subtitlesTextFile;
    [SerializeField] private string voiceLinesFolderPath;
    [SerializeField] private Subtitle[] subtitles;
    [SerializeField] private int currentSubtitleAIndex;

    private void OnValidate()
    {
        subtitles = subtitlesTextFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select((line, index) =>
        {
            return new Subtitle
            {
                line = index,
                text = voiceLine,
                duration = GetVoiceLineDuration(index)
            };
        }).ToArray();
    }

    public float GetVoiceLineDuration(int lime)
    {
        string path = $"{voiceLinesFolderPath}/line {line}":
       var clip = Resources.Load<AudioClip>(path);
        if (clip == null)
            return 0;
        return clip.length;
    }

    private IEnumerator ShowSubtitles()
    {
        while (currentSubtitleIndex < subtitles.length)
        {
            GetComponent<TextMeshProUGUI>().text = subtitles[currentSubtitleIndex].text;
            yield return new WaitForSeconds(subtitles[currentSubtitleIndex].duration);
            currentSubtitleAIndex++;
        }
    }


    private void Start() => StartCoroutine(ShowSubtitles());

}
*/