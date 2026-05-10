using UnityEngine;

[System.Serializable] public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
}
public class MusicLibrary : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public MusicTrack[] tracks;
    public AudioClip GetTrack(string name)
    {
        foreach (var track in tracks)
        {
            if (track.trackName == name)
            {
                return track.clip;
            }
        }
        return null;
    }
}
