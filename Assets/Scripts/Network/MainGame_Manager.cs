using IPCA.Characters;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame_Manager : baseActor
{
    public List<Transform> Spawn_Positions = new List<Transform>();
    public List<Color> Player_Colors = new List<Color>();

    public Camera ViewCam;

    public bool isGameOver = false;

    protected override void onStart()
    {
        //Spawn my avatar
        GameManager.Instance.networkManager.Network_PlayerRef = PhotonNetwork.Instantiate("PlayerCharacter",
                Spawn_Positions[GameManager.Instance.networkManager.Room_CurrentConnections-1].position,
                Spawn_Positions[GameManager.Instance.networkManager.Room_CurrentConnections - 1].rotation, 0).GetComponent<Character_PlayerController>();

        GameManager.Instance.networkManager.Network_PlayerRef.Character_SetColor(Player_Colors[GameManager.Instance.networkManager.Room_CurrentConnections - 1]);
        
        GameManager.Instance.networkManager.Network_PlayerRef.Owner = CharacterOwner.Mine;

        GameManager.Instance.networkManager.Network_PlayerRef.Player_Refresh();

        ViewCam.gameObject.SetActive(false);
    }

    public override void onUpdate()
    {
        if (!isGameOver && (Game_GetAlivePlayers() < 3 || Game_isEveryInnocentDead() || !Game_isAnySusAlive()))
        {
            if (Game_isEveryInnocentDead())
            {
                //Sus wins
                isGameOver = true;
            }
            else if (Game_GetAlivePlayers() < 3 && Game_isAnySusAlive())
            {
                //Sus wins
                isGameOver = true;
            }
            else if (!Game_isAnySusAlive())
            {
                //Innocents wins
                isGameOver = true;
            }
        }
    }

    private bool Game_isEveryInnocentDead()
    {
        return false;
    }

    private bool Game_isAnySusAlive()
    {
        return false;
    }

    private int Game_GetAlivePlayers()
    {
        return 0;
    }

    public override void onFixedUpdate()
    {

    }

    public override void onLateUpdate()
    {

    }

    protected override void onDestroy()
    {

    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 30, 200, 20), "Current connections: " + GameManager.Instance.networkManager.Room_CurrentConnections);
    }
}
