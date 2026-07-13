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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles navigation between different UI menus like pop-ups, screens, and world ui
/// </summary>
public static class UINavigator
{
    public static event Action<UIMenu> OnMenuPushed;
    public static event Action<UIMenu> OnMenuPopped;

    public static GameObject firstSelectedGlobal;

    private static Stack<UIMenu> menuHistory = new();
    private static bool wasMenuPushedThisFrame = false;
    private static UINavigationUpdater updaterInstance;


    // Setup the navigation updater
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if(updaterInstance == null)
        {
            GameObject updaterObject = new GameObject("UINavigationUpdater");
            updaterInstance = updaterObject.AddComponent<UINavigationUpdater>();
            updaterObject.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(updaterObject);

            DeviceManager.OnInputTypeChanged += HandleInputTypeChanged;
        }
    }

    #region System Interaction
    /// <summary>
    /// Push a new UIMenu to the menu stack
    /// </summary>
    /// <param name="newMenu"></param>
    public static void PushMenu(UIMenu newMenu)
    {
        if(newMenu == null) return;

        if(menuHistory.Count > 0)
        {
            UIMenu current = menuHistory.Peek();
            if(current.type == MenuType.Screen && newMenu.type == MenuType.Screen)
            {
                current.SetInteractable(false);
            }
        }

        menuHistory.Push(newMenu);
        newMenu.SetState(true);
        newMenu.SetInteractable(true);

        RebuildNavigation(newMenu);
        FocusMenu(newMenu);

        wasMenuPushedThisFrame = true;
        OnMenuPushed?.Invoke(newMenu);
    }

    /// <summary>
    /// Go back to the previous UIMenu if there is one
    /// </summary>
    public static void GoBackMenu()
    {
        if(menuHistory.Count == 0) return;

        UIMenu current = menuHistory.Peek();

        if(!current.HandleBackRequest()) return;
        if(menuHistory.Count == 1 && !current.canCloseIfLast) return;
        
        menuHistory.Pop();
        current.SetInteractable(false);

        if(current.type != MenuType.World)
            current.SetState(false);

        current.OnMenuClosed();
        OnMenuPopped?.Invoke(current);

        if(menuHistory.Count > 0)
        {
            UIMenu previous = menuHistory.Peek();
            previous.SetState(true);
            previous.SetInteractable(true);

            FocusMenu(previous);
        }
    }

    /// <summary>
    /// Clear all menu history 
    /// </summary>
    public static void ClearMenuHistory()
    {
        menuHistory.Clear();
    }

    /// <summary>
    /// Get the UIMenu currently at the top of the stack
    /// </summary>
    /// <returns></returns>
    public static UIMenu GetCurrentMenu() => menuHistory.Count > 0 ? menuHistory.Peek() : null;
    #endregion

    #region Helper Methods
    // Handle the change in input from controller to keyboard & mouse
    private static void HandleInputTypeChanged(int playerIndex, DeviceManager.InputType type)
    {
        if (playerIndex != 0) return;

        if(type == DeviceManager.InputType.Controller)
        {
            EnableControllerNavigation();
        }
        else
        {
            EventSystem.current?.SetSelectedGameObject(null);
        }
    }

    private static void EnableControllerNavigation()
    {
        if(EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            GameObject toSelect = menuHistory.Count > 0 ? menuHistory.Peek().defaultSelected : firstSelectedGlobal;
            if (toSelect != null) EventSystem.current.SetSelectedGameObject(toSelect);
        }
    }

    //Rebuild the navigation between selectables. If you make custom navigation of UI its best to check those here
    private static void RebuildNavigation(UIMenu newMenu)
    {
        if (newMenu == null) return;

        Selectable[] allSelectables = newMenu.GetComponentsInChildren<Selectable>(true);
        foreach (Selectable select in allSelectables)
        {
            Navigation nav = select.navigation;
            if (nav.mode != Navigation.Mode.Explicit)
            {
                nav.mode = Navigation.Mode.Automatic;
                select.navigation = nav;
            }
        }
    }

    private static void FocusMenu(UIMenu menu)
    {
        if (DeviceManager.GetLastUsedInput(0) == DeviceManager.InputType.Controller && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(menu.defaultSelected);
        }
    }
    #endregion

    #region Internal Class
    private class UINavigationUpdater : MonoBehaviour
    {
        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DeviceManager.OnInputTypeChanged -= HandleInputTypeChanged;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            menuHistory.Clear();
        }

        void Update()
        {
            if (wasMenuPushedThisFrame)
            {
                wasMenuPushedThisFrame = false;
                return;
            }
            
            //hardcoded back 
            bool isCancelPressed = false;
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame) isCancelPressed = true;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) isCancelPressed = true;

            if (isCancelPressed)
            {
                GoBackMenu();
            }
        }
    }
    #endregion
}