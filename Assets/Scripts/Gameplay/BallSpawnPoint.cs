using Gameplay;
using UnityEngine;


//server only class, purpose is to make sure we dont spawn players at a spawnpoint that already has a player inside of it
public class BallSpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform point;
    public bool HasBallPlayer { get; private set; }

    public Transform Point => point;
    
    public void OnPlayerSpawned() => HasBallPlayer = true;
    public void ResetSpawnPoint() => HasBallPlayer = false;
}
