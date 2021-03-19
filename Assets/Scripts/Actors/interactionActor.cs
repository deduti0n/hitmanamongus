using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interactionActor : baseActor
{
    public string InteractionName = "";

    public bool isInteractible = true;

    public delegate void ExecutionQueue();
    public ExecutionQueue ExtraFunc;

    protected override void onStart()
    {

    }

    public virtual void Execute()
    {
        //Disable this component
        isInteractible = false;

        if(ExtraFunc != null)
            ExtraFunc();

        ExtraFunc = null;
    }
}
