using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayerManager : MonoBehaviour
{
    private FMOD.Studio.EventInstance playPlayer;

    public enum SoundPlayer
    {
        Attack,
        Dash
    }

    public void Play(SoundPlayer sound)
    {
        switch (sound)
        {
            case SoundPlayer.Attack:
                playPlayer = FMODUnity.RuntimeManager.CreateInstance("event:/SoundEffects/Axe/Axe");
                break;

            case SoundPlayer.Dash:
                playPlayer = FMODUnity.RuntimeManager.CreateInstance("event:/SoundEffects/Characters/Player/WOOOSH");
                break;
        }
        playPlayer.start();
    }
}
