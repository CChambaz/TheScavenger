using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundWolfManager : MonoBehaviour
{

    private FMOD.Studio.EventInstance playWolf;

    public enum SoundWolf
    {
        Attack
    }

    public void Play(SoundWolf sound)
    {
        switch (sound)
        {
            case SoundWolf.Attack:
                playWolf = FMODUnity.RuntimeManager.CreateInstance("event:/SoundEffects/Characters/Wolf/Wolf - Attack");
                break;
        }
        playWolf.start();
    }
}
