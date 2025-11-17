using UnityEngine;
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource splashSource;
    [SerializeField] AudioSource missSource;
    public AudioClip splashSound;
    public AudioClip missedSound;


    private void Start()
    {
        // musicSource.clip = backgroundMusic;
        // musicSource.Play();
    }

    public void PlaySplashSXF(AudioClip clip)
    {
        splashSource.PlayOneShot(clip);
    }

    public void PlayMissSXF(AudioClip clip)
    {
        missSource.PlayOneShot(clip);
    }


}
