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

    private Coroutine camerRotationCoroutine;

    private List<Vector3> points = new List<Vector3>(4)
    {
        new Vector3(0.5f, 5.0f, -7.0f),
        new Vector3(-6.5f, 5.0f, -3.0f),
        new Vector3(-6.5f, 5.0f, 2.0f),
        new Vector3(0.5f, 5.0f, 6.0f),
        new Vector3(7.0f, 5.0f, 2.0f),
        new Vector3(7.5f, 5.0f, -3.0f)
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

    /// <summary>
    /// Осуществляет горизонтальный облет камеры с левой стороный игровой доски после передачи хода.
    /// </summary>
    /// <param name="duration">Длительнсть облета.</param>
    /// <param name="colorType">Цвет шашек игрока, которому передается ход.</param>
    public void MoveHorizontal(float duration, ColorType colorType)
    {
        switch (colorType)
        {
            case ColorType.White:
                camerRotationCoroutine = StartCoroutine(CamerRotationCoroutine(colorType, duration, new List<Vector3>(4) { points[3], points[4], points[5], points[0] }));
                break;
            case ColorType.Black:
                camerRotationCoroutine = StartCoroutine(CamerRotationCoroutine(colorType, duration, new List<Vector3>(4) { points[0], points[1], points[2], points[3] }));
                break;
        }
    }

    /// <summary>
    /// Осуществляет поворот камеры при пердаче хода другому игроку.
    /// </summary>
    /// <param name="colorType">Цвет шашек игрока, которому передается ход.</param>
    /// <param name="duration">Длительность облета камеры.</param>
    /// <param name="path">Путь из точке для построения кривой Безье как траетории облета.</param>
    /// <returns>IEnumerator как возвращаемое значение корутины.</returns>
    private IEnumerator CamerRotationCoroutine(ColorType colorType,float duration, List<Vector3> path)
    {
        float time = 0;

        while (time < duration)
        {
            transform.position = Beziers.GetPoint(path[0], path[1], path[2], path[3], time);
            transform.LookAt(target);
            time += Time.deltaTime;

            yield return null;
        }

        transform.position = path.Last();

        ///if (transform.position == path.Last())
        //{

            Vector3 camRotateCompensation = Vector3.zero;

            switch (colorType)
            {
                case ColorType.White:
                    camRotateCompensation = Vector3.Lerp(transform.eulerAngles, new Vector3(40f, 0f, 0f), 1);
                    break;
                case ColorType.Black:
                    camRotateCompensation = Vector3.Lerp(transform.eulerAngles, new Vector3(40f, -180f, 0f), 1);
                    break;
            }

            transform.eulerAngles = camRotateCompensation;

            camerRotationCoroutine = null;
            CameraRotationComplete?.Invoke(this, true);
            yield break;
        //}

    }
    #endregion
}
