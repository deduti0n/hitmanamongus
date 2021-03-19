using IPCA.Characters;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character_Task
{
    public interactionActor Task_interaction;
    public bool Task_State = false;

    public Character_Task(interactionActor interaction)
    {
        Task_interaction = interaction;
    }
}

public class MainGame_Manager : baseActor
{
    public List<Character_Task> AvailableTasks = new List<Character_Task>();
    public List<Transform> Spawn_Positions = new List<Transform>();
    public List<Color> Player_Colors = new List<Color>();

    public Menu_UI Game_UI;

    private Menu_UI Game_UI_Ref;

    public Camera ViewCam;

    public bool isGameOver = false;

    public float VotingTimer = 30f;

    public List<string> VotingPoll = new List<string>();

    public bool alreadyVoted = false;

    private PhotonView photonView;

    private bool votingStarted = false;
    private bool GameStarted = false;

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
        GameManager.Instance.networkManager.Network_PlayerRef.Game_UI_Ref = Game_UI_Ref;

        //Reset tasks
        foreach(Character_Task task in AvailableTasks)
        {
            task.Task_State = false;
            task.Task_interaction.isInteractible = true;
            task.Task_interaction.ExtraFunc += delegate() { task.Task_State = true; GameManager.Instance.networkManager.Network_PlayerRef.UpdateTasks(); };
        }

        int i = 0;

        while(i < 2)
        {
            int ranIndex = Random.Range(0, AvailableTasks.Count);

            if (!GameManager.Instance.networkManager.Network_PlayerRef.AvailableTasks.Contains(AvailableTasks[ranIndex]))
                GameManager.Instance.networkManager.Network_PlayerRef.AvailableTasks.Add(AvailableTasks[ranIndex]);

            i++;
        }

        GameManager.Instance.networkManager.Network_PlayerRef.UpdateTasks();

        //Disable overview cam
        ViewCam.gameObject.SetActive(false);

        //Get my component
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    void StartVote(string players)
    {
        //players --> filter out players names and show them on screen to start voting

        //Start timer and voting menu
        Game_UI_Ref.Menu_OpenVotingMenu();

        votingStarted = true;
    }

    [PunRPC]
    void CloseVote(string toKick)
    {
        Game_UI_Ref.Menu_CloseVotingMenu();
        VotingTimer = 30;
        VotingPoll.Clear();
        alreadyVoted = false;

        if(GameManager.Instance.PlayerName == toKick)
            GameManager.Instance.networkManager.Network_PlayerRef.Character_Kill();
    }

    [PunRPC]
    void Vote(string playerName)
    {
        if(GameManager.Instance.networkManager.Network_isLobbyLeader)
        {
            VotingPoll.Add(playerName);
        }

        alreadyVoted = true;
    }

    [PunRPC]
    void SetSuspect(string playerName)
    {
        if (GameManager.Instance.PlayerName == playerName)
            GameManager.Instance.networkManager.Network_PlayerRef.Character_isSuspect = true;
    }

    private void Update_Input()
    {
        if(Input.GetKeyDown(KeyCode.Z) && !votingStarted)
        {
            //Method call on all including self
            photonView.RPC("StartVote", RpcTarget.All);
        }
    }

    public override void onUpdate()
    {
        if(!GameStarted && GameManager.Instance.networkManager.Network_isLobbyLeader && GameManager.Instance.networkManager.Room_CurrentConnections > 3)
        {
            //Set someone as sus
            List<Character_PlayerController> avatars = new List<Character_PlayerController>(GameManager.Instance.networkManager.Network_GetAllPlayerAvatars());

            photonView.RPC("SetSuspect", RpcTarget.All, avatars[Random.Range(0, avatars.Count)].UI_PlayerName.text);

            GameStarted = true;
        }

        //Leader only
        if (GameStarted && GameManager.Instance.networkManager.Network_isLobbyLeader && GameManager.Instance.networkManager.Room_CurrentConnections > 2 && !isGameOver && (Game_GetAlivePlayers() < 3 || Game_isEveryInnocentDead() || !Game_isAnySusAlive()))
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

            //Voting
            if ((VotingTimer < 30f || VotingPoll.Count >= GameManager.Instance.networkManager.Room_CurrentConnections) && votingStarted)
            {
                string toKill = "";

                //Skip or kill a player
                photonView.RPC("CloseVote", RpcTarget.All, toKill);

                votingStarted = false;
            }
        }

        Update_Input();
    }

    private void Game_RoundRestart()
    {
        GameManager.Instance.networkManager.Network_PlayerRef.TeleportToSpawn();
    }

    private bool Game_isEveryInnocentDead()
    {
        int alive = 0;

        foreach (Character_PlayerController player in GameManager.Instance.networkManager.Network_GetAllPlayerAvatars())
        {
            if (!player.Character_isSuspect && !player.Character_isDead)
                alive++;
        }

        return alive > 0 ? false : true;
    }

    private bool Game_isAnySusAlive()
    {
        int alive = 0;

        foreach (Character_PlayerController player in GameManager.Instance.networkManager.Network_GetAllPlayerAvatars())
        {
            if (player.Character_isSuspect && !player.Character_isDead)
                alive++;
        }

        return alive > 0 ? true: false;
    }

    private int Game_GetAlivePlayers()
    {
        int alive = 0;

        foreach(Character_PlayerController player in GameManager.Instance.networkManager.Network_GetAllPlayerAvatars())
        {
            if (!player.Character_isDead)
                alive++;
        }

        return alive;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 30, 200, 20), "Current connections: " + GameManager.Instance.networkManager.Room_CurrentConnections);
    }
}
