using UnityEngine;

namespace Managers.Local
{
    public class PlayerBallInfo : MonoBehaviour
    {
        //private const string BallDirectory = "Balls/";
        //private const string AbilityDirectory = "SpecialAbilities/";
        //private const string WeaponDirectory = "Weapons/";
    
        public static string UserName;
    
        /*
    public static Ball GetBall(int index) => Resources.Load<Ball>(BallDirectory + Balls[index].Ball);
    public static Weapon GetWeapon(int index) => Resources.Load<Weapon>(WeaponDirectory + Balls[index].Weapon);
    public static AbilityStats GetAbility(int index) => Resources.Load<AbilityStats>(AbilityDirectory + Balls[index].Ability);
    */

        //public static Ball GetBall(string value) => Resources.Load<Ball>(BallDirectory + value);
        //public static Weapon GetWeapon(string value) => Resources.Load<Weapon>(WeaponDirectory + value);
        //public static AbilityStats GetAbility(string value) => Resources.Load<AbilityStats>(AbilityDirectory + value);

        // Start is called before the first frame update

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            UserName = "Brawller " + Random.Range(0, 10000);
        }

    }
}
