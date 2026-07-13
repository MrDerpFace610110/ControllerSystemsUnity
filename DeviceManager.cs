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
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
// The input management between Keyboard & Mouse and a controller
/// </summary>
public static class DeviceManager
{
    //=== EVENTS ===
    public static event Action<int, InputType> OnInputTypeChanged;
    public static event Action<int, Gamepad> OnGamepadAssigned;

    //=== MULTIPLAYER CONFIG ===
    public const int MAX_SUPPORTED_PLAYERS = 4;
    public static int MaxExpectedPlayers { get; set; } = 1; // Set this in scene controller

    //=== STATE TRACKING ===
    public enum InputType
    {
        KeyboardMouse,
        Controller
    }

    public static Gamepad[] PlayerGamepads { get; private set; } = new Gamepad[MAX_SUPPORTED_PLAYERS];

    private static bool[] playerInputDisabled = new bool[MAX_SUPPORTED_PLAYERS];
    private static InputType[] lastUsedInput = new InputType[MAX_SUPPORTED_PLAYERS];
    private static int[] lastActiveGamepadId = new int[MAX_SUPPORTED_PLAYERS];

    // Setup the updater monobehaviour 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Intitialize()
    {
        GameObject updaterObject = new GameObject("StaticInputUpdater");
        updaterObject.AddComponent<InputUpdater>();
        GameObject.DontDestroyOnLoad(updaterObject);
        updaterObject.hideFlags = HideFlags.HideAndDontSave;

        // Initalize defaults
        for(int i = 0; i < MAX_SUPPORTED_PLAYERS; i++)
        {
            lastUsedInput[i] = InputType.KeyboardMouse;
            lastActiveGamepadId[i] = -1;
        }
    }

    #region Player State Management
    /// <summary>
    /// Set a specfic players input to enabled or disabled
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="isDisabled"></param>
    public static void SetPlayerInputDisabled(int playerIndex, bool isDisabled)
    {
        if (playerIndex >= 0 && playerIndex < MAX_SUPPORTED_PLAYERS)
        {
            playerInputDisabled[playerIndex] = isDisabled;
        }
    }

    /// <summary>
    /// Check if input is disabled for a player
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public static bool IsInputDisabledForPlayer(int playerIndex)
    {
        if(playerIndex < 0 || playerIndex >= MAX_SUPPORTED_PLAYERS) return true;
        return playerInputDisabled[playerIndex];
    }

    /// <summary>
    /// Get the last used input of a specfic player
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <returns></returns>
    public static InputType GetLastUsedInput(int playerIndex)
    {
        if(playerIndex < 0 || playerIndex >= MAX_SUPPORTED_PLAYERS) return InputType.KeyboardMouse;
        else return lastUsedInput[playerIndex];
    }

    // Switch players input type
    private static void SwitchInput(int playerIndex, InputType type)
    {
        if(lastUsedInput[playerIndex] != type)
        {
            lastUsedInput[playerIndex] = type;
            OnInputTypeChanged?.Invoke(playerIndex, type);
        }
    }
    #endregion

    #region Internal Class
    private class InputUpdater : MonoBehaviour
    {
        void OnEnable()
        {
            InputSystem.onEvent += OnInputEvent;
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void OnDisable()
        {
            InputSystem.onEvent -= OnInputEvent;
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if(!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;

            if(device is Gamepad gamepad)
            {
                int playerIndex = GetOrAssignPlayerIndex(gamepad);
                if(playerIndex == -1) return;

                if(gamepad.deviceId != lastActiveGamepadId[playerIndex])
                {
                    lastActiveGamepadId[playerIndex] = gamepad.deviceId;
                    OnGamepadAssigned?.Invoke(playerIndex, gamepad);
                }

                if(IsInputDisabledForPlayer(playerIndex)) return;

                SwitchInput(playerIndex, InputType.Controller);
            }
            else if(device is Keyboard || device is Mouse)
            {
                if(IsInputDisabledForPlayer(0)) return;
                SwitchInput(0, InputType.KeyboardMouse);
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if(change == InputDeviceChange.Disconnected && device is Gamepad disconnectedGamepad)
            {
                for(int i = 0; i < MaxExpectedPlayers; i++)
                {
                    if(PlayerGamepads[i] != null && PlayerGamepads[i].deviceId == disconnectedGamepad.deviceId)
                    {
                        Debug.Log($"[DeviceManager] Player {i + 1}'s controller disconnected");

                        PlayerGamepads[i] = null;
                        lastActiveGamepadId[i] = -1;

                        OnGamepadAssigned?.Invoke(i, null);
                        break;
                    }
                }
            }
        }

        private int GetOrAssignPlayerIndex(Gamepad gamepad)
        {
            //already assigned
            for(int i = 0; i < MaxExpectedPlayers; i++)
            {
                if(PlayerGamepads[i] != null && PlayerGamepads[i].deviceId == gamepad.deviceId)
                {
                    return i;
                }
            }

            //not assigned
            for(int i = 0; i < MaxExpectedPlayers; i++)
            {
                if(PlayerGamepads[i] == null)
                {
                    PlayerGamepads[i] = gamepad;
                    Debug.Log($"[DeviceManager] {gamepad.displayName} assigned to Player {i + 1}");
                    return i;
                }
            }

            return -1;
        }
    }
    #endregion
}
