using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Checkers;
using UnityEngine;

public class CameraComponent : MonoBehaviour
{
    #region Variables and constants

    public event EventHandler<bool> CameraRotationComplete;

    [SerializeField] private Transform target;

    //private Vector3 cameraRotation = new Vector3(45f, 0f, 0f);
    private Coroutine camerRotationCoroutine;

    private List<Vector3> points = new List<Vector3>(4)
    {
        new Vector3(0.6f, 5f, -6.5f),
        new Vector3(-5f, 5f, -0.3f),
        new Vector3(0.6f, 5f, 6.5f),
        new Vector3(5f, 5f, -0.3f)
    };

    #endregion

    #region MonoBehaviour methods

    private void Start()
    {
        Camera.main.transform.position = points[0];
        //Camera.main.transform.eulerAngles = cameraRotation;
    }

    #endregion

    #region Methods

    public void MoveHorizontal(float duration, ColorType colorType)
    {
        switch (colorType)
        {
            case ColorType.White:
                camerRotationCoroutine = StartCoroutine(CamerRotationCoroutine(duration, new List<Vector3>(2) { points[3], points[0] }));
                break;
            case ColorType.Black:
                camerRotationCoroutine = StartCoroutine(CamerRotationCoroutine(duration, new List<Vector3>(2) { points[1], points[2] }));
                break;
        }
        
    }

    private IEnumerator CamerRotationCoroutine(float duration, List<Vector3> path)
    {
        Vector3 startPosition = transform.position;

        foreach (var targetPosition in path)
        {
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, t / duration);
                transform.LookAt(target);
                yield return null;
            }

            transform.position = startPosition = targetPosition;
        }

        if (transform.position == path.Last())
        {
            camerRotationCoroutine = null;
            CameraRotationComplete?.Invoke(this, true);
            yield break;
        }

    }
    #endregion
}
