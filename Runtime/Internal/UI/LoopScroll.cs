using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
  [RequireComponent(typeof(ScrollRect))]
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class LoopScroll : UdonSharpBehaviour
  {
    [SerializeField] private GameObject _template;
    private ScrollRect _scrollRect;
    private bool _initialized;
    private Vector2 _position;
    private int _lineCount;
    private float _lineHeight;
    private int _length;
    private int[] _lastIndexes;
    private int[] _indexes;
    private UdonSharpBehaviour _callbackUdon;
    private string _callbackEventName;

    private const float SCROLL_THRESHOLD_SQR = 0.003f;

    void Start() => Initialize();

    public int LineCount => _lineCount;

    public int Length => _length;

    public void SetUp(int length, UdonSharpBehaviour callbackUdon, string callbackEventName)
    {
      if (!_initialized) Initialize();
      _length = length;
      _callbackUdon = callbackUdon;
      _callbackEventName = callbackEventName;
      ResetValues();
      Render();
    }

    private void ResetValues()
    {
      if (!_initialized) return;

      _indexes = new int[_lineCount].Populate(-1);
      for (int i = 0; i < _lineCount; i++)
      {
        _scrollRect.content.GetChild(i).gameObject.SetActive(false);
      }
    }

    private void Render()
    {
      AdjustHeight();
      UpdateIndexes();
      UpdatePosition();
      InvokeCallback();
    }

    public void ScrollToTop()
    {
      Initialize();
      _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, 0);
      Render();
    }

    public int[] LastIndexes => _lastIndexes;
    public int[] Indexes => _indexes;

    private void Initialize()
    {
      if (_initialized) return;
      _scrollRect = GetComponent<ScrollRect>();

      if (!Utilities.IsValid(_template) && Utilities.IsValid(_scrollRect) && Utilities.IsValid(_scrollRect.content))
      {
        if (_scrollRect.content.childCount <= 0) return;
        _template = _scrollRect.content.GetChild(0).gameObject;
      }

      if (!Utilities.IsValid(_template)) return;

      _template.SetActive(false);
      _lineHeight = _template.GetComponent<RectTransform>().rect.height;

      if (_lineHeight <= 0) return;

      int count = Mathf.CeilToInt(_scrollRect.GetComponent<RectTransform>().rect.height / _lineHeight);
      for (int i = 0; i < count; i++)
      {
        GameObject obj = Instantiate(_template);
        obj.transform.SetParent(_scrollRect.content, false);
        obj.transform.SetSiblingIndex(_template.transform.GetSiblingIndex() + i + 1);
        obj.SetActive(false);
      }

      _lineCount = count + 1;
      _lastIndexes = new int[_lineCount].Populate(-1);
      _indexes = new int[_lineCount].Populate(-1);
      _initialized = true;
    }

    private void AdjustHeight()
    {
      if (!_initialized) Initialize();
      Vector2 size = _scrollRect.content.sizeDelta;
      size.y = _length * _lineHeight;

      for (int i = _lineCount; i < _scrollRect.content.childCount; i++)
      {
        RectTransform child = _scrollRect.content.GetChild(i).GetComponent<RectTransform>();
        child.anchoredPosition = new Vector2(child.anchoredPosition.x, -size.y);
        if (child.gameObject.activeSelf) size.y += child.rect.height;
      }
      _scrollRect.content.sizeDelta = size;
    }

    private void UpdateIndexes()
    {
      int offset = Mathf.FloorToInt(_scrollRect.content.anchoredPosition.y / _lineHeight);
      int[] indexes = new int[_lineCount];
      for (int i = 0; i < _lineCount; i++)
      {
        int target = offset / _lineCount * _lineCount + i;
        indexes[i] = target < offset ? target + _lineCount : target;
        if (indexes[i] < 0 || indexes[i] > _length - 1) indexes[i] = -1;
      }
      _lastIndexes = _indexes;
      _indexes = indexes;
    }

    private void UpdatePosition()
    {
      for (int i = 0; i < _lineCount; i++)
      {
        if (_indexes[i] == _lastIndexes[i]) continue;
        _scrollRect.content.GetChild(i).gameObject.SetActive(_indexes[i] != -1);
        RectTransform rect = _scrollRect.content.GetChild(i).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -(_indexes[i] * _lineHeight));
      }
    }

    public void OnScroll()
    {
      if (!_initialized) Initialize();
      if ((_position - _scrollRect.content.anchoredPosition).sqrMagnitude <= SCROLL_THRESHOLD_SQR) return;
      _position = _scrollRect.content.anchoredPosition;
      UpdateIndexes();
      UpdatePosition();
      InvokeCallback();
    }

    private void InvokeCallback()
    {
      if (Utilities.IsValid(_callbackUdon) && !string.IsNullOrEmpty(_callbackEventName))
      {
        _callbackUdon.SendCustomEvent(_callbackEventName);
      }
    }
  }
}