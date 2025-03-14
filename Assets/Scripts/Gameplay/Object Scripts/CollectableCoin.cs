using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

namespace Gameplay.Object_Scripts
{
    public class CollectableCoin : Collectable
    {
        [SerializeField] private float collectionTime;
        [SerializeField] private TextMeshPro text;
        [SerializeField] private PositionConstraint constraint;
        [SerializeField] private Collider col;
        private BallPlayer _owner;
        
        private CancellationTokenSource _cancellationToken;

        private Vector3 _defaultPosition;


        private void Awake()
        {
            _defaultPosition = transform.position;
        }

        private async UniTask CoinTimer(CancellationToken token)
        {
            Debug.LogWarning("A Coin/Token/Gem was collected. We need to implement some kind of cancel function if the player is killed before the time completes");

            float ct = 0;
            col.enabled = false;
            text.gameObject.SetActive(true);
            while (ct < collectionTime && _owner ) // While we have an owner and we're in time...
            {
                ct += Time.deltaTime;
                text.text = (collectionTime - ct).ToString("F1");
                text.transform.LookAt(Camera.main.transform);
                //text.transform.LookAt(); Look at local players ball
                await UniTask.Yield();
            }

            if (IsOwner && _owner)
            {
                Award(_owner);
            }
            else
            {
                col.enabled = true;
                constraint.SetSources(null);
                text.gameObject.SetActive(false);
            }
            
        }

    

        protected override void OnTriggerEnter(Collider other)
        {
            other.transform.parent.TryGetComponent(out _owner);
            
            //What if the owner dies? We probably need to cache a cancellation token.

            _owner.OnDestroyed += OnOwnerKilled;
            _cancellationToken = new CancellationTokenSource();
            _ = CoinTimer(_cancellationToken.Token); 

            
            //Let's bind ourselves to the player object, so we must move with them.
            ConstraintSource s = new ConstraintSource
            {
                sourceTransform = _owner.transform,
                weight = 1
            };
            
            constraint.constraintActive = true;
            constraint.SetSources(new List<ConstraintSource>(){s});
        }
        
        private void OnOwnerKilled(ulong killer)
        {
            if (_owner)
            {
                _owner.OnDestroyed -= OnOwnerKilled;
            }

            //if world killed the user, reset to origin
            if (killer == 100)
            {
                transform.position = _defaultPosition;
            }
            
            OnOwnerKilled_ClientRpc();
        }

        [ClientRpc]
        private void OnOwnerKilled_ClientRpc()
        {
            _cancellationToken.Cancel();
            constraint.constraintActive = false;
            text.gameObject.SetActive(false);
            constraint.SetSources(new List<ConstraintSource>(){});
        }
    }
}
