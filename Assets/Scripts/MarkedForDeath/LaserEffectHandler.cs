using System.Collections;
using Gameplay.Balls;
using Managers;
using UnityEngine;
using UnityEngine.VFX;

namespace MarkedForDeath
{
    public class LaserEffectHandler : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private VisualEffect myEffect;
        [SerializeField] private AnimationCurve initBeam;
        private const float TimeToActivate = 0.6f;

        private float _width;
        [SerializeField] private Weapon w;
        private float _lifeTime;
    

        private void OnEnable()
        {
            StartCoroutine(HandleSizeChange());
            myEffect.SendEvent(StaticUtilities.ActivateID);
            _lifeTime = w.GetAbility.Cooldown * 0.6f;
        
            Debug.LogError("THE LASER EFFECT SCRIPT SHOULD NOT EXIST ANYMORE. IT RUNS ON THE GPU PARTICLE SYSTEM AND THEREFORE MUST BE REPLACED");
        
        }

        private void Start()
        {
            gameObject.SetActive(false);
        
            _width = w.Range.y;
            myEffect.SetFloat(StaticUtilities.DelayID, TimeToActivate);
            myEffect.SetVector4(StaticUtilities.ColorID, lineRenderer.material.GetColor(StaticUtilities.ColorID));
        }

        private void LateUpdate()
        {
            //This sucks but cope
            float maxDist = 0;
            for(int i = 0; i < w.HitCount; ++i)
            {
                float d = w.Hits[i].distance;
                if (maxDist < d)
                {
                    maxDist = d;
                }
            }

            if (maxDist == 0) maxDist = w.Range.x;
            maxDist *= 3;
            myEffect.SetFloat(StaticUtilities.PositionID, maxDist);
            lineRenderer.SetPosition(1,maxDist*Vector3.forward);
        
            _lifeTime -= Time.deltaTime;
            if (_lifeTime <= 0)
            {
                BallPlayer.LocalBallPlayer.EnableControls();
                w.ToggleActive();
                gameObject.SetActive(false);
                myEffect.SendEvent(StaticUtilities.EndID);
            }
        }



        private IEnumerator HandleSizeChange()
        {
            float curTime = 0;

            while (curTime < TimeToActivate)
            {
                curTime += Time.deltaTime;
                lineRenderer.startWidth = Mathf.Lerp(0.01f, _width, initBeam.Evaluate(curTime / TimeToActivate));
                yield return null;
            }
            w.ToggleActive();
            lineRenderer.startWidth = _width;
            BallPlayer.LocalBallPlayer.DisableControls();
        }

        public void SetProperty(int id, float amount)
        {
            lineRenderer.material.SetFloat(id, amount);
        }
    }
}
