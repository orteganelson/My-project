using UnityEngine;

public class FadeAnimatorController : MonoBehaviour
{
    private Animator animator;

    // Nombres de los triggers en tu Animator Controller
    public string fadeInTriggerName = "FadeIn";
    public string fadeOutTriggerName = "FadeOut";

    void Awake()
    {
        // Obtiene la referencia al componente Animator en este GameObject
        animator = GetComponent<Animator>();

        // Asegúrate de que el Animator se haya encontrado
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject!");
            enabled = false; // Desactiva el script para evitar errores futuros
        }
    }

    public void StartFadeInAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(fadeInTriggerName);
        }
    }

    public void StartFadeOutAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(fadeOutTriggerName);
        }
    }
}