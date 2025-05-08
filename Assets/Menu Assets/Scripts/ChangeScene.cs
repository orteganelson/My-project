/*using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}*/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChangeScene : MonoBehaviour
{
    public Animator fadeAnimator; // Asigna aquí el Animator de tu panel de fundido
    public string sceneToLoad; // La escena que se cargará después del fundido
    private AsyncOperation sceneLoadOperation;
    public void FadeAndLoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        fadeAnimator.SetTrigger("FadeOut"); // Activa la animación de FadeOut
        // La escena se cargará al final de la animación gracias al evento en el Animator
    }

    // Este método será llamado por un Evento de Animación al final de la animación FadeOut
    public void OnFadeComplete()
    {
        //SceneManager.LoadScene(sceneToLoad);
        //sceneLoadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
        sceneLoadOperation.allowSceneActivation = false; // No activar la escena automáticamente

    }
    public void ActivateScene()
    {
        if (sceneLoadOperation != null)
        {
            sceneLoadOperation.allowSceneActivation = true; // Activar la nueva escena cuando esté lista
        }
    }
}