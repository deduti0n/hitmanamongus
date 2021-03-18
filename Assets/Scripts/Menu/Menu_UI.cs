using IPCA.Characters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_UI : baseActor
{
    [Header("Interact")]
    public CanvasGroup Interaction_Root;

    [Header("Task list")]
    public Text Tasks_List;
    public GameObject Task_Progress;

    [Header("Voting Menu")]
    public CanvasGroup VoteMenu;
    public GameObject VotingItemTemplate;

    protected override void onStart()
    {
        GameManager.Instance.UI_Ref = this;
        VoteMenu.alpha = 0f;
        VoteMenu.interactable = false;
        UI_SetTaskProgressState(false);
    }

    public void UI_UpdateTaskList(string list)
    {
        Tasks_List.text = list;
    }

    public void UI_SetTaskProgressState(bool state)
    {
        Task_Progress.SetActive(state);
    }

    IEnumerator OpenMenu()
    {
        yield return new WaitForSeconds(1f);
        Menu_OpenVotingMenu();
    }

    public void Menu_OpenVotingMenu()
    {
        GameManager.Instance.networkManager.Network_PlayerRef.Character_Lock = true;
        Cursor.lockState = CursorLockMode.None;
        VoteMenu.alpha = 1f;
        VoteMenu.interactable = true;
    }

    public void Menu_CloseVotingMenu()
    {
        GameManager.Instance.networkManager.Network_PlayerRef.Character_Lock = false;

        Cursor.lockState = CursorLockMode.Locked;
        VoteMenu.alpha = 0f;
        VoteMenu.interactable = false;

        //Teleport to home base
    }

    public void Menu_CastVote()
    {
        //Empty cast
        VoteMenu.interactable = false;
    }

    public void Menu_CastVote(string player)
    {
        //Player cast
        Menu_CastVote();
    }

    public override void onUpdate()
    {
        Character_PlayerController PlayerRef = GameManager.Instance.networkManager.Network_PlayerRef;

        if (PlayerRef && PlayerRef.targetInteraction)
            Interaction_Root.alpha = 1f;
        else
            Interaction_Root.alpha = 0f;
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
