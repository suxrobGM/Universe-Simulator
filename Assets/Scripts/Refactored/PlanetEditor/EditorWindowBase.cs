using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class EditorWindowBase
{
    private PlanetComponent _currentPlanet;
    
    private bool _shown;
    public bool Shown
    {
        get => _shown;
        set => Show(value);
    }
    
    // graphics
    [SerializeField] private GameObject _editorContainer;
    [SerializeField] private Text _barText;
    [SerializeField] private string _windowTitle;
    [SerializeField] private UnityEvent _onEditedPlanetChanged;
    [SerializeField] private Vector2 _offsetToShow;
    private Vector2 _hiddenPos = new(-9000, -9000);

    public PlanetComponent CurrentPlanet
    {
        get => _currentPlanet;
        set
        {
            _currentPlanet = value;
            _barText.text = $"{_windowTitle} {_currentPlanet.Name}";
            _onEditedPlanetChanged.Invoke();
        }
    }

    void Show(bool target)
    {
        _shown = target;
        ChangeDisplay();
    }

    void ChangeDisplay()
    {
        if (_shown)
        {
            // set offset to always display in window, not outside
            float offsetX =
                Input.mousePosition.x - _offsetToShow.x >
                _editorContainer.GetComponent<RectTransform>().rect.width / 2 &&
                Input.mousePosition.x - _offsetToShow.x < Screen.currentResolution.width
                    ? _offsetToShow.x
                    : -_offsetToShow.x;
            
            float offsetY =
                Input.mousePosition.y - _offsetToShow.y >
                _editorContainer.GetComponent<RectTransform>().rect.height / 2 &&
                Input.mousePosition.y - _offsetToShow.y < Screen.currentResolution.height
                    ? _offsetToShow.y
                    : -_offsetToShow.y;
            
            _editorContainer.GetComponent<RectTransform>().position = Input.mousePosition - new Vector3(offsetX, offsetY);
            return;
        }

        _editorContainer.GetComponent<RectTransform>().position = _hiddenPos;
    }
}
