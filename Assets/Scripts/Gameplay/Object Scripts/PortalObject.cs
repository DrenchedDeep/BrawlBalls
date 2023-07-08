using System;
using System.Collections.Generic;
using Gameplay.Object_Scripts;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PortalObject : PlaceableObject
{
    private PortalObject boundPortal;
    private const int Precision = 12;
    // Start is called before the first frame update

    private void Bind(PortalObject portal)
    {
        boundPortal = portal;
    }

    //There's really two choices here, either cut a hole in the mesh collider
    //OR
    //Just when a physics object goes over us, force them to fall... Obviously this can have it's problems.
    
    //ref https://www.youtube.com/watch?v=5qGE2PL9wwU
    [ContextMenu("MoveCollider")]
    
    private void Start()
    {
        transform.lossyScale.Set(1,1,1);
        //Only collide with default layer...
        Physics.Raycast(transform.position, Vector3.down,  out RaycastHit h,3, 1);

        BoxCollider collider  = GetComponent<BoxCollider>();



        Transform host;
        if (!h.transform)
        {
            print("No host??");
            Debug.DrawRay(transform.position, Vector3.down * 3, Color.red, 15, false);
        }
        
        if (h.transform.childCount != 0)
        {
            h.transform.GetComponent<MeshCollider>().enabled = false;
            host = h.transform;
        }
        else
        {
            host = h.transform.parent;
        }

        transform.position = h.point;
        transform.parent = host;
        transform.localEulerAngles = new Vector3(90, 0, 0);
        

        

        Vector3 pos = transform.position;
        Vector2 holePos = new Vector2(pos.x, pos.z);
        PolygonCollider2D groundCollider2D = host.GetChild(0).GetComponent<PolygonCollider2D>();
        MeshCollider generatedMeshCollider = host.GetChild(1).GetComponent<MeshCollider>();
        
        
        

        float width = GetComponent<DecalProjector>().size.x;
        collider.center = new Vector3(0, 0, 1.5f);
        collider.size = new Vector3(width, width,1f);
        width /= 2;
        //Generate 2D collisions
        
       

        Vector3 scaledPos = host.localScale;
        Vector3 ownerOffset = host.position;
        ownerOffset.z /= scaledPos.z;
        ownerOffset.x /= scaledPos.x;
        ownerOffset.y /= scaledPos.y;
        holePos.y = holePos.y/scaledPos.z - ownerOffset.z ;
        holePos.x = holePos.x/scaledPos.x - ownerOffset.x;

        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;

        foreach (Vector2 borderPt in groundCollider2D.GetPath(0))
        {
            if (borderPt.x < min.x && borderPt.y < min.y)
                min = borderPt;
            else if (borderPt.x > max.x && borderPt.y > max.y)
                max = borderPt;
        }
        print(min +" , " + max);

        Vector2[] pointPositions = new Vector2[Precision];
        float angle = 0;
        float inc = 360f / Precision * Mathf.Deg2Rad;

        //scaledPos = Quaternion.AngleAxis(host.eulerAngles.x, Vector3.forward)*scaledPos;
        //holePos.y += Mathf.-3.722211f;
        for (int index = 0; index < Precision; index++)
        {
            pointPositions[index].x =  Mathf.Clamp(Mathf.Cos(angle)  /scaledPos.x * width + holePos.x,min.x,max.x);
            pointPositions[index].y =  Mathf.Clamp(Mathf.Sin(angle) /scaledPos.z * width + holePos.y,min.y,max.y);

            angle += inc;
        }
        
        
        //Make order not matter.
        int n = groundCollider2D.pathCount;
        
        groundCollider2D.pathCount = n+1; //-2+1 (For 2d3d projections, +1 for self)
        groundCollider2D.SetPath(n, pointPositions);
        
        //Generate 3D collisions
        Vector3 scale = new Vector3(1 / scaledPos.x, 1 / scaledPos.y, 1 / scaledPos.z);
        generatedMeshCollider.transform.localScale = scale;
        
        generatedMeshCollider.transform.localPosition = new Vector3(-ownerOffset.x, 0, -ownerOffset.y);
        Mesh m = groundCollider2D.CreateMesh(true, true);
        m.Optimize();
        generatedMeshCollider.sharedMesh = m;
        
        
        
        

    }


    private void OnDestroy()
    {
        if(boundPortal) Destroy(boundPortal);
    }


    protected override void OnHit(Ball hit)
    {
        //Actually probably don't want this to do anything...
    }
}
