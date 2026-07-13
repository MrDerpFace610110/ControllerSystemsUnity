using UnityEngine;

/// <summary>
/// Create and configure controller rumble effects
/// </summary>
[CreateAssetMenu(fileName = "New Rumble Profile", menuName = "Controller System/Rumble Profile")]
public class RumbleProfile : ScriptableObject
{
    [Tooltip("Higher = more important")] public int priority = 0;

    [Tooltip("Duration of rumble")] public float duration = 0.5f;
    [Tooltip("Does the rumble repeat until told otherwise")] public bool isContinuous = false;

    [Header("Curves")]
    public AnimationCurve lowFreqCurve;
    public AnimationCurve highFreqCurve;

    public void Play()
    {
        ControllerRumble.PlayProfile(this);
    }
}
