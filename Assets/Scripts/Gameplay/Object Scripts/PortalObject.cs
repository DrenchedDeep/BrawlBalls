using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Managers.Local;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Gameplay.Object_Scripts
{
    public class PortalObject : PlaceableObject
    {
        private PortalObject _boundPortal;

        private const int ChanceToGetLucky = 100; // ;) 1 in 100
        private static bool _someoneGotLucky;
        private bool _isOnCooldown;

        [SerializeField,Min(0)] private float portalDelay  = 1;
        [SerializeField] private int forcedIDX = -1;

        private static readonly List<PortalObject> Portals = new();
        private static readonly List<int> ActivePortalIds = new();
        private int _myId;
        private Material _myMat;

        protected override bool UseDelay => false;
    
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInit()
        {
            _someoneGotLucky = false;
        }


        private void Start()
        {
            Portals.Add(this);
        
            DecalProjector dp = GetComponent<DecalProjector>();
            _myMat = new Material(dp.material);
            dp.material = _myMat;
        
            if (forcedIDX != -1)
            {
                _myId = forcedIDX;
                _isOnCooldown = false;
                return;
            }

            _myId = Portals.Count - 1;
            _myMat.SetColor(StaticUtilities.ColorID, ParticleManager.GetRandomPrimaryColor);
            _myMat.SetColor(StaticUtilities.SecondaryColorID, ParticleManager.GetRandomSecondaryColor);
        

            switch (_myId)
            {
                case 2:
                    SetState(true);
                    _myMat.SetFloat(StaticUtilities.SpeedID, 0.5f);
                    break;
                //First two portals are used for special zone...
                case 3:
                
                    Portals[2].SetState(false);
                    SetState(false);
                    break;
                case > 3:
                    SetState(false);
                    break;
            }

            transform.eulerAngles = new Vector3(90, 0, 0); //???
        }

        public override void OnDestroy()
        {
            if(_boundPortal) Destroy(_boundPortal);
        }


    
        //TODO: While the owners camera catches up... So like, dist 8, the player is in the shadow realm.
        //Keep the player hidden until the camera is in range, and apply a screen space shader to that player.
        //Remove the effect and fire the player once in range...
        protected override void OnHit(BallPlayer hit)
        {
            print("hit ball: " + _isOnCooldown + ", " + ActivePortalIds.Count);
            if (_isOnCooldown || ActivePortalIds.Count <= 1) return;

            ActivePortalIds.Remove(_myId);

            int toID;
        

            if (!_someoneGotLucky && Random.Range(0, ChanceToGetLucky) == 0)
            {
                toID = 0;
                _someoneGotLucky = true;
            }
            else
            {
                toID = ActivePortalIds[Random.Range(0, ActivePortalIds.Count)];
            }

            //Choose another portal
            PortalObject outPortal = Portals[toID];
            Transform outPortalTrans = outPortal.transform;
            Vector3 direction = outPortalTrans.forward;

            hit.transform.GetChild(0).position = outPortalTrans.position + direction;

            hit.GetBall.ChangeVelocity(direction*hit.GetBall.Speed*-2f, ForceMode.Impulse, true);
        
            StartCooldown();
            PlayParticles();
        
            if (toID < 2) return;
            outPortal.StartCooldown();
            outPortal.PlayParticles();
        }




        private void PlayParticles()
        {
        
        }

        private void StartCooldown()
        {
            if (_isOnCooldown) return; // prevent issues where being called twice.
            _ = CoolDown();
            if(ActivePortalIds.Count == 1) Portals[ActivePortalIds[0]].StartCooldown();
        }

        private async UniTask CoolDown()
        {
            SetState(true);
            await UniTask.Delay((int)(portalDelay * 1000));
            SetState(false);
        }

        private void SetState(bool cooldown)
        {
            _isOnCooldown = cooldown;
            if (cooldown)
            {
                ActivePortalIds.Remove(_myId);
                _myMat.SetFloat(StaticUtilities.SpeedID, 0.5f);
            }
            else
            {
                ActivePortalIds.Add(_myId);
                _myMat.SetFloat(StaticUtilities.SpeedID, 3f);
            }
        
        }

    }
}
