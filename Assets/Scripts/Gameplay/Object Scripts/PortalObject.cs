using System.Collections;
using System.Collections.Generic;
using Gameplay.Object_Scripts;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class PortalObject : PlaceableObject
{
    private PortalObject boundPortal;

    private const int ChanceToGetLucky = 100; // ;) 1 in 100
    private static bool _someoneGotLucky;
    private bool isOnCooldown;

    private static readonly WaitForSeconds CoolDownDuration = new (3);

    [SerializeField] private int forcedIDX = -1;

    private static readonly List<PortalObject> Portals = new();
    private static readonly List<int> ActivePortalIds = new();
    private int myId;
    private Material myMat;

    protected override bool UseDelay => false;


    private void Start()
    {
        Portals.Add(this);
        
        DecalProjector dp = GetComponent<DecalProjector>();
        myMat = new Material(dp.material);
        dp.material = myMat;
        
        if (forcedIDX != -1)
        {
            myId = forcedIDX;
            isOnCooldown = false;
            return;
        }

        myId = Portals.Count - 1;


        

        myMat.SetColor(ParticleManager.ColorID, ParticleManager.GetRandomPrimaryColor);
        myMat.SetColor(ParticleManager.SecondaryColorID, ParticleManager.GetRandomSecondaryColor);
        

        switch (myId)
        {
            case 2:
                SetState(true);
                myMat.SetFloat(ParticleManager.SpeedID, 0.5f);
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
    }
    
    private void OnDestroy()
    {
        if(boundPortal) Destroy(boundPortal);
    }


    
    //TODO: While the owners camera catches up... So like, dist 8, the player is in the shadow realm.
    //Keep the player hidden until the camera is in range, and apply a screen space shader to that player.
    //Remove the effect and fire the player once in range...
    protected override void OnHit(Ball hit)
    {
        print("hit ball: " + isOnCooldown + ", " + ActivePortalIds.Count);
        if (isOnCooldown || ActivePortalIds.Count <= 1) return;

        ActivePortalIds.Remove(myId);

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

        hit.ChangeVelocity(direction*hit.Speed*2f, ForceMode.Impulse, true);
        
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
        if (isOnCooldown) return; // prevent issues where being called twice.
        StartCoroutine(CoolDown());
        if(ActivePortalIds.Count == 1) Portals[ActivePortalIds[0]].StartCooldown();
    }

    private IEnumerator CoolDown()
    {
        SetState(true);
        yield return CoolDownDuration;
        SetState(false);
    }

    private void SetState(bool cooldown)
    {
        isOnCooldown = cooldown;
        if (cooldown)
        {
            ActivePortalIds.Remove(myId);
            myMat.SetFloat(ParticleManager.SpeedID, 0.5f);
        }
        else
        {
            ActivePortalIds.Add(myId);
            myMat.SetFloat(ParticleManager.SpeedID, 3f);
        }
        
    }

}
