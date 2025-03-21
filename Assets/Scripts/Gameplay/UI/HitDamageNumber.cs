using System;
using Cysharp.Threading.Tasks;
using Gameplay.Pools;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class HitDamageNumber : PooledObject
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float timeToReturnToPool = 1f;
    [SerializeField] private Color[] colors;

    private static readonly int Show = Animator.StringToHash("Show");

    
    public void Init(int damage)
    {
        text.text = damage.ToString();
        text.color = colors[Random.Range(0, colors.Length)];
        animator.SetTrigger(Show);
        _ = ReturnToPoolTimer();
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }

    private async UniTask ReturnToPoolTimer()
    {
        await UniTask.WaitForSeconds(timeToReturnToPool);
        ReturnToPool();
    }
}
