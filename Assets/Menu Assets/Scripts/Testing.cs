using UnityEngine;
using UnityEngine.SceneManagement;
public class Testing : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}