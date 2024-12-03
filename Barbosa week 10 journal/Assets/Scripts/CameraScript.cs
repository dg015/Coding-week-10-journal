using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraScript : MonoBehaviour
{

    [SerializeField] private Camera followCamera;
    private Vector2 viewPortHalfSize;
    private float leftBoundaryLimit;
    private float rightBoundaryLimit;
    private float bottomBoundaryLimit;

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothing = 5f;


    // Start is called before the first frame update
    void Start()
    {
        tilemap.CompressBounds();
        calculateCameraBoundaries();
    }

    private void calculateCameraBoundaries()
    {


        viewPortHalfSize = new Vector2(followCamera.orthographicSize * followCamera.aspect, followCamera.orthographicSize);


        leftBoundaryLimit = tilemap.transform.position.x + tilemap.cellBounds.min.x + viewPortHalfSize.x;

        rightBoundaryLimit = tilemap.transform.position.x + tilemap.cellBounds.max.x - viewPortHalfSize.x;

        bottomBoundaryLimit = tilemap.transform.position.y + tilemap.cellBounds.min.y + viewPortHalfSize.y;

    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector2 desiredPosition = target.position + new Vector3(offset.x, offset.y, transform.position.z);
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-smoothing * Time.deltaTime));


        smoothPosition.x = Mathf.Clamp(smoothPosition.x, leftBoundaryLimit, rightBoundaryLimit);
        smoothPosition.y = Mathf.Clamp(smoothPosition.y, bottomBoundaryLimit, smoothPosition.y);
        smoothPosition.z = -10;
        transform.position = smoothPosition;
        
    }
}
