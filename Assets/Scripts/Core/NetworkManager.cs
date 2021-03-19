using IPCA.Characters;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPCA.Networking
{
    public class Network_Player
    {
        public string PlayerName = "NoName";
        public int PlayerID = -1;

        public Character_PlayerController PlayerAvatar;

        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;

        public Network_Player()
        {

        }

        public void Network_SetPosition(Vector3 position)
        {
            PlayerPosition = position;
        }

        public void Network_SetRotation(Quaternion rotation)
        {
            PlayerRotation = rotation;
        }
        public void Network_SpawnAvatar()
        {
            if(PlayerAvatar == null)
            {
                //PlayerAvatar = Object.Instantiate(, PlayerPosition, PlayerRotation);
            }
        }
    }

    public enum NetworkConState
    {
        Connecting,
        Failed,
        Connected,
        Idle
    }

    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public NetworkConState Network_ConnectionState = NetworkConState.Idle;
        public Character_PlayerController Network_PlayerRef = null;
        
        private List<Character_PlayerController> Network_AllPlayerRef = new List<Character_PlayerController>();

        public bool Network_isLobbyLeader = false;
        public NetworkConState Room_ConnectionState = NetworkConState.Idle;

        public int Room_CurrentConnections = 0;

        void Awake()
        {
            //Automatically sync scenes
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Update()
        {
            if (Room_ConnectionState == NetworkConState.Connected)
                Room_CurrentConnections = PhotonNetwork.CurrentRoom.PlayerCount;
            else
                Room_CurrentConnections = 0;
        }

        public List<Character_PlayerController> Network_GetAllPlayerAvatars()
        {
            foreach(Character_PlayerController avatar in Network_AllPlayerRef.ToArray())
            {
                if (avatar == null)
                    Network_AllPlayerRef.Remove(avatar);
            }

            return Network_AllPlayerRef;
        }

        public void Network_AddPlayerAvatars(Character_PlayerController avatar)
        {
            Network_AllPlayerRef.Add(avatar);
        }

        public void Network_ConnectToPhoton()
        {
            //Connect to Photon network
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.ConnectUsingSettings();

            Network_ConnectionState = NetworkConState.Connecting;
        }

        public override void OnConnected()
        {
            base.OnConnected();

            Network_ConnectionState = NetworkConState.Connected;
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Network_ConnectionState = NetworkConState.Failed;

            Room_ConnectionState = NetworkConState.Failed;

            Debug.Log("onDisconnect(): " + cause);
        }

        public override void OnJoinedRoom()
        {
            Room_ConnectionState = NetworkConState.Connected;

            Debug.Log("Connected to room!");

            if (PhotonNetwork.IsMasterClient)
            {
                Network_isLobbyLeader = true;
                PhotonNetwork.LoadLevel("TestRoom_WaitingRoom"); // fixes client instantiating twice
            }
            else
                Network_isLobbyLeader = false;
        }

        public void Network_ConnectToRoom(string roomName)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = GameManager.Instance.PlayerName;

                Room_ConnectionState = NetworkConState.Connecting;

                RoomOptions roomOptions = new RoomOptions();
                TypedLobby typedLobby = new TypedLobby(roomName, LobbyType.Default);
                PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby);
            }
        }
    }
}