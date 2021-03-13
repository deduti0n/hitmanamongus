using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    Dictionary<string, AudioClip> AFX = new Dictionary<string, AudioClip>();
        
    public AudioManager()
    {
        //Load audio from resources
        foreach (AudioClip audio in Resources.LoadAll<AudioClip>("Audio/"))
            AFX.Add(audio.name, audio);

        Debug.Log("# Audio Manager loaded " + AFX.Count + " sounds.");
    }
        
    public AudioSource PlayClipAt(string ID, bool Loop)
    {
        AudioSource aSource = PlayClipAt(AFX[ID], Loop, "SoundClip");

        return aSource; // return the AudioSource reference
    }

    public AudioClip GetAudioClip(string ID)
    {
        return AFX[ID];
    }

    public AudioSource PlayClipAt(AudioClip clip, bool Loop, string name)
    {
        GameObject tempGO = new GameObject(name); // create the temp object
        tempGO.transform.position = Vector3.zero; // set its position
        AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
        aSource.clip = clip; // define the clip

        // set other aSource properties here, if desired
        aSource.spatialBlend = 0f;

        aSource.loop = Loop;

        aSource.Play(); // start the sound

        if (!Loop)
            Object.Destroy(tempGO, clip.length); // destroy object after clip duration

        return aSource; // return the AudioSource reference
    }

    public AudioSource PlayClipAt(Vector3 Position, string audioID, bool Loop, string name)
    {
        GameObject tempGO = new GameObject(name); // create the temp object
        tempGO.transform.position = Position; // set its position
        AudioSource aSource = tempGO.AddComponent<AudioSource>(); // add an audio source
        aSource.clip = AFX[audioID]; // define the clip

        // set other aSource properties here, if desired
        aSource.spatialBlend = 1f;

        aSource.loop = Loop;

        aSource.Play(); // start the sound

        if (!Loop)
            Object.Destroy(tempGO, AFX[audioID].length); // destroy object after clip duration

        return aSource; // return the AudioSource reference
    }
}