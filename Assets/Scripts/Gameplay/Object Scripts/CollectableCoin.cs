using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.Balls;
using TMPro;
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
        private NetworkBall _owner;
        
        private async UniTask CoinTimer()
        {
            Debug.LogWarning("A Coin/Token/Gem was collected. We need to implement some kind of cancel function if the player is killed before the time completes");

            float ct = 0;
            col.enabled = false;
            text.gameObject.SetActive(true);
            while (ct < collectionTime && _owner ) // While we have an owner and we're in time...
            {
                ct += Time.deltaTime;
                text.text = (collectionTime - ct).ToString("F1");
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
            _ = CoinTimer();
            
            //Let's bind ourselves to the player object, so we must move with them.
            ConstraintSource s = new ConstraintSource
            {
                sourceTransform = _owner.transform
            };
            constraint.constraintActive = true;
            constraint.SetSources(new List<ConstraintSource>(){s});
        }
    }
}
