using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay;
using Gameplay.Weapons;
using Managers.Local;
using Managers.Network;
using RotaryHeart.Lib.PhysicsExtension;
using Unity.Netcode;
using UnityEngine;
using Physics = UnityEngine.Physics;

public class TheButtonWeapon : BaseWeapon
{
    [SerializeField] private float buttonHoldTime = 3f;
    [SerializeField] private float explosionRadius = 15;
    [SerializeField] private Transform buttonTransform;
    [SerializeField] private AnimationCurve damageFallOffCurve;
    
    private CancellationTokenSource _cancellationTokenSource;
    private float _currentTime;
    private bool _isHoldingDown;
    public override void AttackStart()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _ = ButtonCountdown(_cancellationTokenSource.Token);
        buttonTransform.localScale = new Vector3(1f, .1f, 1f);
        _isHoldingDown = true;
    }

    public override void AttackEnd()
    {
        buttonTransform.localScale = Vector3.one;
        _isHoldingDown = false;
        _cancellationTokenSource.Cancel();
    }

    public override void Start()
    {
        base.Start();
        Owner.OnDestroyed += OnDeath;
    }
    

    private async UniTask ButtonCountdown(CancellationToken token)
    {
        await UniTask.WaitForSeconds(buttonHoldTime, cancellationToken: token);
        /*/
        Debug.Log("start!");
        while (_currentTime < buttonHoldTime)
        {
            _currentTime += Time.deltaTime;
            Debug.Log("Current Time:" + _currentTime);
            buttonTransform.localScale = new Vector3(1, _currentTime / buttonHoldTime, 1);
            await UniTask.Yield();
        }
        /*/
        
        if (token.IsCancellationRequested)
        {
            return;
        }
        
        Explode_ServerRpc();
    }

    void OnDeath(ulong killer, int childID)
    {
        if (_isHoldingDown)
        {
            Explode_ServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void Explode_ServerRpc()
    {
        DamageProperties damageProperties = new DamageProperties(1000, transform.forward, Owner.OwnerClientId, Owner.ChildID.Value);

        Collider[] results = Physics.OverlapSphere(transform.position, explosionRadius, stats.HitLayers);

        #if UNITY_EDITOR
        
        DebugExtensions.DebugWireSphere(transform.position, Color.red, explosionRadius, 5, PreviewCondition.Both);
        
        #endif
        
        foreach (Collider col in results)
        {
            if (col.transform.parent && col.transform.parent.TryGetComponent(out BallPlayer ballPlayer))
            {
                float dist = (Owner.transform.position - col.transform.position).magnitude;
            
                Debug.Log("HIT: " + col.gameObject.name + " with a distance of: " + dist);
                
                ballPlayer.TakeDamage_ServerRpc(damageProperties);
            }
        }
        
        NetworkGameManager.Instance.PlayParticleGlobally_ServerRpc("Explosion", transform.position, transform.rotation);
        Owner.TakeDamage_ServerRpc(damageProperties);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BallPlayer>())
        {
           Explode_ServerRpc();
        }
    }
}
