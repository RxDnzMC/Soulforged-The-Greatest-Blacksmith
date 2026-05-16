using UnityEngine;

public class BossMusicController : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance?.PlayTrack("Boss (15 Minute)");
    }

    void OnDestroy()
    {
        MusicManager.Instance?.PlayTrack("Stage 1");
    }
}