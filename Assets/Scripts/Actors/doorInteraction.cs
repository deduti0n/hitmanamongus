using IPCA.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorInteraction : interactionActor
{
    Animator anima;

    protected override void onStart()
    {
        anima = GetComponent<Animator>();
    }

    public override void onUpdate()
    {
        //Automatic door opening behaviour when players are near
        List<Character_PlayerController> players = new List<Character_PlayerController>(
            GameManager.Instance.networkManager.Network_GetAllPlayerAvatars());

        bool isOpen = false;

        foreach(Character_PlayerController avatar in players.ToArray())
        {
            float distance = Vector3.Distance(transform.position, avatar.transform.position);

            if (distance < 5f)
                isOpen = true;
        }

        anima.SetBool("isOpen", isOpen);
    }
}
