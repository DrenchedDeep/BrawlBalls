using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.ActionMaps
{

    public enum ESpriteInputRequest
    {
        Click,
        Left,
        Right,
        Back,
        Select,
        QuickPlay,
        Ability,
        WeaponAbility
    }
    
    [CreateAssetMenu(fileName = "InputSpriteActionMap", menuName = "Scriptable Objects/InputSpriteActionMap")]
    public class InputSpriteActionMap : ScriptableObject
    {
        [field: SerializeField] public string  DisplayName { get; private set; }

        [field: Header("Buttons")]
        [field: SerializeField] public Sprite ClickIcon { get; private set; }
        [field: SerializeField] public Sprite RotateLeft { get; private set; }
        [field: SerializeField] public Sprite RotateRight { get; private set; }
        [field: SerializeField] public Sprite Back { get; private set; }
        
        [field: Header("Functional")]
        [field: SerializeField] public Sprite Select { get; private set; }
        [field: SerializeField] public Sprite QuickPlay { get; private set; }
        [field: SerializeField] public Sprite Ability { get; private set; }
        [field: SerializeField] public Sprite WeaponAbility { get; private set; }

        public Sprite GetSpriteByEnum(ESpriteInputRequest request) =>
            request switch
            {
                ESpriteInputRequest.Click => ClickIcon,
                ESpriteInputRequest.Left => RotateLeft,
                ESpriteInputRequest.Right => RotateRight,
                ESpriteInputRequest.Back => Back,
                ESpriteInputRequest.Select => Select,
                ESpriteInputRequest.QuickPlay => QuickPlay,
                ESpriteInputRequest.Ability => Ability,
                ESpriteInputRequest.WeaponAbility => WeaponAbility,
                _ => null
            };
    }
}
