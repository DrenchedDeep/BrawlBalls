using UnityEngine;

public class PlayerBallInfo : MonoBehaviour
{
    //private const string BallDirectory = "Balls/";
    //private const string AbilityDirectory = "SpecialAbilities/";
    //private const string WeaponDirectory = "Weapons/";
    
    public static readonly BallStructure[] Balls = new BallStructure[3];
    
    /*
    public static Ball GetBall(int index) => Resources.Load<Ball>(BallDirectory + Balls[index].Ball);
    public static Weapon GetWeapon(int index) => Resources.Load<Weapon>(WeaponDirectory + Balls[index].Weapon);
    public static AbilityStats GetAbility(int index) => Resources.Load<AbilityStats>(AbilityDirectory + Balls[index].Ability);
    */

    //public static Ball GetBall(string value) => Resources.Load<Ball>(BallDirectory + value);
    //public static Weapon GetWeapon(string value) => Resources.Load<Weapon>(WeaponDirectory + value);
    //public static AbilityStats GetAbility(string value) => Resources.Load<AbilityStats>(AbilityDirectory + value);

    // Start is called before the first frame update
    void Awake()
    {

        //if (!PlayerPrefs.HasKey("Ball0"))
        {
            GenerateDefaults();
        }

        
        for (int i = 0; i < Balls.Length; ++i)
        {
            Balls[i].Ball = PlayerPrefs.GetString("Ball" + i);
            Balls[i].Weapon = PlayerPrefs.GetString("Weapon" + i);
            Balls[i].Ability = PlayerPrefs.GetString("Ability" + i);
        }
        
    }

    private void GenerateDefaults()
    {
        Balls[0].Ball = "SoccerBall";
        Balls[1].Ball = "PaintBall";
        Balls[2].Ball = "CannonBall";
        
        Balls[0].Ability = "Jump";
        Balls[1].Ability = "Glue";
        Balls[2].Ability = "Protect";
        
        Balls[0].Weapon = "Spike";
        Balls[1].Weapon = "Beam";
        Balls[2].Weapon = "Abductor";
        UpdateSaveData(0);
        UpdateSaveData(1);
        UpdateSaveData(2);
    }

   
    
    

    private void UpdateSaveData(int index)
    {
        PlayerPrefs.SetString("Ball"+index, Balls[index].Ball);
        PlayerPrefs.SetString("Weapon"+index, Balls[index].Weapon);
        PlayerPrefs.SetString("Ability"+index, Balls[index].Ability);
        PlayerPrefs.Save();
    }


    public struct BallStructure
    {
        public string Ball;
        public string Weapon;
        public string Ability;
    }
}
