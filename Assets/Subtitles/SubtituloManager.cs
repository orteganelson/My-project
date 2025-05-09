using UnityEngine;
using TMPro;
using System.Collections;

public class SubtituloManager : MonoBehaviour
{
    public AudioSource audioSource;
    public TextMeshProUGUI textoSubtitulo;
    public CanvasGroup panelSubtitulo;

    [System.Serializable]
    public class LineaSubtitulo
    {
        public string texto;
        public float tiempoInicio;
        public float duracion;
    }

    public LineaSubtitulo[] lineas;

    void Start()
    {
        StartCoroutine(ReproducirSubtitulos());
    }

    IEnumerator ReproducirSubtitulos()
    {
        audioSource.Play();

        foreach (var linea in lineas)
        {
            yield return new WaitForSeconds(linea.tiempoInicio);
            textoSubtitulo.text = linea.texto;
            panelSubtitulo.alpha = 1;
            yield return new WaitForSeconds(linea.duracion);
            panelSubtitulo.alpha = 0;
        }
    }
}
