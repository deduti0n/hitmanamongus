using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActorTypes
{
    Default,
    Player,
    Interaction,
    Other,
}

[System.Serializable]
public class baseActor : MonoBehaviour
{
    [Header("Actor settings")]
    public ActorTypes ActorType = ActorTypes.Default;

    [HideInInspector]
    public bool isBeingNoisy = false;


    private void Awake()
    {
        //Initialize actor
        GameManager.Instance.actorManager.AddActor(this);
        onStart();
    }

    protected virtual void onStart()
    {

    }

    public virtual void onUpdate()
    {
        
    }

    public virtual void onFixedUpdate()
    {

    }

    public virtual void onLateUpdate()
    {

    }

    protected virtual void onDestroy()
    {

    }

    private void OnDestroy()
    {
        //Unitialize actor
        GameManager.Instance.actorManager.RemoveActor(this);
        onDestroy();
    }


    protected GameObject Actor_GetChildGameObject(string withName)
    {
        Transform[] ts = transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}
