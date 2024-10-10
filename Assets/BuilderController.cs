using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderController : MonoBehaviour
{
    public GameObject metaball;
    private Camera mainCamera;

    private GameObject temp;
    public LayerMask layerMask;
    public bool SelectTool = true;
    public bool BoneTool = true;

    public float instantiateDistance;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        InstantiateNewBone();
    }

    // Update is called once per frame
    void Update()
    {
        if(SelectTool)
        {
            MoveCurrentBone();
            if(Input.GetMouseButtonDown(0)){
                InstantiateNewBone();
            }
        }

    }

    Vector3 GetClosestPoint()
    {
        Vector3 closestPoint = Vector3.zero;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.SphereCast(ray, 1f, out RaycastHit hit, float.MaxValue, layerMask))
        {
            closestPoint = hit.point;
        }

        return closestPoint;
    }

    Vector3 CalculatePointAtDistance(Vector3 startPoint, float distance)
    {
        Vector3 direction = (startPoint - transform.position).normalized;

        Vector3 targetPoint = startPoint + direction * distance;

        return targetPoint;
    }

    void InstantiateNewBone() 
    {
        if(temp != null)
        {
            temp.layer = 3;
            temp = null;
        }

        temp = Instantiate(metaball, GetClosestPoint(), Quaternion.identity);
        temp.layer = 6;
    }

    void MoveCurrentBone()
    {
        var closestPoint = GetClosestPoint();

        if(closestPoint != Vector3.zero){
            temp.GetComponent<Transform>().position = CalculatePointAtDistance(closestPoint, instantiateDistance);
        }
    }



}
