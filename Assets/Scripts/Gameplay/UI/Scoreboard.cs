using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MainMenu.UI;
using Managers.Local;
using Managers.Network;
using TMPro;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class Scoreboard : MonoBehaviour
    {
        private readonly ScoreHolders[] _holders = new ScoreHolders[8];
        private int _playerCount;
        
        private static Color _myColor;
        private static Color _otherColor;
        
        [SerializeField] private Color myColor;
        [SerializeField] private Color otherColor;

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInit()
        {
            _myColor = Color.black;
            _otherColor =  Color.black;
        }

        void Awake()
        {
            if (!NetworkManager.Singleton)
            {
                Debug.LogWarning("No NetworkManager, disabled scoreboard", gameObject);
                gameObject.SetActive(false);
                return;
            }
            //Store all objects & Reset.
            _playerCount = 0;
            for (int i = 0; i < _holders.Length; ++i)
            {
                Debug.Log("This will likely have issues once we do local multiplayer.");
                
                _holders[i] = new ScoreHolders(transform.GetChild(i), NetworkManager.Singleton.LocalClientId);
                _holders[i].Disable();
            }
            _myColor = myColor;
            _otherColor = otherColor;
        }

        private void OnEnable()
        {
            NetworkGameManager.Instance.OnPlayerListUpdated += Initialize;
            NetworkGameManager.Instance.OnPlayerScoreUpdated += UpdateScore;
        }

        private void OnDisable()
        { 
            NetworkGameManager.Instance.OnPlayerListUpdated -= Initialize;
            NetworkGameManager.Instance.OnPlayerScoreUpdated -= UpdateScore;
        }
        
        private void Initialize()
        {
            transform.parent.gameObject.SetActive(true);
            print("Refreshing Scoreboard:");

            foreach (ScoreHolders scoreHolder in _holders)
            {
                scoreHolder.Disable();
            }
            
            List<BallPlayerInfo> players = new List<BallPlayerInfo>();

            foreach (BallPlayerInfo player in NetworkGameManager.Instance.Players)
            {
                players.Add(player);
            }
        
            List<BallPlayerInfo> sortedPlayers = players
                .OrderByDescending(p => p.Score) 
                .Take(_holders.Length) 
                .ToList();

            for(int i = 0; i < sortedPlayers.Count; ++i)
            {
                BallPlayerInfo ballPlayerInfo = sortedPlayers[i];
                _holders[i].ChangeTo(ballPlayerInfo.Username.ToString(), ballPlayerInfo.Score, ballPlayerInfo.ClientID, 0);
            }
            
            /*/
            
            foreach (var ball in NetworkGameManager.Instance.Players)
            {
              //  InitializeServerRpc(ball.Username.ToString());
                 AddPlayerToScoreboard(ball);
            }
            /*/
            
        }

        private void AddPlayerToScoreboard(BallPlayerInfo ballPlayerInfo)
        {
            List<BallPlayerInfo> players = new List<BallPlayerInfo>();

            foreach (BallPlayerInfo player in NetworkGameManager.Instance.Players)
            {
                players.Add(player);
            }
        
            List<BallPlayerInfo> sortedPlayers = players
                .OrderByDescending(p => p.Score) 
                .Take(_holders.Length) 
                .ToList();

            for(int i = 0; i < _holders.Length; ++i)
            {
                
                _holders[i].ChangeTo(ballPlayerInfo.Username.ToString(), ballPlayerInfo.Score, ballPlayerInfo.ClientID, 0);
            }
            
            
            /*/
            _holders[_playerCount++].ChangeTo(ballPlayerInfo.Username.ToString(), ballPlayerInfo.Score, ballPlayerInfo.ClientID, 0);
            /*/
        }
        
        private void UpdateScore(ulong playerID, int change)
        {
            for (int index = 0; index < _holders.Length; index++)
            {
                ScoreHolders current = _holders[index];
                if (current.Id == playerID)
                {
                    print($"Updating score: {(current.Value + change)} for ID: {playerID}");

                    current.ModifyScoreHolder(current.Value + change, index + 1);

                    /*/
                    //Length back down
                    while (--index >= 0)
                    {  
                        ScoreHolders above = _holders[index];
                        if (current.Value > above.Value)
                        {
                        //    _holders[index + 1] = above;
                         //   _holders[index] = current;
                            
                 
                          //  _holders[index + 1].Refresh();
                            //_holders[index].Refresh();
                        }
                    }
                    /*/
                    
                    for (int i = 0; i < _holders.Length; i++)
                    {
                        if (_holders[i + 1].Value > _holders[i].Value)
                        {
                            Debug.Log(_holders[i + 1].PlayerName + " has more score then " + _holders[i].PlayerName + "swapping them"); 
                            
                            string tempName = _holders[i].PlayerName;
                            float tempScore = _holders[i].Value;
                            ulong tempId = _holders[i].Id;
                            int tempValue = _holders[i].Value;

                            _holders[i].ChangeTo(_holders[i + 1].PlayerName, _holders[i + 1].Value, _holders[i + 1].Id, _holders[i + 1].Value);
                            _holders[i + 1].ChangeTo(tempName, tempScore, tempId, tempValue);

                        }
                    }
                    
                    return;
                }
            }
            
        }
        
        /*/

        [ClientRpc]
        private void RemoveUserFromScoreboardClientRpc(ulong id)
        {
            //Iterate from the top index, to the amount of players in the game.
            for (int index = 0; index < _playerCount; index++)
            {
                //Once we found the player we're looking for.
                if ( _holders[index].Id == id)
                {
                    //Iterate through the remaining slots -1 for the removed slot
                    while (index < _playerCount-1)
                    {
                        //And change the current one, to the next one.
                        _holders[index].ChangeTo(_holders[index + 1]);
                        ++index;
                    }
                    //Disable the last slot.
                    _holders[_playerCount - 1].Disable();
                    return;
                }
            }
        }

        [ClientRpc]
        private void AddUserToScoreboard_ClientRpc(ulong id, string playerName)
        {
            print("adding user to scoreboard: " + playerName);
            _holders[_playerCount++].ChangeTo(playerName, id, 0); //TODO: value will be given by game manager in the future
        }
        
        
        //Update scores using a bubble up approach.
        [ServerRpc(RequireOwnership = false)]
        public void UpdateScore_ServerRpc(ulong playerID, int change)
        {
            UpdateScore_ClientRpc(playerID, change);
        }


        [ClientRpc]
        private void UpdateScore_ClientRpc(ulong playerID, int change)
        {

            for (int index = 0; index < _holders.Length; index++)
            {
                ScoreHolders current = _holders[index];
                if (current.Id == playerID)
                {
                    print($"Updating score: {(current.Value + change)} for ID: {playerID}");

                    current.ModifyScoreHolder(current.Value + change, index + 1);

                    //Length back down
                    while (--index >= 0)
                    {
                        ScoreHolders above = _holders[index];
                        if (current.Value > above.Value)
                        {
                            //SWAP
                            _holders[index + 1] = above;
                            _holders[index] = current;
                        }
                    }

                    return;
                }
            }
        }
    

        

        //Anyone can call this.
        [ServerRpc(RequireOwnership = false)]
        private void InitializeServerRpc(string userName, ServerRpcParams caller = default)
        {
        
            print("Initializing Scoreboard locally");
            AddUserToScoreboard_ClientRpc(caller.Receive.SenderClientId, userName);
            //NetworkManager.Singleton.OnClientConnectedCallback += ctx => AddUserToScoreboardClientRpc(ctx, PlayerBallInfo.UserName);
            NetworkManager.Singleton.OnClientDisconnectCallback += RemoveUserFromScoreboardClientRpc;
        }
        /*/

        #region ScoreHolderInfo
        private class ScoreHolders
        {
        
            public int Value { get; private set; }
            public ulong InitialId { get; private set; }
            public ulong Id { get; private set; }
            public float Score { get; private set; }
            private string _playerName;
            
            public string PlayerName => _playerName;
        
            private readonly Image _root;
            private readonly TextMeshProUGUI _scoreText;
            private readonly TextMeshProUGUI _rankText;
            private readonly TextMeshProUGUI _nameText;

            public ScoreHolders(Transform root, ulong initialID)
            {
                Value = 0;
                Id = 0;
                InitialId = initialID;
                _root = root.GetComponent<Image>();
                _rankText = root.GetChild(0).GetComponent<TextMeshProUGUI>();
                _nameText = root.GetChild(1).GetComponent<TextMeshProUGUI>();
                _scoreText = root.GetChild(2).GetComponent<TextMeshProUGUI>();
                _playerName = null;
            }

            public void ChangeTo(string playerName, float score, ulong id, int value)
            {
                _root.gameObject.SetActive(true);
                _playerName = playerName;
                _nameText.text = playerName;
                ModifyScoreHolder((int)score, _root.transform.GetSiblingIndex()+1);
                
                _root.color =  id == NetworkManager.Singleton.LocalClientId ? _myColor : _otherColor; 
                
                Id = id;
            }

            public void Refresh()
            {
                Debug.Log($"Refreshing: ID {Id}, Name {PlayerName}, Score {Value}");

                _nameText.text = PlayerName;
            }

            public void ChangeTo(ScoreHolders other)
            {

                ChangeTo(other._playerName, other.Score, other.Id, other.Value);
            }


            public void ModifyScoreHolder(int newValue, int rank)
            {
                Value = newValue;
                _rankText.text = $"{rank}.";
                _scoreText.text = newValue.ToString();
            }


            public void Disable()
            {
                _root.gameObject.SetActive(false);
            }
        }
        #endregion
    
    }
}
