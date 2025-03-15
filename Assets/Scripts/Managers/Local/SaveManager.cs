using System;
using System.IO;
using System.Runtime.Serialization;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

namespace Managers.Local
{
    public static class SaveManager
    {

        private static readonly string DataPath = Application.persistentDataPath + '/'; 
        private static readonly string SaveDataDirectory = DataPath + "SaveData";
        private static readonly string FileExtention = ".dat";
        private static readonly DataContractSerializer Serializer = new(typeof(PlayerData));

        //TODO: Make better
        public static PlayerData MyBalls;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RunTimeInit()
        {
            MyBalls = null;
        }


        private static string GetCompleteDirectory(string file) => SaveDataDirectory + '/'+ file + FileExtention;
        
        public static async UniTask<PlayerData> LoadData()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                return CreateDefaultPlayerData("Brawller " + UnityEngine.Random.Range(0, 10000));
            }

            string name = await AuthenticationService.Instance.GetPlayerNameAsync();
            string str = GetCompleteDirectory(await AuthenticationService.Instance.GetPlayerNameAsync());
            
            if (!ValidateSaveFolder() ||  !ValidateSaveFile(str))
            {
                return CreateDefaultPlayerData(name);
            }

            PlayerData balls;
            try
            {
                await using FileStream reader = File.Open(str, FileMode.Open, FileAccess.Read, FileShare.Read);
                balls = Serializer.ReadObject(reader) as PlayerData;

            }
            catch (Exception e)
            {
                Debug.LogError("File Reading issue: "+ e);
                return CreateDefaultPlayerData(name);
            }

            return balls;
        }

        private static PlayerData CreateDefaultPlayerData(string username)
        {
            PlayerData p = new PlayerData();
            p.Username = username;
            BallStructure primaryBall = new BallStructure()
            {
                ball = "SoccerBall",
                ability = "Jump",
                weapon = "SpikeWeapon"
            };
            BallStructure secondary = new BallStructure()
            {
                ball = "PaintBall",
                ability = "Caltrops",
                //weapon = "LaserWeapon"
                weapon = "SpikeWeapon"
            };
            BallStructure tertiaryBall = new BallStructure()
            {
                ball = "CannonBall",
                ability = "Protect",
                weapon = "SpikeWeapon"
                //weapon = "RocketLauncherWeapon"
            };
            p.SetAllBalls(new[]
            {
                secondary,
                primaryBall,
                tertiaryBall
            });
            return p;
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
                return false;
            }
            return true;
        }

        [DataContract]
        public class PlayerData
        {
            [DataMember] public string Username;
            [DataMember] private BallStructure[] _balls;

            public bool HasChanges() => _hasChanges;

            [NonSerialized] private bool _hasChanges;
            
            public async UniTask SaveData()
            {
                Debug.Log("Beginning Save for player: "+ Username);
                if (!_hasChanges) return;
                
                ValidateSaveFolder();
            
                string str = GetCompleteDirectory(Username);
                ValidateSaveFile(str);
                try
                {
                    await using FileStream writer = File.Open(str, FileMode.Create, FileAccess.Write, FileShare.Write);
                    Serializer.WriteObject(writer, this);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to Save data: "+ e);
                }

                _hasChanges = false;
                
                Debug.Log("Save complete!");
            }

            public BallStructure GetReadonlyBall(int index) => _balls[index];

            public void SetBallType(int index, string ball)
            {
                if (string.Equals(_balls[index].ball, ball)) return;
                _balls[index].ball = ball;
                _hasChanges = true;
            }
            
            public void SetBallWeapon(int index, string weapon)
            {
                if (string.Equals(_balls[index].weapon, weapon)) return;
                _balls[index].weapon = weapon;
                _hasChanges = true;
            }
            
            public void SetBallAbility(int index, string ability)
            {
                if (string.Equals(_balls[index].ability, ability)) return;
                _balls[index].ability = ability;
                _hasChanges = true;
            }

            public void SetAllBalls(BallStructure[] ballStructures)
            {
                _balls = ballStructures;
                _hasChanges = true;
            }
        }

        [Serializable]
        public struct BallStructure
        {
            public string ball;
            public string weapon;
            public string ability;
        }
    }
}