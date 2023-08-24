using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : NetworkBehaviour
{
    //Being static, means as long as the game doesn't close, you can see who was in the lobby.
    private static readonly ScoreHolders[] Holders = new ScoreHolders[8];
    private static int _playerCount;
    private static ulong _myID;

    [SerializeField] private Color myColor;
    [SerializeField] private Color otherColor;

    private static Color _myColor;
    private static Color _otherColor;
    
    
    void Start()
    {
        _myColor = myColor;
        _otherColor = otherColor;
        
        //Store all objects & Reset.
        _playerCount = 0;
        for (int i = 0; i < Holders.Length; ++i)
        {
            Holders[i] = new ScoreHolders(transform.GetChild(i));
            Holders[i].Disable();
        }

        _myID = NetworkManager.Singleton.LocalClientId;

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ctx =>
                AddUserToScoreboardClientRpc(ctx, AuthenticationService.Instance.PlayerName);
            NetworkManager.Singleton.OnClientDisconnectCallback += RemoveUserFromScoreboardClientRpc;
        }
        //Unfortunate
        transform.parent.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void RemoveUserFromScoreboardClientRpc(ulong id)
    {
        //Iterate from the top index, to the amount of players in the game.
        for (int index = 0; index < _playerCount; index++)
        {
            //Once we found the player we're looking for.
            if ( Holders[index].Id == id)
            {
                //Iterate through the remaining slots -1 for the removed slot
                while (index < _playerCount-1)
                {
                    //And change the current one, to the next one.
                    Holders[index].ChangeTo(Holders[index + 1]);
                    ++index;
                }
                //Disable the last slot.
                Holders[_playerCount - 1].Disable();
                return;
            }
        }
    }

    [ClientRpc]
    private void AddUserToScoreboardClientRpc(ulong id, string playerName)
    {
        print("adding user to scoreboard: " + playerName);
        Holders[_playerCount++].ChangeTo(playerName, id, 0); //TODO: value will be given by game manager in the future
    }


    //Update scores using a bubble up approach.
    public static void UpdateScore(ulong playerID, int newScore)
    {
        for (int index = 0; index < Holders.Length; index++)
        {
            ScoreHolders current = Holders[index];
            if ( current.Id == playerID)
            {
                
                //Length back down
                while (index-- > 0)
                {
                    ScoreHolders above = Holders[index];
                    if (current.Value > above.Value)
                    {
                        //SWAP
                        Holders[index+1] = above;
                        Holders[index] = current;
                    }
                }
                current.UpdateScore(newScore, index+1);
                return;
            }
        }
    }


    public void Initialize()
    {
        transform.parent.gameObject.SetActive(true);
        InitializeServerRpc(AuthenticationService.Instance.PlayerName);
    }

    //Anyone can call this.
    [ServerRpc(RequireOwnership = false)]
    private void InitializeServerRpc(string userName, ServerRpcParams caller = default)
    {
        
        print("Initializing Scoreboard locally");
        AddUserToScoreboardClientRpc(caller.Receive.SenderClientId, userName);
    }

    #region ScoreHolderInfo
    private struct ScoreHolders
    {
        
        public int Value { get; private set; }
        public ulong Id { get; private set; }
        private string _playerName;
        
        private readonly Image _root;
        private readonly TextMeshProUGUI _scoreText;
        private readonly TextMeshProUGUI _rankText;
        private readonly TextMeshProUGUI _nameText;

        public ScoreHolders(Transform root)
        {
            Value = 0;
            Id = 0;
            _root = root.GetComponent<Image>();
            _rankText = root.GetChild(0).GetComponent<TextMeshProUGUI>();
            _nameText = root.GetChild(1).GetComponent<TextMeshProUGUI>();
            _scoreText = root.GetChild(2).GetComponent<TextMeshProUGUI>();
            _playerName = null;
        }

        public void ChangeTo(string playerName, ulong id, int value)
        {
            _root.gameObject.SetActive(true);
            _playerName = playerName;
            _nameText.text = playerName;
            UpdateScore(value, _root.transform.GetSiblingIndex()+1);

            _root.color = id == _myID ? _myColor : _otherColor; 
                
            Id = id;
        }

        public void ChangeTo(ScoreHolders other)
        {
            ChangeTo(other._playerName, other.Id, other.Value);
        }


        public void UpdateScore(int newValue, int rank)
        {
            Value = newValue;
            _rankText.text = rank + ".";
            _scoreText.text = newValue.ToString();
        }


        public void Disable()
        {
            _root.gameObject.SetActive(false);
        }
    }
    #endregion
    
}
