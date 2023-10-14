using FDB.Components.Settings;
using System;
using UnityEngine;
using UnityEngine.Events;

public class SettingsGUIComponent : MonoBehaviour
{
    [SerializeField] Vector2 _designResolution = new Vector2(800, 600);
    [SerializeField] Vector2 _pageSize = new Vector2(400, 400);

    public UnityAction OnClose;

    SettingsGUI<DefaultKeyData> _gui;

    public void Setup(string userId, Type settingsType)
    {
        var controller = SettingsController.Get(userId, settingsType);
        _gui = new SettingsGUI<DefaultKeyData>(GUIMode.Screen, controller, GetGuiSize, OnCloseHandler, null);
    }

    protected virtual void GetGuiSize(out Matrix4x4 matrix, out Vector2 screenSize, out Rect pageSize)
    {
        var screenScale = Mathf.Min(
            Screen.width / _designResolution.x,
            Screen.height / _designResolution.y);

        matrix = Matrix4x4.identity * Matrix4x4.Scale(new Vector3(screenScale, screenScale, 1));
        screenSize = new Vector2(Screen.width / screenScale, Screen.height / screenScale);
        pageSize = new Rect(
            (screenSize.x - _pageSize.x) / 2,
            (screenSize.y - _pageSize.y) / 2,
            _pageSize.x, 
            _pageSize.y);
    }

    private void OnCloseHandler()
    {
        OnClose?.Invoke();
    }

    protected virtual void Update()
    {
        _gui?.Update();
    }

    private void OnGUI()
    {
        _gui?.OnGUI();
    }
}
