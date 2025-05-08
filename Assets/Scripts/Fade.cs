using UnityEngine;

public class Fade : MonoBehaviour
{
    public Animator animator;

    // Ya no necesitamos la llamada automática en Start
    // void Start()
    // {
    //     Invoke("FadeOut",2);
    // }

    public void FadeOut()
    {
        animator.Play("FadeOut");
    }
}