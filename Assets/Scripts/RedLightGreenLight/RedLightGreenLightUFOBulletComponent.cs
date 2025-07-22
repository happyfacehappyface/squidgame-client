using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RedLightGreenLightUFOBulletComponent : MonoBehaviour
{
    [SerializeField] private Transform _transform;
    [SerializeField] private Animator _animator;
    private TimeSpan _currentTime;

    private Vector2 _origin;
    private Vector2 _dest;
    private TimeSpan _duration;

    public void ManualStart(Vector2 origin, Vector2 dest, TimeSpan duration)
    {
        _origin = origin;
        _dest = dest;
        _duration = duration;

        _transform.SetLocalPositionAndRotation(_origin, Quaternion.Euler(0f, 0f, Mathf.Atan2(_dest.y - _origin.y, _dest.x - _origin.x) * Mathf.Rad2Deg));
        _currentTime = TimeSpan.Zero;
    }

    public void ManualUpdate()
    {
        _currentTime += TimeSpan.FromSeconds(Time.deltaTime);

        _transform.localPosition = Vector2.Lerp(_origin, _dest, (float)(_currentTime.TotalSeconds / _duration.TotalSeconds));
    }


    public bool ShouldBeDeleted()
    {
        return _currentTime >= (_duration + TimeSpan.FromSeconds(0.2f));
    }

}
