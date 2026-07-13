/* 
 * ControllerSystemsUnity
 * Copyright (c) 2026 Brandon Swannell
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using UnityEngine;

/// <summary>
/// Handles the access of UI glyphs of controller configuration buttons
/// </summary>
public static class InputIconManager
{
    private static InputIconDatabase _database;

    /// <summary>
    /// Define the InputIconDataBase to be used
    /// </summary>
    /// <param name="database"></param>
    public static void Initialize(InputIconDatabase database)
    {
        _database = database;

        if(_database == null || database.xboxIcons == null)
        {
            Debug.LogWarning("[InputIconManager] Initalized with a null database or null Xbox icons, falling back to text");
        }
    }

    /// <summary>
    /// Get the icons associated with a specfic device
    /// </summary>
    /// <param name="deviceName"></param>
    /// <returns></returns>
    public static ControlIcons GetControlIcons(string deviceName)
    {
        if(_database == null)
        {
            Debug.LogError("[InputIconManager] Database has not been initialized!");
            return null;
        }

        if (string.IsNullOrWhiteSpace(deviceName)) return _database.xboxIcons;

        string normalizedName = deviceName.ToLowerInvariant();

        if (normalizedName.Contains("playstation") || normalizedName.Contains("dualshock") || normalizedName.Contains("dualsense"))
        {
            return _database.playstationIcons;
        }

        if (normalizedName.Contains("switch") || normalizedName.Contains("nintendo"))
        {
            return _database.switchIcons;
        }

        //default / fallback
        return _database.xboxIcons;
    }

    /// <summary>
    /// Get device icon for a specfic control
    /// </summary>
    /// <param name="deviceName"></param>
    /// <param name="controlName"></param>
    /// <returns></returns>
    public static Sprite GetControlIcon(string deviceName, string controlName)
    {
        ControlIcons icons = GetControlIcons(deviceName);
        return icons != null ? icons.GetIconForAction(controlName) : null;
    }

    /// <summary>
    /// Get the TMP tag in a TMP_SpriteAsset matching the icon. Requires the TMP_SpriteAsset to be set on the text component
    /// </summary>
    /// <param name="deviceName"></param>
    /// <param name="actionName"></param>
    /// <returns></returns>
    public static string GetTMPTagForAction(string deviceName, string actionName)
    {
        ControlIcons icons = GetControlIcons(deviceName);
        
        if (icons != null)
        {
            return icons.GetTMPTag(actionName);
        }
        
        return "<sprite name=\"MissingIcon\">"; 
    }
}