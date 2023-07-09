using UnityEngine;

public class PaintBall : Ball
{
    public override void OnDestroy()
    {
        base.OnDestroy();
        Vector3 pos = transform.GetChild(0).position;
        Collider[] cols=Physics.OverlapSphere(pos, 5, GameManager.PlayerLayers);
        foreach (Collider c in cols)
        {
            Material createdMat = new Material(ParticleManager.GlueBallMat);
        
            //Kill me :(
            createdMat.SetFloat(ParticleManager.ColorID, Random.Range(0,1f));
            createdMat.SetInt(ParticleManager.RandomTexID, Random.Range(0,4));
            createdMat.SetVector(ParticleManager.RandomOffsetID, new Vector4(Random.Range(-0.25f,0.25f),Random.Range(-0.25f,0.25f)));
            c.transform.parent.GetComponent<Ball>().ApplySlow(this, createdMat);
        }
    }
}
