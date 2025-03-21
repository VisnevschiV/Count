using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    public AudioSource click;
    public AudioSource win;
    public AudioSource music;
    
    public void PlayClick()
    {
        click.Play();
    }

    public void PlayWin()
    {
        win.Play();
    }

    public void ToggleSound()
    {
        win.volume = win.volume == 0 ? 1 : 0;
        click.volume = click.volume == 0 ? 1 : 0;
    }

    public void ToggleMusic()
    {
        music.volume = music.volume == 0 ? 1 : 0;
    }

}
