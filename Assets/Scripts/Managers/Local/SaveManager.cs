using System;
using System.IO;
using System.Xml.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using Random = System.Random;

namespace Managers.Local
{
    public static class SaveManager
    {

        private static readonly string DataPath = Application.persistentDataPath + '/'; 
        private static readonly string SaveDataDirectory = DataPath + "SaveData";
        private static readonly string FileExtention = ".dat";
        private static readonly XmlSerializer Serializer = new(typeof(BallStructure[]));

        //TODO: Make better
        public static BallStructure[] MyBalls = Array.Empty<BallStructure>();

        public static string UserName;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RunTimeInit()
        {
            MyBalls = Array.Empty<BallStructure>();
            UserName = "Brawller " + UnityEngine.Random.Range(0, 10000);
        }


        private static string GetCompleteDirectory(string file) => SaveDataDirectory + '/'+ file + FileExtention;
        
        public static async UniTask<BallStructure[]> LoadData()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                return ResetPlayerData();
            }

            string str = GetCompleteDirectory(await AuthenticationService.Instance.GetPlayerNameAsync());
            
            if (!ValidateSaveFolder() ||  !ValidateSaveFile(str))
            {
                return ResetPlayerData();
            }

            BallStructure[] balls;
            try
            {
                await using FileStream reader = File.OpenRead(str);
                balls = Serializer.Deserialize(reader) as BallStructure[];

            }
            catch (Exception e)
            {
                Debug.LogError("File Reading issue: "+ e);
                return ResetPlayerData();
            }

            return balls;
        }

        public static async UniTask SaveData(BallStructure[] data)
        {
            ValidateSaveFolder();
            
            string str = GetCompleteDirectory(await AuthenticationService.Instance.GetPlayerNameAsync());
            ValidateSaveFile(str);
            try
            {
                await using FileStream writer = File.OpenWrite(str);
                Serializer.Serialize(writer, data);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to Save data: "+ e);
            }
        }
        
        public static BallStructure[] ResetPlayerData()
        {
            BallStructure primaryBall = new BallStructure()
            {
                Ball = "SoccerBall",
                Ability = "Jump",
                Weapon = "SpikeWeapon"
            };
            BallStructure secondary = new BallStructure()
            {
                Ball = "PaintBall",
                Ability = "Glue",
                Weapon = "LaserWeapon"
            };
            BallStructure tertiaryBall = new BallStructure()
            {
                Ball = "CannonBall",
                Ability = "Protect",
                Weapon = "RocketLauncherWeapon"
            };
            return new[]
            {
                secondary,
                primaryBall,
                tertiaryBall
            };
        }

        private static bool ValidateSaveFolder()
        {
            Debug.Log("Validating save directory: " + SaveDataDirectory);
            
            if (!Directory.Exists(SaveDataDirectory))
            {
                Directory.CreateDirectory(SaveDataDirectory);
                return false;
            }
            return true;
        }

        private static bool ValidateSaveFile(string str)
        {
            Debug.Log("Validating save file: " + str);

            if (!File.Exists(str))
            {
                File.Create(str);
                return false;
            }
            return true;
        }

        public struct BallStructure
        {
            public string Ball;
            public string Weapon;
            public string Ability;
        }
    }
}