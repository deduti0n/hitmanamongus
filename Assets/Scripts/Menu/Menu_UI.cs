using IPCA.Characters;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu_UI : baseActor
{
    [Header("Sus")]
    public CanvasGroup Killer_Root;

    [Header("Death")]
    public CanvasGroup Death_Root;

    [Header("Interact")]
    public CanvasGroup Interaction_Root;

    [Header("Task list")]
    public Text Tasks_List;
    public GameObject Task_Progress;

    [Header("Voting Menu")]
    public CanvasGroup VoteMenu;
    public GameObject VotingItemTemplate;
    public List<Transform> VoteItems = new List<Transform>();

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
        //Menu_OpenVotingMenu();
    }

    public void Menu_OpenVotingMenu(List<string> players)
    {
        GameManager.Instance.networkManager.Network_PlayerRef.Character_Lock = true;
        Cursor.lockState = CursorLockMode.None;
        VoteMenu.alpha = 1f;
        VoteMenu.interactable = true;

        //Skip button
        Button skip = VoteMenu.transform.Find("ButtonSkip").GetComponent<Button>();
        skip.onClick.RemoveAllListeners();

        skip.onClick.AddListener(delegate
        {
            //Send Vote
            if (GameManager.Instance.ingameManager)
                GameManager.Instance.ingameManager.MenuVote("");

            VoteMenu.interactable = false;
        });

        int i = 1;

        //Voting buttons
        foreach(Transform item in VoteItems)
        {
            if(i <= players.Count)
            {
                item.gameObject.SetActive(true);
                item.transform.Find("Label").GetComponent<TextMeshProUGUI>().text = players[i - 1];
                item.transform.Find("Votes").gameObject.SetActive(false);

                VoteMenu.interactable = true;
                string vote = players[i - 1];

                Button bt = item.transform.Find("Back").GetComponent<Button>();
                
                bt.onClick.RemoveAllListeners();

                bt.onClick.AddListener(delegate 
                {
                    //Send Vote
                    if(GameManager.Instance.ingameManager)
                        GameManager.Instance.ingameManager.MenuVote(vote);

                    item.transform.Find("Votes").gameObject.SetActive(true);
                    VoteMenu.interactable = false;
                });      
            }
            else
                item.gameObject.SetActive(false);

            i++;
        }
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

        if (!PlayerRef)
            return;

        if (PlayerRef && PlayerRef.targetInteraction)
            Interaction_Root.alpha = 1f;
        else
            Interaction_Root.alpha = 0f;

        UI_SetTaskProgressState(PlayerRef.char_duringTask);

        Death_Root.alpha = Mathf.Lerp(Death_Root.alpha, PlayerRef.Character_isDead ? 1f : 0f, 5f * Time.deltaTime);
        Killer_Root.alpha = Mathf.Lerp(Killer_Root.alpha, PlayerRef.Character_isSuspect ? 1f : 0f, 5f * Time.deltaTime);

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
