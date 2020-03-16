using System;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public Sound[] sounds;
    private void Awake() {
        // Not implemented yet
        return;

        foreach(var sound in sounds) {
            sound.source = this.gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
        }
    }

    public void PlaySound(string name) {
        // Not implemented yet
        return;

        var sound = Array.Find(sounds, s => s.name == name);

        sound.source.Play();
    }
}
