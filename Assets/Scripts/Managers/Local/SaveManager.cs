using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Managers.Local
{
#if UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE_XBOXONE || UNITY_GAMECORE_SCARLETT || UNITY_SWITCH
     #define UNITY_CONSOLE
#endif
    
    public static class SaveManager
    {

        private static readonly string DataPath = Application.persistentDataPath + '/'; 
        private static readonly string SaveDataDirectory = DataPath + "SaveData";
        private static readonly string FileExtention = ".dat";
        private static readonly DataContractSerializer Serializer = new(typeof(PlayerData));

        private static Dictionary<string, PlayerData> _playerData;
        private static string GetCompleteDirectory(string file) => SaveDataDirectory + '/'+ file + FileExtention;


        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void RunTimeLoad()
        {
            _playerData = new();
        }

        public static bool TryGetPlayerData(string playerID, out PlayerData data) => _playerData.TryGetValue(playerID, out data);

        public static bool TryGetPlayerData(PlayerInput playerInput, out PlayerData data) => _playerData.TryGetValue(RetrieveClientNameFromPlayerInput(playerInput), out data);

        public static string GetCompiledNames(string delimiter = ";")
        {
            StringBuilder sb = new();
            int n = 0;
            foreach (var kvp in _playerData)
            {
                sb.Append(kvp.Key);
                if(++n <= _playerData.Count) sb.Append(delimiter);
            }
            return sb.ToString();
        }

        public static async UniTask SavePlayer(string user)
        {
            await _playerData[user].SaveData();
        }

        public static async UniTask SavePlayer(PlayerInput playerInput)
        {
            await SavePlayer(RetrieveClientNameFromPlayerInput(playerInput));
        }

        private static string RetrieveClientNameFromPlayerInput(PlayerInput playerInput)
        {
#if UNITY_CONSOLE
            return playerInput.user.platformUserAccountId;
#else
            
            //We should ideally do it based on player index, there's no other guaranteed' safe way.
            //But if we do it with player index, what happens if player 2 disconnects and then reconnects?
            //We need like a boolean table to make sure that an account is not currently logged in, or just to make sure the key isn't currently present.

            //It'd probably be best in the future to make a table.
            //We somehow need to know every players index.
            foreach (var kvp in _playerData)
            {
                if (playerInput == kvp.Value.LocalInput) return kvp.Value.Username;
            }
            Debug.LogWarning("We failed to find the player input: " , playerInput);
            return null;

            // Check to see if the name exists... See how many players are connected via the LocalMultiPlayer system... Retrieve the name from player prefs at that index?
            // We really should cache if possible.
            // If no name exists (or just set default) to generate a new name...
#endif
        }

        public static async UniTask SaveAllPlayers()
        {
            try
            {
                UniTask[] tasks = new UniTask[_playerData.Count];
                int i = 0;
                foreach (var pair in _playerData)
                {
                    tasks[i++] = pair.Value.SaveData();

                }

                await UniTask.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed while saving everyone" + e);
            }
        }

        private static int GetMinPlayer()
        {
            int minIndex = 0;
            foreach (var kvp in _playerData)
            {
                minIndex = Mathf.Max(minIndex, kvp.Value.PlayerIndex);
            }

            return minIndex + 1;
        }

        public static async UniTask<PlayerData> LoadPlayer(PlayerInput controller)
        {
            PlayerData data;
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                data =  CreateDefaultPlayerData(controller.user.platformUserAccountId);
                data!.LocalInput = controller;
                _playerData.Add(controller.user.platformUserAccountId,data);
                return data;
            }
            
            #if UNITY_CONSOLE
            Debug.Log("Trying new loading player method, using platformUserAccountId");
            string name = controller.user.platformUserAccountId;//await AuthenticationService.Instance.GetPlayerNameAsync();
            #else
            int minIndex = GetMinPlayer() ; 
            Debug.Log("Trying new loading player method, using randomName for index: " + minIndex);
            string name = PlayerPrefs.GetString("Player"+minIndex);//await AuthenticationService.Instance.GetPlayerNameAsync();

            #endif
            
            string str = GetCompleteDirectory(name);
            
            if (!ValidateSaveFolder() ||  !ValidateSaveFile(str))
            {
                data = CreateDefaultPlayerData(name);
                data!.LocalInput = controller;
                _playerData.Add(data.Username,data);
                return data;
            }

            try
            {
                await using FileStream reader = File.Open(str, FileMode.Open, FileAccess.Read, FileShare.Read);
                data = Serializer.ReadObject(reader) as PlayerData;
                data!.LocalInput = controller;
                if(!_playerData.TryAdd(data!.Username, data))
                    _playerData[data.Username] = data;
            }
            catch (Exception e)
            {
                Debug.LogError("File Reading issue: "+ e);
                data =  CreateDefaultPlayerData(name);
                data!.LocalInput = controller;
                _playerData.Add(data.Username, data);
            }
            return data;
        }

        private static PlayerData CreateDefaultPlayerData(string username)
        {

            int minIndex = GetMinPlayer();
            
            PlayerData p = new PlayerData
            {
                #if UNITY_CONSOLE
                Username = username,
                #else
                Username = PlayerPrefs.GetString("Player" + minIndex, GenerateSillyName()),
                PlayerIndex = minIndex
                #endif
            };
            
            #if !UNITY_CONSOLE
            PlayerPrefs.SetString("Player" +  p.PlayerIndex,p.Username);
            PlayerPrefs.Save();
            #endif
            
            BallStructure primaryBall = new BallStructure()
            {
                ball = "SoccerBall",
                ability = "Jump",
                weapon = "SpikeWeapon"
            };
            BallStructure secondary = new BallStructure()
            {
                ball = "PaintBall",
                ability = "Jump",
                weapon = "LaserWeapon"
            };
            BallStructure tertiaryBall = new BallStructure()
            {
                ball = "CannonBall",
                ability = "Protect",
                weapon = "RocketLauncherWeapon"
            };
            p.SetAllBalls(new[]
            {
                secondary,
                primaryBall,
                tertiaryBall
            });
            Debug.Log("Creating new default player: " + p.Username);

            
            return p;
        }
 
        public static string GenerateSillyName()
        {
            string[] adjectives = 
            { 
                "Happy", "Quirky", "Goofy", "Wobbly", "Jolly", "Bouncy", "Clumsy", "Silly", "Zany", "Cheeky", 
                "Nutty", "Lanky", "Wacky", "Giddy", "Dizzy", "Floppy", "Snappy", "Grumpy", "Loopy", "Wiggly" 
            };

            string[] verbs = 
            { 
                "Pink", "Glowing", "Omnipotent", "Soaring", "Bubbling", "Sizzling", "Mysterious", "Floating", "Radiant", "Shimmering", 
                "Thundering", "Dazzling", "Exploding", "Humming", "Sneaky", "Jiggling", "Zooming", "Melting", "Glistening", "Twinkling" 
            };

            string[] nouns = 
            { 
                "Lemon", "Squash", "Fox", "Goober", "Turnip", "Walrus", "Pickle", "Marshmallow", "Cactus", "Penguin", 
                "Pancake", "Banana", "Teapot", "Jellybean", "Toaster", "Muffin", "Octopus", "Spoon", "Meatball", "Sasquatch" 
            };
            return $"{adjectives[Random.Range(0, adjectives.Length)]}{verbs[Random.Range(0,verbs.Length)]}{nouns[Random.Range(0, nouns.Length)]}{Random.Range(0,1000)+1}";
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
            [NonSerialized] public PlayerInput LocalInput;

            #if !UNITY_CONSOLE
            [DataMember] public int PlayerIndex;
            #endif
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