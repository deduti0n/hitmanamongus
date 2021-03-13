using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DebugItemType
{
    Click,
    Mode,
    Toggle,
    Submenu
}

public class DebugMenuItem
{
    public delegate void ClickDelegate();
    ClickDelegate myClick;

    public string ItemName = "";
    public DebugItemType debugItemType = DebugItemType.Mode;

    public bool toggleState = false;

    public DebugMenuItem(string ItemName, DebugItemType debugItemType, ClickDelegate myClick)
    {
        this.ItemName = ItemName;
        this.debugItemType = debugItemType;
        this.myClick = myClick;
    }

    public void onClick()
    {
        if (debugItemType == DebugItemType.Toggle)
            toggleState = !toggleState;

        myClick();
    }
}

public class DebugMenuManager
{
    public bool PLAYER_FLY_MODE = false;

    public bool CAMERA_FREE_MODE = false;
    public bool UI_HIDE_HUD = false;

    float deltaTime = 0.0f;

    Vector2 nativeSize = new Vector2(1280, 720);

    List<DebugMenuItem> debugItems = new List<DebugMenuItem>();
    List<DebugMenuItem> debugSubmenuItems = new List<DebugMenuItem>();

    bool usingSubMenu = false;

    int debugIndex = 0;
    int debugSubmenuIndex = 0;
    bool isHoldingButton = false;
    bool isHoldingButtonSubmit = false;
    bool isHoldingButtonCancel = false;

    bool showLoadingBox = false;

    string submenuTitle = "";

    public DebugMenuManager()
    {
        //Debug settings
        debugItems.Add(new DebugMenuItem("Relaunch current scene", DebugItemType.Click, delegate () 
        {
            Debug.Log("Reloading scene...");
            showLoadingBox = true;
            CAMERA_FREE_MODE = false;
            GameManager.Instance.Scene_Load(SceneManager.GetActiveScene().name);
        }));

        debugItems.Add(new DebugMenuItem("Scene Loader...", DebugItemType.Submenu, delegate () 
        {
            usingSubMenu = true;
            debugSubmenuIndex = 0;
            submenuTitle = "List of scenes";

            //Fill submenu with items
            debugSubmenuItems.Clear();

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));

                debugSubmenuItems.Add(new DebugMenuItem(name, DebugItemType.Click, delegate ()
                {
                    //Load scene
                    showLoadingBox = true;

                    CAMERA_FREE_MODE = false;

                    Debug.Log("Loading scene...");
                    GameManager.Instance.Scene_Load(name);
                }));
            }
        }));

        debugItems.Add(new DebugMenuItem("Hide HUD", DebugItemType.Toggle, delegate ()
        {
            UI_HIDE_HUD = !UI_HIDE_HUD;
        }));

        debugItems.Add(new DebugMenuItem("Spawn Player", DebugItemType.Click, delegate ()
        {
            Debug.Log("Spawning player actor...");

            GameManager.Instance.GameIsPaused = false;
        }));

        debugItems.Add(new DebugMenuItem("Manual Camera", DebugItemType.Toggle, delegate ()
        {
            CAMERA_FREE_MODE = !CAMERA_FREE_MODE;
        }));
        
        debugItems.Add(new DebugMenuItem("God Mode", DebugItemType.Toggle, delegate ()
        {
            PLAYER_FLY_MODE = !PLAYER_FLY_MODE;
        }));
    }

    private void debugMenu_Selector(float axis, ref bool hold, ref int id, int maxID)
    {
        //Input -- UpDownArrows_PC
        if ((axis > 0f || Input.GetKeyDown(KeyCode.UpArrow)) && !hold)
        {
            if (id == 0)
                id = maxID;
            else
                id--;

            hold = true;

            GameManager.Instance.audioManager.PlayClipAt("AFX_DebugFocus", false).volume = 0.05f;
        }
        else if ((axis < 0f || Input.GetKeyDown(KeyCode.DownArrow)) && !hold)
        {
            if (id == maxID)
                id = 0;
            else
                id++;

            hold = true;

            GameManager.Instance.audioManager.PlayClipAt("AFX_DebugFocus", false).volume = 0.05f;
        }
    }

    public void debug_DrawMenu()
    {
        Vector3 scale = new Vector3(Screen.width / nativeSize.x, Screen.height / nativeSize.y, 1.0f);
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, scale);

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        
        float currentFPS = (1f / deltaTime);

        if(currentFPS > 30f)
            GUI.color = Color.green;
        else if(currentFPS < 30f)
            GUI.color = Color.red;
        else
            GUI.color = Color.cyan;

        GUI.Label(new Rect(10, 5, 200, 20), "FPS: " + (int)currentFPS);

        GUI.color = Color.white;

        if (GameManager.Instance.GameIsPaused)
        {
            GUI.Box(new Rect(5, 55, Screen.width / 6, Screen.height / 2), "DEBUG MENU");

            float itemY = 95;
            int id = 0;

            if (!GameManager.Instance.isLoadingScene)
            {
                if (!usingSubMenu)
                    debugMenu_Selector(Input.GetAxis("UpDownArrows"), ref isHoldingButton, ref debugIndex, debugItems.Count - 1);
                else
                {
                    debugMenu_Selector(Input.GetAxis("UpDownArrows"), ref isHoldingButton, ref debugSubmenuIndex, debugSubmenuItems.Count - 1);

                    if (Input.GetButton("Cancel"))
                    {
                        usingSubMenu = false;
                        GameManager.Instance.audioManager.PlayClipAt("AFX_DebugSelect", false).volume = 0.05f;
                    }
                }
            }

            if ((Input.GetAxis("UpDownArrows") == 0f && (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))))
                isHoldingButton = false;

            if (Input.GetButton("Submit") && !isHoldingButtonSubmit && !GameManager.Instance.isLoadingScene)
            {
                GameManager.Instance.audioManager.PlayClipAt("AFX_DebugSelect", false).volume = 0.05f;

                if(!usingSubMenu && debugItems.Count > 0)
                    debugItems[debugIndex].onClick();
                else if (usingSubMenu && debugSubmenuItems.Count > 0)
                    debugSubmenuItems[debugSubmenuIndex].onClick();

                isHoldingButtonSubmit = true;
            }
            else if (!Input.GetButton("Submit"))
                isHoldingButtonSubmit = false;

            //Draw main debug menu
            foreach (DebugMenuItem item in debugItems)
            {
                string prefix = "";
                string sufix = "*";

                if (debugIndex == id)
                {
                    if (!usingSubMenu)
                        prefix = "> ";

                    GUI.color = Color.yellow;
                }
                else if (item.toggleState)
                    GUI.color = Color.cyan;
                else
                    GUI.color = Color.white;

                if (!item.toggleState)
                    sufix = "";

                GUI.Label(new Rect(15, itemY, 250, 20), prefix + item.ItemName + sufix);

                itemY += 20;

                id++;
            }

            //Submenu
            if (usingSubMenu)
            {
                GUI.Box(new Rect(20 + Screen.width / 6, 100, Screen.width / 6, Screen.height / 3), submenuTitle);

                itemY = 120;
                id = 0;

                //Draw main debug menu
                foreach (DebugMenuItem item in debugSubmenuItems.ToArray())
                {
                    string prefix = "";
                    string sufix = "*";
                
                    if (debugSubmenuIndex == id)
                    {
                        prefix = "> ";
                        GUI.color = Color.yellow;
                    }
                    else
                        GUI.color = Color.white;

                    if (!item.toggleState)
                        sufix = "";

                    GUI.Label(new Rect(35 + Screen.width / 6, itemY, 250, 20), prefix + item.ItemName + sufix);

                    itemY += 20;

                    id++;
                }
            }

            //Loading prompt
            if (showLoadingBox)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(35, 35, 250, 20), "LOADING. PLEASE WAIT...");
            }
        }
        else
        {
            debugIndex = 0;
            usingSubMenu = false;
            showLoadingBox = false;
        }
    }

    public void debug_backgroundOperations()
    {
        
    }
}
