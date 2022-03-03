using UnityEngine;

public static class Beziers 
{
    /// <summary>Вычисляет кривую Безье между точками.</summary>
    /// <param name="p0">Первая точка.</param>
    /// <param name="p1">Вторая точка.</param>
    /// <param name="p2">Третья точка.</param>
    /// <param name="p3">Четвертая точка.</param>
    /// <param name="t"></param>
    /// <returns>Интерполированный вектор.</returns>
    /// <remark>https://www.youtube.com/watch?v=EttpoUTSHN0&t=11s</remark>>
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 p01 = Vector3.Lerp(p0, p1, t);
        Vector3 p12 = Vector3.Lerp(p1, p2, t);
        Vector3 p23 = Vector3.Lerp(p2, p3, t);

        Vector3 p012 = Vector3.Lerp(p01, p12, t);
        Vector3 p123 = Vector3.Lerp(p12, p23, t);

        Vector3 p0123 = Vector3.Lerp(p012, p123, t);

        return p0123;
    }
}
