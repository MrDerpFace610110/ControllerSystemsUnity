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

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handle the rumbling of different players controllers
/// </summary>
public static class ControllerRumble
{
    private static int[] activePriority = new int[DeviceManager.MAX_SUPPORTED_PLAYERS];
    private static int[] currentRumbleId = new int[DeviceManager.MAX_SUPPORTED_PLAYERS];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Application.quitting += StopAllRumble;

        for(int i = 0; i < activePriority.Length; i++)
        {
            activePriority[i] = -1;
            currentRumbleId[i] = 0;
        }
    }

    /// <summary>
    /// Play a RumbleProfile for a specified player index
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="playerIndex"></param>
    public static void PlayProfile(RumbleProfile profile, int playerIndex = 0)
    {
        if(profile == null || playerIndex < 0 || playerIndex > 3) return;

        _ = RumbleCurve(profile.lowFreqCurve, profile.highFreqCurve, profile.duration, profile.priority, profile.isContinuous, playerIndex);
    }

    //Activate and handle the rumbling for specfic players
    private static async Task RumbleCurve(AnimationCurve lowFreqCurve, AnimationCurve highFreqCurve, float duration, int priority, bool isContinuous, int playerIndex)
    {
        if(priority < activePriority[playerIndex]) return;

        //check gamepad for current player
        Gamepad gamepad = DeviceManager.PlayerGamepads[playerIndex];
        if(gamepad == null) return;

        //make sure curves are set
        if (lowFreqCurve == null || highFreqCurve == null)
        {
            Debug.LogWarning("Rumble profile is missing Animation Curves.");
            return;
        }

        activePriority[playerIndex] = priority;
        int sessionId = ++currentRumbleId[playerIndex];

        float elapsed = 0f;
        while(isContinuous || elapsed < duration)
        {
            if(sessionId != currentRumbleId[playerIndex]) return;

            float normalTime;
            if(duration > 0)
            {
                normalTime = isContinuous ? (elapsed % duration) / duration : elapsed / duration;
            }
            else
            {
                normalTime = 0f;
            }

            float low = lowFreqCurve.Evaluate(normalTime);
            float high = highFreqCurve.Evaluate(normalTime);
            gamepad.SetMotorSpeeds(low, high);

            await Task.Yield();
            elapsed += Time.deltaTime;
        }

        if(sessionId == currentRumbleId[playerIndex])
        {
            StopRumble(playerIndex);
        }
    }

    /// <summary>
    /// Pause all rumble
    /// </summary>
    public static void PauseRumble()
    {
        InputSystem.PauseHaptics();
    }

    /// <summary>
    /// Resume all rumble
    /// </summary>
    public static void ResumeRumble()
    {
        InputSystem.ResumeHaptics();
    }

    /// <summary>
    /// Stop the rumble for a specfic player
    /// </summary>
    /// <param name="playerIndex"></param>
    public static void StopRumble(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 3) return;

        activePriority[playerIndex] = -1;
        currentRumbleId[playerIndex]++;

        Gamepad gamepad = DeviceManager.PlayerGamepads[playerIndex];
        if(gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
            gamepad.ResetHaptics();
        }
    }
    
    /// <summary>
    /// Stop rumble for all players
    /// </summary>
    public static void StopAllRumble()
    {
        for (int i = 0; i < DeviceManager.MAX_SUPPORTED_PLAYERS; i++)
        {
            StopRumble(i);
        }

        //safty catch
        foreach (var gamepad in Gamepad.all)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
            gamepad.ResetHaptics();
        }
    }
}
