using System;
using UnityEngine;
using System.Reflection;
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

    public static bool IsPrefab(this GameObject go) => go.scene.name == null;

    /// <summary>
    /// Set object's Private field using reflection. Very useful for Editor stuff, like custom Editor for classes
    /// </summary>
    public static void SetPrivateField(this object targetObject, string fieldName, object value)
    {
        Type type = targetObject.GetType();
        FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (fieldInfo != null)
            fieldInfo.SetValue(targetObject, value);
        else
            Debug.LogWarning($"Field {fieldName} not found in {type.Name}");
    }

#endregion

#region Vectors

    /// <summary>
    /// Override specific value of Vector2
    /// </summary>
    /// <returns></returns>
    public static Vector2 With(this Vector2 vector, float? x = null, float? y = null) =>
        new(x ?? vector.x, y ?? vector.y);
    
    public static Vector2 RoundDigits(this Vector2 vector, int digitsCount)
    {
        int num = (int)Mathf.Pow(10, digitsCount);
        return new Vector2(Mathf.Round(vector.x * num) / num, Mathf.Round(vector.y * num) / num);
    }

    /// <summary>
    /// Override specific value of Vector3
    /// </summary>
    /// <returns></returns>
    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null) =>
        new(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    
    public static Vector3 RoundDigits(this Vector3 vector, int digitsCount)
    {
        int num = (int)Mathf.Pow(10, digitsCount);
        return new Vector4(Mathf.Round(vector.x * num) / num, Mathf.Round(vector.y * num) / num, Mathf.Round(vector.z * num) / num);
    }

    /// <summary>
    /// Override specific value of Vector4
    /// </summary>
    /// <returns></returns>
    public static Vector4 With(this Vector4 vector, float? x = null, float? y = null, float? z = null, float? w = null) =>
        new(x ?? vector.x, y ?? vector.y, z ?? vector.z, w ?? vector.w);

    public static Vector4 RoundDigits(this Vector4 vector, int digitsCount)
    {
        int num = (int)Mathf.Pow(10, digitsCount);
        return new Vector4(Mathf.Round(vector.x * num) / num, Mathf.Round(vector.y * num) / num, Mathf.Round(vector.z * num) / num, Mathf.Round(vector.w * num) / num);
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

#endregion
}
}