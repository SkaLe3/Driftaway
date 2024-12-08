using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraTransition : MonoBehaviour
{

    [SerializeField] private Camera FirstCamera; 
    [SerializeField] private Camera SecondCamera;  
    [SerializeField] private Transform FocusTarget; 
    [SerializeField] private Transform[] ControlPoints = new Transform[2];
    [SerializeField] private AnimationCurve EaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    private Transform[] SplineControlPoints; 
    private float CameraTransitionDuration = 3f;
    private Camera TempCamera;

    private void OnValidate()
    {
        UpdatePoints();
    }
    private void Start()
    {
        UpdatePoints();
    }

    private void UpdatePoints()
    {
        if (FirstCamera != null && SecondCamera != null && ControlPoints.Length == 2)
        {
            SplineControlPoints = new Transform[4]
            {
                FirstCamera.transform,   
                ControlPoints[0],        
                ControlPoints[1],        
                SecondCamera.transform   
            };
        } 
    }

    public void StartTransition(float duration)
    {

        CameraTransitionDuration = duration;
        FirstCamera.gameObject.SetActive(false); // Deactivate the first camera
        CreateTempCamera();
        StartCoroutine(SmoothCameraTransition());
        Debug.Log("Transition Started");
    }

    private void CreateTempCamera()
    {
        TempCamera = Instantiate(FirstCamera); // Copy FirstCamera
        TempCamera.name = "TempCamera"; // Name the new camera
        TempCamera.transform.position = FirstCamera.transform.position;
        TempCamera.transform.rotation = FirstCamera.transform.rotation;

        TempCamera.gameObject.SetActive(true);
    }

    private IEnumerator SmoothCameraTransition()
    {
        float elapsedTime = 0f;
        Vector3 targetPosition = SecondCamera.transform.position;
        Quaternion startRotation = FirstCamera.transform.rotation;
        Quaternion endRotation = SecondCamera.transform.rotation;
        while (elapsedTime < CameraTransitionDuration)
        {
            float t = elapsedTime / CameraTransitionDuration;
            t = EaseCurve.Evaluate(t);
            Vector3 splinePosition = GetBezierPoint(t);
            TempCamera.transform.position = splinePosition;
            Quaternion interpolatedRotation = Quaternion.Lerp(startRotation, endRotation, t);
            TempCamera.transform.rotation = interpolatedRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        TempCamera.transform.position = targetPosition;
        TempCamera.transform.rotation = endRotation;
        TempCamera.gameObject.SetActive(false);
        SecondCamera.gameObject.SetActive(true);
        Destroy(TempCamera.gameObject);
    }

    private Vector3 GetBezierPoint(float t)
    {
        if (SplineControlPoints.Length != 4)
        {
            Debug.LogError("Bezier curve requires 4 control points.");
            return Vector3.zero;
        }

        // Cubic Bezier Curve calculation (De Casteljau's Algorithm)
        Vector3 p0 = SplineControlPoints[0].position;
        Vector3 p1 = SplineControlPoints[1].position;
        Vector3 p2 = SplineControlPoints[2].position;
        Vector3 p3 = SplineControlPoints[3].position;

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(d, e, t);
    }

     private void OnDrawGizmos()
    {
        if (SplineControlPoints.Length == 4)
        {
            Gizmos.color = Color.red;

            Vector3 previousPoint = SplineControlPoints[0].position;
            for (float t = 0f; t <= 1f; t += 0.05f)
            {
                Vector3 point = GetBezierPoint(t);
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            Gizmos.color = Color.blue;
            foreach (var point in SplineControlPoints)
            {
                Gizmos.DrawSphere(point.position, 0.1f);
            }
        }
    }
}
