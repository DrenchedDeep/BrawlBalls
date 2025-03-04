using Gameplay;
using UnityEngine;

namespace Managers.Controls
{
    public static class PlayerControls
    {
        /*
        private static GameControls _controls;
        private static BallPlayer _player;
            
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            _controls = new GameControls();
            _player = null;

            _controls.Game.EnterPause.performed += _ => PauseGame();
            _controls.Game.Ability.performed += ctx => _player!.TryDoAbility(ctx.ReadValueAsButton());
            _controls.Game.Weapon.performed += ctx => _player!.TryDoWeapon(ctx.ReadValueAsButton());
            _controls.Game.Steer.performed += ctx => _player!.SetSteer(ctx.ReadValue<Vector2>());
            
            
            _controls.UI.ExitPause.performed += _ => UnpauseGame();
        }

        public static void DisableControls()
        {
            _controls.Disable();
        }

        public static void EnableControls()
        {
            _controls.Enable();
        }

        private static void UnpauseGame()
        {
            _controls.Game.Enable();
            _controls.UI.Disable();
        }

        private static void PauseGame()
        {
            _controls.Game.Disable();
            _controls.UI.Enable();
            
            Debug.LogWarning("User tried pausing game, Integrate Pause Menu");
        }

        public static void BindOwner(BallPlayer owner)
        {
            _player = owner;
            
            _controls.Game.Enable();
            _controls.UI.Disable();
            
            Debug.Log($"I've bound this player to the local controls: {owner.name} ", owner);
        }*/
    }
}
