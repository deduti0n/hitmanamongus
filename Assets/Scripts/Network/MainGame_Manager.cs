using IPCA.Characters;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    protected void Start()
    {
        GameManager.Instance.ingameManager = this;

        //Spawn avatar
        int id = PhotonNetwork.CurrentRoom.PlayerCount - 1;

        if (id < 0)
            id = 0;

        Debug.Log("Spawning player...");

        GameManager.Instance.networkManager.Network_PlayerRef = PhotonNetwork.Instantiate("PlayerCharacter",
                Spawn_Positions[id].position,
                Spawn_Positions[id].rotation, 0).GetComponent<Character_PlayerController>();

        GameManager.Instance.networkManager.Network_PlayerRef.Character_SetColor(Player_Colors[id]);
        
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
        //filter out players names and show them on screen to start voting
        string[] playersFiltered = players.Split(';');

        List<string> playerNames = new List<string>();

        foreach(string player in playersFiltered)
        {
            if (player != "" && player != GameManager.Instance.PlayerName)
                playerNames.Add(player);
        }

        //Start timer and voting menu
        if (GameManager.Instance.networkManager.Network_isLobbyLeader)
        { 
            //timer is lobby leader side
            VotingTimer = 30; 
        }

        Game_UI_Ref.Menu_OpenVotingMenu(playerNames);

        votingStarted = true;
    }

    [PunRPC]
    void RequestStartVote()
    {
        if (GameManager.Instance.networkManager.Network_isLobbyLeader)
        {
            List<Character_PlayerController> avatars = new List<Character_PlayerController>(GameManager.Instance.networkManager.Network_GetAllPlayerAvatars());
            string players = "";

            foreach (Character_PlayerController avatar in avatars)
            {
                players += avatar.UI_PlayerName.text + ";";
            }

            photonView.RPC("StartVote", RpcTarget.All, players);
        }
    }

    [PunRPC]
    void CloseVote(string toKick)
    {
        Game_UI_Ref.Menu_CloseVotingMenu();
        VotingTimer = 30;
        VotingPoll.Clear();
        alreadyVoted = false;

        if (GameManager.Instance.PlayerName == toKick)
            GameManager.Instance.networkManager.Network_PlayerRef.Character_Kill();

        StartCoroutine(CooldownMenu());
    }

    IEnumerator CooldownMenu()
    {
        yield return new WaitForSeconds(4f);
        votingStarted = false;
    }

    [PunRPC]
    void Vote(string playerName)
    {
        if(GameManager.Instance.networkManager.Network_isLobbyLeader)
        {
            VotingPoll.Add(playerName);
        }
    }

    public void MenuVote(string playerName)
    {
        photonView.RPC("Vote", RpcTarget.All, playerName);
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
        if(Input.GetKeyDown(KeyCode.Z) && !votingStarted && GameManager.Instance.networkManager.Room_CurrentConnections > 1)
        {
            //Either ask lobby leader to start or start it myself
            if (GameManager.Instance.networkManager.Network_isLobbyLeader)
            {
                List<Character_PlayerController> avatars = new List<Character_PlayerController>(GameManager.Instance.networkManager.Network_GetAllPlayerAvatars());
                string players = "";

                foreach (Character_PlayerController avatar in avatars)
                {
                    players += avatar.PlayerName + ";";
                }

                photonView.RPC("StartVote", RpcTarget.All, players);

                votingStarted = true;
            }
            else
            {
                votingStarted = true;
                photonView.RPC("RequestStartVote", RpcTarget.All);
            }
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
        if (GameManager.Instance.networkManager.Network_isLobbyLeader && GameManager.Instance.networkManager.Room_CurrentConnections > 2 && !isGameOver)
        {
            /*if (Game_isEveryInnocentDead())
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
            }*/

            
        }

        //Voting
        if (GameManager.Instance.networkManager.Network_isLobbyLeader && (VotingTimer < 30f || VotingPoll.Count >= GameManager.Instance.networkManager.Room_CurrentConnections) && votingStarted)
        {
            string toKill = "";

            if (VotingPoll.Count > 2)
            {
                var q = from x in VotingPoll
                        group x by x into g
                        let count = g.Count()
                        orderby count descending
                        select new { Value = g.Key, Count = count };

                string popularVote = "";
                int DuplicatesVote = 0;

                foreach (var vote in q)
                {
                    if (DuplicatesVote < vote.Count)
                    {
                        popularVote = vote.Value;
                        DuplicatesVote = vote.Count;
                    }
                }

                toKill = popularVote;
            }

            //Skip or kill a player
            photonView.RPC("CloseVote", RpcTarget.All, toKill);

            votingStarted = false;
        }
        else if (VotingTimer > 0 && votingStarted)
        {
            //VotingTimer -= 1f * Time.deltaTime;

            if (VotingTimer < 0f)
                VotingTimer = 0f;
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
