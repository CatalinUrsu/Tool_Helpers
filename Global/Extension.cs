using System;
using Constants;
using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;
#if DOTWEEN
using DG.Tweening;
#endif

namespace Helpers
{
public static class Extension
{
#region IEnumerable

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

#endregion

#if DOTWEEN
#region DoTween

    /// <summary>
    /// If tween IsActive or IsPlaying, than End (Kill of Complete)
    /// </summary>
    /// <param name="tween"></param>
    /// <param name="complete">Decide to Complete or Kill tween</param>
    public static void CheckAndEnd(this Tween tween, bool complete = true)
    {
        if (tween == null) return;

        if (tween.IsActive() && tween.IsPlaying())
        {
            if (complete)
                tween.Complete();
            else
                tween.Kill();
        }
    }

#endregion
#endif

#region Object

    /// <summary>
    /// If component persist -> return it, else add it to gameobject
    /// </summary>
    /// <param name="gameObject"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetOrAdd<T>(this GameObject gameObject) where T : Component =>
        gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();

    /// <summary>
    ///  Get Object or null (can be used for null-propagation / null-coalescing operations)
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;

    /// <summary>
    /// Get children of transform (very usefull for LINQs)
    /// </summary>
    public static IEnumerable<Transform> Children(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
            yield return transform.GetChild(i);
    }

#endregion

#region Vectors

    /// <summary>
    /// Override specific value of Vector2
    /// </summary>
    /// <returns></returns>
    public static Vector2 With(this Vector2 vector, float? x = null, float? y = null) =>
        new(x ?? vector.x, y ?? vector.y);

    /// <summary>
    /// Override specific value of Vector3
    /// </summary>
    /// <returns></returns>
    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null) =>
        new(x ?? vector.x, y ?? vector.y, z ?? vector.z);

    /// <summary>
    /// Override specific value of Vector4
    /// </summary>
    /// <returns></returns>
    public static Vector4 With(this Vector4 vector, float? x = null, float? y = null, float? z = null, float? w = null) =>
        new(x ?? vector.x, y ?? vector.y, z ?? vector.z, w ?? vector.w);

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

#endregion
}
}