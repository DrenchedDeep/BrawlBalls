using System;
using System.Collections;
using UnityEngine;

public class PodiumRotator : MonoBehaviour
{
    [SerializeField] private float duration = 1;
    private Transform [] transPositions;
    private bool isRotating;
    private int curForward = 1;
    private bool isLowering;
    [SerializeField] private Camera cam;
    private bool inCustomization;
    
    // Start is called before the first frame update
    void Start()
    {
        int n = transform.childCount;
        transPositions = new Transform[n];
        print(n);
        for (int i = 0; i < n; ++i)
        {
            transPositions[i] = transform.GetChild(i);
        }
    }

    //"interface" so no mistakes.
    public void MoveLeft()
    {
        Move(-1);
    }

    public void MoveRight()
    {
        Move(1);
    }

    
    private void Update()
    {
        #if UNITY_EDITOR
        if(!Input.GetMouseButtonDown(0)) return;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
#else
        if(Input.GetTouch(0).phase != TouchPhase.Began) return;
        Ray ray = cam.ScreenPointToRay(Input.GetTouch(0).position);
        #endif
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            //Open customization menu
            if (hit.point.x > 0)
                MoveRight();
            else
                MoveLeft();
        }
    }

    private void Move(int dir)
    {
        if (isRotating) return;
        int start = curForward;
        Vector3 prv = transPositions[start].position;
        isRotating = true;
        print("rotating: " + dir);
        do
        {
            start += dir;
            if (start < 0) start = transPositions.Length - 1;
            else if (start ==  transPositions.Length) start = 0;
            Vector3 temp = transPositions[start].position;
            StartCoroutine(SlerpIt(prv, transPositions[start]));
            prv = temp;
        } while (curForward != start);
        curForward += dir;
        if (curForward < 0) curForward = transPositions.Length - 1;
        else if (curForward ==  transPositions.Length) curForward = 0;
    }

    private IEnumerator SlerpIt(Vector3 next, Transform id)
    {
        float curTime = 0;
        Vector3 origin = id.position;
        while (curTime < duration)
        {
            curTime += Time.deltaTime;
            id.position = Vector3.Slerp(origin, next, curTime / duration);
            yield return null;
        }

        isRotating = false;
    }

}
