using UnityEngine;

public class PlayerBallInfo : MonoBehaviour
{
    public static PlayerBallInfo Instance { get; private set; }
    public readonly BallStructure[] Balls = new BallStructure[3];

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance) Destroy(gameObject);
        Instance = this;

        for (int i = 0; i < Balls.Length; ++i)
        {
            Balls[i].Ball = PlayerPrefs.GetString("Ball" + i);
            Balls[i].Weapon = PlayerPrefs.GetString("Weapon" + i);
            Balls[i].Ability = PlayerPrefs.GetString("Ability" + i);
        }
    }

    private void UpdateSaveData(int index)
    {
        PlayerPrefs.SetString("Ball"+index, Balls[index].Ball);
        PlayerPrefs.SetString("Weapon"+index, Balls[index].Ball);
        PlayerPrefs.SetString("Ability"+index, Balls[index].Ball);
        PlayerPrefs.Save();
    }


    public struct BallStructure
    {
        public string Ball;
        public string Weapon;
        public string Ability;
    }
}
