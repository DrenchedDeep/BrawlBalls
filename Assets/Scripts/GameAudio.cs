using Managers.Local;
using Managers.Network;
using UnityEngine;

/// <summary>
/// THIS IS FOR THE GAME AUDIO -- CAN BE FOUND IN THE MANAGERS PREFAB ON THE NETWORKGAMEMANAGER, ALL OF THIS IS REPLICATED SO EVERYBODY WILL PLAY THEIR OWN COPY OF THE AUDIO
/// </summary>
///
public class GameAudio : MonoBehaviour
{
    private void OnEnable()
    {
        NetworkGameManager.Instance.OnGameReachedOverTime += OnGameReachedOverTime;
        NetworkGameManager.Instance.OnGameCountdownDecremented += OnGameStartCountdownDecremented;

    }

    private void OnDisable()
    {
        NetworkGameManager.Instance.OnGameReachedOverTime -= OnGameReachedOverTime;
        NetworkGameManager.Instance.OnGameCountdownDecremented -= OnGameStartCountdownDecremented;
    }

    /// <summary>
    /// CALLED WHEN THE GAME HAS REACHED OVERTIME, EVERY CLIENT WILL PLAY THIS AUDIO SO JUST DO IT NORMALLY
    /// </summary>
    private void OnGameReachedOverTime()
    {
        AudioManager.instance.TriggerGameState(1);
    }
    
    /// <summary>
    /// IF THIS VALUE IS 0, IT MEANS THE TEXT IS SAYING "GO", OTHERWISE IT WILL BE 3,2,1... GO!
    /// </summary>
    /// <param name="value"></param>
    private void OnGameStartCountdownDecremented(int value)
    {
        if (value == 0)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Beep[1], transform.position);
        }
        else if (value > 0 && value < 4)
        {
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Beep[0], transform.position);
        }
    }
}
