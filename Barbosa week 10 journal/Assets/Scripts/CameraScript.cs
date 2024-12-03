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

    private Vector3 shakOffset = Vector3.zero;

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
        //testing purposes
        if(Input.GetKeyDown(KeyCode.H))
        {
            shake(2.5f, 3f);
        }


        Vector2 desiredPosition = target.position + new Vector3(offset.x, offset.y, transform.position.z) + shakOffset;
        Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, 1 - Mathf.Exp(-smoothing * Time.deltaTime));


        smoothPosition.x = Mathf.Clamp(smoothPosition.x, leftBoundaryLimit, rightBoundaryLimit);
        smoothPosition.y = Mathf.Clamp(smoothPosition.y, bottomBoundaryLimit, smoothPosition.y);
        smoothPosition.z = -10;
        transform.position = smoothPosition;
        
    }

    public void shake(float intensity, float duration)
    {
        StartCoroutine(shakeCoroutine(intensity, duration));
    }

    private IEnumerator shakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            shakOffset = Random.insideUnitCircle * intensity;
            elapsed += Time.deltaTime;

            yield return null;
        }
        shakOffset = Vector3.zero;
    }
}
