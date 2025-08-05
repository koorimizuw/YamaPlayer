﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

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
        private UdonEvent _callback;

        void Start() => Initialize();

        public UdonEvent CallbackEvent
        {
            get
            {
                if (!_initialized) Initialize();
                return _callback;
            }
            set
            {
                if (!_initialized) Initialize();
                _callback = value;
            }
        }

        public int LineCount => _lineCount;

        public int Length
        {
            get => _length;
            set
            {
                if (!_initialized) Initialize();
                _length = value;
                ResetValues();
                Render();
            }
        }

        public void ResetValues()
        {
            _indexes = new int[_lineCount].Populate(-1);
            for (int i = 0; i < _lineCount; i++)
                _scrollRect.content.GetChild(i).gameObject.SetActive(false);
        }

        public void Render()
        {
            AdjustHeight();
            UpdateIndexes();
            UpdatePosition();
            _callback.Invoke();
        }

        public void ScrollToTop()
        {
            _scrollRect.content.anchoredPosition = new Vector2(_scrollRect.content.anchoredPosition.x, 0);
            Render();
        }

        public int[] LastIndexes => _lastIndexes;
        public int[] Indexes => _indexes;

        public void Initialize()
        {
            if (_initialized) return;
            _scrollRect = GetComponent<ScrollRect>();
            if (_template == null) _template = _scrollRect.content.GetChild(0).gameObject;
            _template.SetActive(false);

            _lineHeight = _template.GetComponent<RectTransform>().rect.height;
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
            _callback = UdonEvent.Empty();
            _initialized = true;
        }

        public void AdjustHeight()
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

        public void UpdateIndexes()
        {
            int offset = Mathf.FloorToInt(_scrollRect.content.anchoredPosition.y / _lineHeight);
            int[] indexes = new int[_lineCount];
            for (int i = 0; i < _lineCount; i++)
            {
                int target = (offset / _lineCount) * _lineCount + i;
                indexes[i] = target < offset ? target + _lineCount : target;
                if (indexes[i] < 0 || indexes[i] > _length - 1) indexes[i] = -1;
            }
            _lastIndexes = _indexes;
            _indexes = indexes;
        }

        public void UpdatePosition()
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
            if ((_position - _scrollRect.content.anchoredPosition).sqrMagnitude <= 0.003f) return;
            _position = _scrollRect.content.anchoredPosition;
            UpdateIndexes();
            UpdatePosition();
            _callback.Invoke();
        }
    }
}