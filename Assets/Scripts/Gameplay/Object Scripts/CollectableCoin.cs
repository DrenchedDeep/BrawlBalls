using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

public class CollectableCoin : Collectable
{
    [SerializeField] private float collectionTime;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private PositionConstraint constraint;
    [SerializeField] private Collider col;
    private Transform owner;
    private IEnumerator CoinTimer()
    {
        float ct = 0;
        col.enabled = false;
        text.gameObject.SetActive(true);
        while (ct < collectionTime && owner ) // While we have an owner and we're in time...
        {
            ct += Time.deltaTime;
            text.text = (collectionTime - ct).ToString("F1");
            //text.transform.LookAt(); Look at local players ball
            yield return null;
        }

        if (owner && IsOwner)
        {
            Award(owner.parent.GetComponent<Ball>());
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
        owner = other.transform;
        StartCoroutine(CoinTimer());
        ConstraintSource s = new ConstraintSource
        {
            sourceTransform = owner
        };
        constraint.constraintActive = true;
        constraint.SetSources(new List<ConstraintSource>(){s});

    }
}
