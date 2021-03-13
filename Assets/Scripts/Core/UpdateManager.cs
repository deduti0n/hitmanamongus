using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdateManager : MonoBehaviour
{
    public bool isLoadingScene = false;

    public void asyncSceneLoader(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        isLoadingScene = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
            yield return null;

        isLoadingScene = false;

        //Unpause after load
        if (GameManager.Instance.GameIsPaused)
            GameManager.Instance.GameIsPaused = false;
    }

    void Update()
    {
        //Keeps gears inside the GameManager spinning
        GameManager.Instance.onUpdate();
    }

    void FixedUpdate()
    {
        //Keeps gears inside the GameManager spinning
        GameManager.Instance.onFixedUpdate();
    }

    void LateUpdate()
    {
        //Keeps gears inside the GameManager spinning
        GameManager.Instance.onLateUpdate();
    }

    void OnGUI()
    {
        //Keeps gears inside the GameManager spinning
        GameManager.Instance.onGUIDraw();
    }
}
