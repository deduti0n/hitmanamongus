using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terminalInteraction : interactionActor
{
    protected override void onStart()
    {

    }

    public override void Execute()
    {
        base.Execute();

        GameManager.Instance.networkManager.Network_PlayerRef.Character_StartTask();

        //UI loading task timer and mark as complete
        isInteractible = false;
    }
}
