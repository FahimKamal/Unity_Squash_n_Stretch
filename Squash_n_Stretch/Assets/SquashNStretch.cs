using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SquashNStretch : MonoBehaviour
{
    [Header("Notes")]
    [SerializeField, Multiline(2)] private string notes;

    [Header("Squash and stretch Core")] 
    [SerializeField] [Tooltip("Defaults to current Go if not set and Should be child transform")]
    private Transform transformToAffect;
    [SerializeField] private SquashStretchAxis axisToAffect = SquashStretchAxis.Y;
    
    [Flags]
    public enum SquashStretchAxis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
    }
    [SerializeField, Range(0, 1f)] private float animationDuration = 0.25f;
    [SerializeField] private bool canBeOverWritten;
    [SerializeField] private bool playOnStart;
    [SerializeField] private bool playsEveryTime = true;
    [SerializeField, Range(0, 100f), Tooltip("% from 0 to 100%")]
    private float chanceToPlay = 100f;
    

    [Header("Animation Settings")] 
    [SerializeField] private float initialScale = 1f;
    [SerializeField] private float maximumScale = 1.3f;
    [SerializeField] private bool resetToInitialScaleAfterAnimation = true;
    [SerializeField] private bool reverseAnimationCurveAfterPlaying;
    private bool _isReversed;
    
    [SerializeField] private AnimationCurve squashAndStretchCurve = new AnimationCurve(
        new Keyframe(0f, 0f), 
        new Keyframe(0.25f, 1f),
        new Keyframe(1f, 0f)
        );
    
    [Header("Looping Settings")]
    [SerializeField] private bool looping;
    [SerializeField] private float loopingDelay = 0.5f;
    
    private Coroutine _squashAndStretchCoroutine;
    private WaitForSeconds _loopingDelayWaitForSeconds;
    private Vector3 _initialScaleVector;
    
    private bool affectX => (axisToAffect & SquashStretchAxis.X) != 0;
    private bool affectY => (axisToAffect & SquashStretchAxis.Y) != 0;
    private bool affectZ => (axisToAffect & SquashStretchAxis.Z) != 0;
    // Will Add Later

    private void Awake()
    {
        if (transformToAffect == null)
        {
            transformToAffect = transform;
        }
        _initialScaleVector = transformToAffect.localScale;
        _loopingDelayWaitForSeconds = new WaitForSeconds(loopingDelay);
    }

    private void Start()
    {
        if (playOnStart)
        {
            CheckForAndStartCoroutine();
        }
    }

    
    [ContextMenu("Play Squash and Stretch")]
    public void PlaySquashAndStretch()
    {
        if (looping && !canBeOverWritten)
        {
            return;
        }
        CheckForAndStartCoroutine();
    }

    private void CheckForAndStartCoroutine()
    {
        if (axisToAffect == SquashStretchAxis.None)
        {
            Debug.LogWarning("Axis to affect is set to None", gameObject);
            return;
        }

        if (_squashAndStretchCoroutine != null)
        {
            Debug.Log("working");
            StopCoroutine(_squashAndStretchCoroutine);
            if (playsEveryTime && resetToInitialScaleAfterAnimation)
            {
                transform.localScale = _initialScaleVector;
            }
        }

        _squashAndStretchCoroutine = StartCoroutine(SquashAndStretchEffect());
    }

    private IEnumerator SquashAndStretchEffect()
    {
        Debug.Log("Squash And Stretch");
        do
        {
            if (!playsEveryTime)
            {
                var random = Random.Range(0, 100f);
                if (random > chanceToPlay)
                {
                    yield return null;
                    continue;
                }
            }
            
            if (reverseAnimationCurveAfterPlaying)
            {
                _isReversed = !_isReversed;
            }
            
            var elapsedTime = 0f;
            var originalScale = _initialScaleVector;
            var modifiedScale = originalScale;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;

                float curvePosition;
                if (_isReversed)
                {
                    curvePosition = 1 - (elapsedTime / animationDuration);
                }
                else
                {
                    curvePosition = elapsedTime / animationDuration;
                }
                var curveValue = squashAndStretchCurve.Evaluate(curvePosition);
                var remappedValue = initialScale + (curveValue * (maximumScale - initialScale));

                var minimumThreshold = 0.0001f;
                if (Mathf.Abs(remappedValue) < minimumThreshold)
                {
                    remappedValue = minimumThreshold;
                }

                if (affectX)
                    modifiedScale.x = originalScale.x * remappedValue;
                else
                    modifiedScale.x = originalScale.x / remappedValue;

                if (affectY)
                    modifiedScale.y = originalScale.y * remappedValue;
                else
                    modifiedScale.y = originalScale.y / remappedValue;

                if (affectZ)
                    modifiedScale.z = originalScale.z * remappedValue;
                else
                    modifiedScale.z = originalScale.z / remappedValue;
                
                transformToAffect.localScale = modifiedScale;
                
                yield return null;
            }

            if (resetToInitialScaleAfterAnimation)
            {
                transformToAffect.localScale = originalScale;
            }

            if (looping)
            {
                yield return _loopingDelayWaitForSeconds;
            }
            
        } while (looping);
    }

    public void SetLooping(bool shouldLoop)
    {
        looping = shouldLoop;
    }
}
