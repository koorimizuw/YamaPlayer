using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class Modal : UdonSharpBehaviour
  {
    [SerializeField] private Text _titleText;
    [SerializeField] private Text _messageText;
    [SerializeField, RegisterEvent(nameof(Button.onClick), nameof(Close))] private Button _closeButton;
    [SerializeField, RegisterEvent(nameof(Button.onClick), nameof(Execute))] private Button _executeButton;
    [SerializeField, RegisterEvent(nameof(Button.onClick), nameof(Execute2))] private Button _execute2Button;
    [SerializeField] private Text _closeText, _executeText, _execute2Text;
    [SerializeField] private float _maxHeight = 400f;
    [SerializeField] private ScrollRect _scrollRect;
    private UdonSharpBehaviour _targetUdon;
    private string _closeEventName, _executeEventName, _execute2EventName;
    private RectTransform _scrollRectTransform;

    private void Start()
    {
      if (Utilities.IsValid(_scrollRect))
      {
        _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
      }
    }

    private void ExecuteAndClose(string eventName)
    {
      if (Utilities.IsValid(_targetUdon) && !string.IsNullOrEmpty(eventName))
      {
        _targetUdon.SendCustomEvent(eventName);
      }
      gameObject.SetActive(false);
    }

    public void Close() => ExecuteAndClose(_closeEventName);
    public void Execute() => ExecuteAndClose(_executeEventName);
    public void Execute2() => ExecuteAndClose(_execute2EventName);

    public void Show(string title, string message, string closeText, string executeText, UdonSharpBehaviour targetUdon, string closeEventName, string executeEventName)
    {
      Show(title, message, closeText, executeText, "", targetUdon, closeEventName, executeEventName, "");
    }

    public void Show(string title, string message, string closeText, string executeText, string execute2Text, UdonSharpBehaviour targetUdon, string closeEventName, string executeEventName, string execute2EventName)
    {
      if (Utilities.IsValid(_titleText)) _titleText.text = title;
      if (Utilities.IsValid(_messageText)) _messageText.text = message;
      if (Utilities.IsValid(_closeText)) _closeText.text = closeText;
      if (Utilities.IsValid(_executeText)) _executeText.text = executeText;
      if (Utilities.IsValid(_execute2Text)) _execute2Text.text = execute2Text;
      _targetUdon = targetUdon;
      _closeEventName = closeEventName;
      _executeEventName = executeEventName;
      _execute2EventName = execute2EventName;

      if (Utilities.IsValid(_executeButton)) _executeButton.gameObject.SetActive(Utilities.IsValid(targetUdon) && !string.IsNullOrEmpty(executeEventName));
      if (Utilities.IsValid(_execute2Button)) _execute2Button.gameObject.SetActive(Utilities.IsValid(targetUdon) && !string.IsNullOrEmpty(execute2EventName));

      gameObject.SetActive(true);
      SendCustomEventDelayedFrames(nameof(AdaptMaxHeight), 3);
    }

    public void AdaptMaxHeight()
    {
      if (!Utilities.IsValid(_scrollRect) || !Utilities.IsValid(_scrollRect.content) || !Utilities.IsValid(_scrollRectTransform)) return;
      if (_maxHeight <= 0) return;

      float contentHeight = _scrollRect.content.sizeDelta.y;
      bool over = contentHeight > _maxHeight;
      _scrollRect.vertical = over;
      _scrollRectTransform.sizeDelta = new Vector2(_scrollRectTransform.sizeDelta.x, over ? _maxHeight : contentHeight);
      _scrollRect.verticalNormalizedPosition = 1f;
    }
  }
}