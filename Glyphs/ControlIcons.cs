using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pair the name of a control and its matching icon
/// </summary>
[System.Serializable]
public struct InputActionSpriteBinding
{
    public string controlName;
    public Sprite icon;
}

/// <summary>
/// Define the sprites and their respective binding for each input
/// </summary>
[CreateAssetMenu(fileName = "NewControllerIcons", menuName = "Controller System/Control Icon")]
public class ControlIcons : ScriptableObject
{
    public string deviceName; // device name is handled by InputIconManager. kept for refined funcionality later
    public List<InputActionSpriteBinding> bindings;

    /// <summary>
    /// Return the icon for a matching control path
    /// </summary>
    /// <param name="controlPath"></param>
    /// <returns></returns>
    public Sprite GetIconForAction(string controlPath)
    {
        if(string.IsNullOrWhiteSpace(controlPath) || bindings == null)
        {
            return null;
        }

        string normalizedPath = controlPath.ToLowerInvariant();
        int slashIndex = normalizedPath.IndexOf('/');
        string controlToken = slashIndex >= 0 && slashIndex < normalizedPath.Length - 1 
            ? normalizedPath[(slashIndex + 1)..] 
            : normalizedPath;
        controlToken = controlToken.Trim().Trim('{', '}');

        Sprite bestMatchIcon = null;
        int bestMatchLength = -1;

        foreach(InputActionSpriteBinding binding in bindings)
        {
            if(string.IsNullOrWhiteSpace(binding.controlName) || binding.icon == null)
            {
                continue;
            }

            string normalizedControlName = binding.controlName.ToLowerInvariant();
            bool exactTokenMatch = string.Equals(controlToken, normalizedControlName);
            bool partialTokenMatch = controlToken.Contains(normalizedControlName);
            bool pathSegmentMatch = normalizedPath.Contains("/" + normalizedControlName);

            if(!exactTokenMatch && !partialTokenMatch && !pathSegmentMatch)
            {
                continue;
            }

            if(normalizedControlName.Length > bestMatchLength)
            {
                bestMatchLength = normalizedControlName.Length;
                bestMatchIcon = binding.icon;
            }
        }

        return bestMatchIcon;
    }

    /// <summary>
    /// Get the TMP tag based on a control path
    /// </summary>
    /// <param name="controlPath"></param>
    /// <returns></returns>
    public string GetTMPTag(string controlPath)
    {
        Sprite icon = GetIconForAction(controlPath);

        if (icon != null)
        {
            return $"<sprite name=\"{icon.name}\">";
        }

        return $"[{controlPath}]";
    }
}