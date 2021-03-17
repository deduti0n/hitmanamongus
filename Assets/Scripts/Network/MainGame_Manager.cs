using IPCA.Characters;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGame_Manager : baseActor
{
    public List<Transform> Spawn_Positions = new List<Transform>();
    public List<Color> Player_Colors = new List<Color>();

    public Menu_UI Game_UI;

    private Menu_UI Game_UI_Ref;

    public Camera ViewCam;

    public bool isGameOver = false;

    public float VotingTimer = 30f;

    public Dictionary<string, string> VotingPoll = new Dictionary<string,string>();

    private PhotonView photonView;

    protected override void onStart()
    {
        //Spawn avatar
        GameManager.Instance.networkManager.Network_PlayerRef = PhotonNetwork.Instantiate("PlayerCharacter",
                Spawn_Positions[GameManager.Instance.networkManager.Room_CurrentConnections-1].position,
                Spawn_Positions[GameManager.Instance.networkManager.Room_CurrentConnections - 1].rotation, 0).GetComponent<Character_PlayerController>();

        GameManager.Instance.networkManager.Network_PlayerRef.Character_SetColor(Player_Colors[GameManager.Instance.networkManager.Room_CurrentConnections - 1]);
        
        GameManager.Instance.networkManager.Network_PlayerRef.Owner = CharacterOwner.Mine;

        GameManager.Instance.networkManager.Network_PlayerRef.Player_Refresh();

        //Spawn UI
        Game_UI_Ref = Instantiate(Game_UI);

        //Generate a number of tasks to complete 


        //Disable overview cam
        ViewCam.gameObject.SetActive(false);

        photonView = GetComponent<PhotonView>();
    }


    [PunRPC]
    void StartVote()
    {
        //Start timer and voting menu
        Game_UI_Ref.Menu_OpenVotingMenu();
    }

    private void Update_Input()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            //Method call on all
            photonView.RPC("StartVote", RpcTarget.All);
        }
    }


    public override void onUpdate()
    {
        //Game over
        if (GameManager.Instance.networkManager.Network_isLobbyLeader && !isGameOver && (Game_GetAlivePlayers() < 3 || Game_isEveryInnocentDead() || !Game_isAnySusAlive()))
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

        //Voting
        if(GameManager.Instance.networkManager.Network_isLobbyLeader && (VotingTimer < 30f || VotingPoll.Count >= GameManager.Instance.networkManager.Room_CurrentConnections))
        {
            //Either skip or kill the chosen player
        }

        Update_Input();
    }

    private void Game_RoundRestart()
    {
        GameManager.Instance.networkManager.Network_PlayerRef.TeleportToSpawn();
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
