using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorManager : baseManagerClass
{
    //Game actor references
    private List<baseActor> gameActors = new List<baseActor>();

    public ActorManager()
    {
        Debug.Log("Actor Manager initialized.");
    }

    public void AddActor(baseActor actor)
    {
        gameActors.Add(actor);
    }

    public void RemoveActor(baseActor actor)
    {
        gameActors.Remove(actor);
    }

    public override void onUpdate()
    {
        //Update actors
        for (int i = 0; i < gameActors.Count; i++)
            if (gameActors[i].enabled && gameActors[i].gameObject.activeSelf)
                gameActors[i].onUpdate();
    }

    public override void onLateUpdate()
    {
        //Update actors
        for (int i = 0; i < gameActors.Count; i++)
            if (gameActors[i].enabled && gameActors[i].gameObject.activeSelf)
                gameActors[i].onLateUpdate();
    }

    public override void onFixedUpdate()
    {
        //Update actors
        for (int i = 0; i < gameActors.Count; i++)
            if (gameActors[i].enabled && gameActors[i].gameObject.activeSelf)
                gameActors[i].onFixedUpdate();
    }

    public List<baseActor> GetNearbyActorsByType(ActorTypes type, Vector3 position, float MaxDistance)
    {
        List<baseActor> actors = new List<baseActor>();

        foreach(baseActor actor in gameActors)
        {
            if (actor.ActorType == type && Vector3.Distance(position, actor.transform.position) < MaxDistance && actor.enabled && actor.gameObject.activeSelf)
                actors.Add(actor);
        }

        actors = actors.OrderBy(
        x => Vector3.Distance(position, x.transform.position)
        ).ToList();
        
        return actors;
    }

    public List<baseActor> GetNearbyActorsByType(ActorTypes type, Vector3 position, float MaxDistance, bool isDead)
    {
        List<baseActor> actors = new List<baseActor>();

        foreach (baseActor actor in gameActors)
        {
            if (actor.ActorType == type && Vector3.Distance(position, actor.transform.position) < MaxDistance)
                actors.Add(actor);
        }

        return actors;
    }

    public bool isActorPlayer(GameObject targetActor)
    {
        foreach (baseActor actor in gameActors)
        {
            if (actor.gameObject == targetActor && actor.ActorType == ActorTypes.Player)
                return true;
        }

        return false;
    }
}