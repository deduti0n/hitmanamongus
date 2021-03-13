using IPCA.Characters;
using IPCA.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    //One of the few monobehaviours that lives in every scene
    private UpdateManager updateManager;

    public NetworkManager networkManager;

    //Manager object references
    public ActorManager actorManager = new ActorManager();

    public DebugMenuManager debugManager = new DebugMenuManager();

    public AudioManager audioManager = new AudioManager();

    public Menu_UI UI_Ref;

    //References
    public string PlayerName = "NoName";

    //Pause
    public bool GameIsPaused
    {
        set
        {
            if (!isLoadingScene)
            {
                core_isPaused = value;
                Core_SetPauseState(core_isPaused);
            }
        }

        get
        {
            return core_isPaused;
        }
    }

    private bool core_isPaused = false;

    public bool isLoadingScene
    {
        get
        {
            return updateManager.isLoadingScene;
        }
    }

    #region GameManager Singleton Ref & Constructor

    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                new GameManager();

            return _instance;
        }
    }

    private GameManager()
    {
        if (_instance != null)
            return;
        
        _instance = this;

        onStart();
    }

    #endregion

    #region Main Methods

    private void onStart()
    {
        //Rendering Settings
        Application.targetFrameRate = 60;

        InitializeUpdateManager();
    }

    public void onGUIDraw()
    {
        debugManager.debug_DrawMenu();
    }

    public void onUpdate()
    {
        //Update other systems
        actorManager.onUpdate();

        debugManager.debug_backgroundOperations();

        if (Input.GetButtonDown("PauseKey"))
            GameIsPaused = !GameIsPaused;
    }

    public void onLateUpdate()
    {
        //Update other systems
        actorManager.onLateUpdate();
    }

    public void onFixedUpdate()
    {
        //Update other systems
        actorManager.onFixedUpdate();
    }

    #endregion

    #region Initializations

    private void InitializeUpdateManager()
    {
        GameObject temp = new GameObject("System_UpdateManager");
        updateManager = temp.AddComponent<UpdateManager>();

        //Keep it from destroy itself
        Object.DontDestroyOnLoad(updateManager);

        temp = new GameObject("System_NetworkManager");
        networkManager = temp.AddComponent<NetworkManager>();

        //Keep it from destroy itself
        Object.DontDestroyOnLoad(networkManager);
    }

    #endregion

    private void Core_SetPauseState(bool state)
    {
        if (debugManager.CAMERA_FREE_MODE)
            return;

        /*if(state)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;*/
    }

    public void Scene_Load(string sceneName)
    {
        if (!isLoadingScene)
            updateManager.asyncSceneLoader(sceneName);
        else
            Debug.Log("Scene already loading!");
    }
}
