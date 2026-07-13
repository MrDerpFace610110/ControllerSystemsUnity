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

using System;
using System.Linq;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the rebinding of InputAction's in sepcfic groups
/// </summary>
public static class InputRebindHandler
{
    /// <summary>
    /// Start rebinding a InputAction in a specified group.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="groupName"></param>
    /// <param name="denyList"></param>
    /// <param name="onComplete"></param>
    public static void StartRebind(InputAction action, string groupName, RebindDenyList denyList = null, Action onComplete = null)
    {
        if(action == null)
        {
            onComplete?.Invoke();
            return;
        }

        int bindingIndex = action.bindings.IndexOf(x => x.groups.Contains(groupName));
        if(bindingIndex == -1)
        {
            onComplete?.Invoke();
            return;
        }

        string previousOverride = action.bindings[bindingIndex].overridePath;

        action.Disable();
        var rebindOp = action.PerformInteractiveRebinding(bindingIndex)
            .WithBindingGroup(groupName)
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                var newPath = action.bindings[bindingIndex].effectivePath;

                bool duplicate = IsPathAlreadyUsed(action.actionMap.asset, action, bindingIndex, groupName, newPath); 
                if(duplicate)
                {
                    if(string.IsNullOrWhiteSpace(previousOverride))
                    {
                        action.RemoveBindingOverride(bindingIndex);
                    }
                    else
                    {
                        action.ApplyBindingOverride(bindingIndex, previousOverride);
                    }
                }

                action.Enable();
                onComplete?.Invoke();
                operation.Dispose();
            })
            .OnCancel(operation =>
            {
                action.Enable();
                onComplete?.Invoke();
                operation.Dispose();
            });

        if(denyList != null)
        {
            foreach(var blocked in denyList.GetBlockedPaths(groupName))
            {
                rebindOp.WithControlsExcluding(blocked);
            }
        }

        rebindOp.Start();
    }

    /// <summary>
    /// Reset a InputAction to its default binding
    /// </summary>
    /// <param name="action"></param>
    public static void ResetBindsForAction(InputAction action)
    {
        if(action == null) return;

        action.RemoveAllBindingOverrides();
    }

    /// <summary>
    /// Reset all controls in a InputActionAsset to their defaults
    /// </summary>
    /// <param name="actions"></param>
    public static void ResetAllBinds(InputActionAsset actions)
    {
        if(actions == null) return;

        actions.RemoveAllBindingOverrides();
    }

    /// <summary>
    /// Check if an action is currently its default
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static bool ActionIsDefault(InputAction action)
    {
        return !action.bindings.Any(b => b.hasOverrides);
    }

    // Check that a bind isnt already being used for another control
    private static bool IsPathAlreadyUsed(InputActionAsset asset, InputAction action, int bindingIndex, string groupName, string newPath)
    {
        if(string.IsNullOrWhiteSpace(newPath)) return false;

        foreach(var map in asset.actionMaps)
        {
            foreach(var a in map.actions)
            {
                for(int i = 0; i < a.bindings.Count; i++)
                {
                    var b = a.bindings[i];

                    if(a == action && i == bindingIndex) continue;
                    if(b.isComposite) continue;
                    if(!BindingHasGroup(b.groups, groupName)) continue;

                    string existingPath = !string.IsNullOrWhiteSpace(b.effectivePath) ? b.effectivePath : 
                                !string.IsNullOrWhiteSpace(b.overridePath) ? b.overridePath : b.path;

                    if(string.Equals(existingPath, newPath, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
        }

        return false;
    }
    
    // Check if the binding is actually in a valid group
    private static bool BindingHasGroup(string groups, string groupName)
    {
        if(string.IsNullOrWhiteSpace(groups) || string.IsNullOrWhiteSpace(groupName)) return false;

        var split = groups.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach(var g in split)
        {
            if(string.Equals(g.Trim(), groupName.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }
}