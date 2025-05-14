using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SubtitleEntry
{
    [Tooltip("Tiempo de inicio del subtítulo en segundos")]
    public float startTime;

    [Tooltip("Tiempo de fin del subtítulo en segundos")]
    public float endTime;

    [Tooltip("Texto que se mostrará como subtítulo")]
    [TextArea]
    public string text;
}

[CreateAssetMenu(fileName = "NewSubtitleTrack", menuName = "Subtitles/Subtitle Track")]
public class SubtitleTrack : ScriptableObject
{
    [Header("Clip de audio asociado a los subtítulos")]
    public AudioClip voiceClip;

    [Header("Lista de subtítulos sincronizados")]
    public List<SubtitleEntry> subtitles = new List<SubtitleEntry>();
}
