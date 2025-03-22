using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    
    public AudioSource click;
    public AudioSource win;
    public AudioSource music;
    public AudioSource timer;
    public AudioSource gameOver;
    
    public void PlayClick()
    {
        click.Play();
    }

    public void PlayWin()
    {
        win.Play();
    }
    
    public void PlayTimer()
    {
        timer.Play();
    }
    
    public void PlayGameOver()
    {
        gameOver.Play();
    }

    public void ToggleSound()
    {
        win.volume = win.volume == 0 ? 1 : 0;
        click.volume = click.volume == 0 ? 1 : 0;
        timer.volume = timer.volume == 0 ? 1 : 0;
        gameOver.volume = gameOver.volume == 0 ? 1 : 0;
    }

    public void ToggleMusic()
    {
        music.volume = music.volume == 0 ? 1 : 0;
    }

}
