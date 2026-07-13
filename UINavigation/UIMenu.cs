using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Different types of menus. Used to define how each type is handled by the navigator
/// </summary>
public enum MenuType
{
    Screen,
    Popup,
    World
}

/// <summary>
/// Defines data and actions for each unique menu
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIMenu : MonoBehaviour
{
    [Tooltip("Type of UIMenu. Used to define specfic behaviours")] public MenuType type;
    [Tooltip("The default selected object when on this UIMenu")] public GameObject defaultSelected;
    [Tooltip("If the UIMenu is last int he stack can i be closed or not")] public bool canCloseIfLast = true;

    public UnityEvent OnMenuClosedEvent;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// Set if the GameObject is active or not
    /// </summary>
    /// <param name="isActive"></param>
    public virtual void SetState(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Set if the UI under the CanvasGroup is interactable
    /// </summary>
    /// <param name="interactable"></param>
    public virtual void SetInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    /// <summary>
    /// Refocus the EventSystem's currently selected object on the menus default
    /// </summary>
    public virtual void RefocusDefault()
    {
        if(defaultSelected == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(defaultSelected);
    }

    /// <summary>
    /// Fires code from the OnMenuClosedEvent UnityEvent when the menu is closed
    /// </summary>
    public virtual void OnMenuClosed()
    {
        OnMenuClosedEvent?.Invoke();
    }

    /// <summary>
    /// Custom handling of going back a menu
    /// </summary>
    /// <returns></returns>
    public virtual bool HandleBackRequest()
    {
        return true;
    }
}