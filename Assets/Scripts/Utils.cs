using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static void Log(string message)
    {
        #if UNITY_EDITOR
        Debug.Log(message);
        #endif
    }

    public static void LogWarning(string message)
    {
        #if UNITY_EDITOR
        Debug.LogWarning(message);
        #endif
    }

    public static void LogError(string message)
    {
        #if UNITY_EDITOR
        Debug.LogError(message);
        #endif
    }

    public static T[] DeepCopy1D<T>(T[] array)
    {
        T[] newArray = new T[array.Length];
        Array.Copy(array, newArray, array.Length);
        return newArray;
    }

    public static T[] CreateFill1D<T>(int length, T value)
    {
        T[] newArray = new T[length];
        for (int i = 0; i < length; i++)
        {
            newArray[i] = value;
        }
        return newArray;
    }

    
}
