using IPCA.Characters;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Network_SyncPlayerController : MonoBehaviourPun, IPunObservable
{
    /// <summary>
    /// Custom component to sync my player controller more freely
    /// </summary>

    private Character_PlayerController PlayerRef;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!PlayerRef)
            return;

        //Syncing
        if (stream.IsWriting)
        {
            //Send player name
            stream.SendNext(GameManager.Instance.PlayerName);

            //Color
            stream.SendNext(PlayerRef.Character_GetColor());

            //Death state
            stream.SendNext(PlayerRef.Character_isDead);
        }
        else
        {
            //Receive player name
            PlayerRef.Character_SetName((string)stream.ReceiveNext());

            //Color
            PlayerRef.Character_SetColor((Vector3)stream.ReceiveNext());

            //Death state
            PlayerRef.Character_isDead = ((bool)stream.ReceiveNext());
        }
    }

    void Start()
    {
        PlayerRef = GetComponent<Character_PlayerController>();
    }
}
