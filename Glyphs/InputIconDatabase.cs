using UnityEngine;

/// <summary>
/// Stores the ControlIcons data for each configuration
/// </summary>
[CreateAssetMenu(fileName = "New Input Icon Database", menuName = "Controller System/Input Icon Database")]
public class InputIconDatabase : ScriptableObject
{
    [Header("Console Icon Profiles")]
    public ControlIcons xboxIcons;
    public ControlIcons playstationIcons;
    public ControlIcons switchIcons;
}