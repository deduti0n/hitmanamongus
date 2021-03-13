using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Menu_Main : baseActor
{
    public TextMeshProUGUI UI_PhotonConnectionState;
    public TextMeshProUGUI UI_RoomName;
    public TextMeshProUGUI UI_GamerName;

    public GameObject UI_MainOptions;
    public CanvasGroup UI_RoomOptions;
    public GameObject UI_RoomState;

    private bool isConnected = false;

    public void Networking_OpenRoomMenu()
    {
        UI_MainOptions.gameObject.SetActive(false);
        UI_RoomOptions.gameObject.SetActive(true);
    }

    public void Networking_CloseRoomMenu()
    {
        UI_MainOptions.gameObject.SetActive(true);
        UI_RoomOptions.gameObject.SetActive(false);
    }

    public void Networking_ConnectToRoom()
    {
        UI_RoomOptions.interactable = false;
        UI_RoomOptions.alpha = 0.5f;
        UI_RoomState.gameObject.SetActive(true);


        UI_GamerName.text = UI_GamerName.text.Replace(" ", "");

        if (UI_GamerName.text.Length > 0)
            GameManager.Instance.PlayerName = UI_GamerName.text;

        GameManager.Instance.networkManager.Network_ConnectToRoom(UI_RoomName.text);
    }

    protected override void onStart()
    {
        //Make sure mouse is unlocked
        Cursor.lockState = CursorLockMode.None;

        //Start connection
        GameManager.Instance.networkManager.Network_ConnectToPhoton();
    }

    public override void onUpdate()
    {
        switch(GameManager.Instance.networkManager.Network_ConnectionState)
        {
            case IPCA.Networking.NetworkConState.Connected:
                {
                    UI_PhotonConnectionState.text = "Connected!";
                    UI_PhotonConnectionState.color = Color.green;

                    if(!isConnected)
                    {
                        UI_MainOptions.gameObject.SetActive(true);
                        UI_RoomOptions.gameObject.SetActive(false);

                        UI_RoomState.gameObject.SetActive(false);

                        UI_RoomOptions.interactable = true;
                        UI_RoomOptions.alpha = 1f;
                    }

                    isConnected = true;

                    break;
                }
            case IPCA.Networking.NetworkConState.Connecting:
                {
                    UI_PhotonConnectionState.text = "Connecting...";
                    UI_PhotonConnectionState.color = Color.yellow;

                    UI_MainOptions.gameObject.SetActive(false);
                    UI_RoomOptions.gameObject.SetActive(false);

                    UI_RoomState.gameObject.SetActive(false);

                    isConnected = false;

                    break;
                }
            case IPCA.Networking.NetworkConState.Failed:
                {
                    UI_PhotonConnectionState.text = "Failed";
                    UI_PhotonConnectionState.color = Color.red;

                    UI_MainOptions.gameObject.SetActive(false);
                    UI_RoomOptions.gameObject.SetActive(false);
                    UI_RoomState.gameObject.SetActive(false);

                    UI_RoomOptions.interactable = true;
                    UI_RoomOptions.alpha = 1f;

                    isConnected = false;

                    break;
                }
            case IPCA.Networking.NetworkConState.Idle:
                {
                    UI_PhotonConnectionState.text = "Idle";
                    UI_PhotonConnectionState.color = Color.white;

                    UI_MainOptions.gameObject.SetActive(false);
                    UI_RoomOptions.gameObject.SetActive(false);
                    UI_RoomState.gameObject.SetActive(false);
                    isConnected = false;

                    break;
                }
        }

        switch(GameManager.Instance.networkManager.Room_ConnectionState)
        {
            case IPCA.Networking.NetworkConState.Failed:
                {
                    UI_MainOptions.gameObject.SetActive(false);
                    UI_RoomOptions.gameObject.SetActive(false);
                    UI_RoomState.gameObject.SetActive(false);

                    UI_RoomOptions.interactable = true;
                    UI_RoomOptions.alpha = 1f;

                    break;
                }
        }
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
}
