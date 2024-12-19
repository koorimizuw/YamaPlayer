
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Yamadev.YamaStream.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Modal : UdonSharpBehaviour
    {
        [SerializeField] Text _title;
        [SerializeField] Text _message;
        [SerializeField] ScrollRect _scrollRect;
        [SerializeField] Button _close;
        [SerializeField] Button _execute;
        [SerializeField] Button _execute2;
        [SerializeField] Text _closeText;
        [SerializeField] Text _executeText;
        [SerializeField] Text _execute2Text;
        [SerializeField] float _maxHeight;
        UdonEvent _closeEvent;
        UdonEvent _executeEvent;
        UdonEvent _execute2Event;
        RectTransform _scrollRectTransform;
        bool _initialized;

        private void initialize()
        {
            _closeEvent = UdonEvent.Empty();
            _executeEvent = UdonEvent.Empty();
            _execute2Event = UdonEvent.Empty();
            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
            _initialized = true;
        }

        private void Update() => AdaptMaxHeight();

        public UdonEvent CloseEvent
        {
            get
            {
                if (!_initialized) initialize();
                return _closeEvent;
            }
            set
            {
                if (!_initialized) initialize();
                _closeEvent = value;
            }
        }
        public UdonEvent ExecuteEvent
        {
            get
            {
                if (!_initialized) initialize();
                return _executeEvent;
            }
            set
            {
                if (!_initialized) initialize();
                _executeEvent = value;
            }
        }
        public UdonEvent Execute2Event
        {
            get
            {
                if (!_initialized) initialize();
                return _execute2Event;
            }
            set
            {
                if (!_initialized) initialize();
                _execute2Event = value;
            }
        }

        public bool IsActive => gameObject.activeSelf;

        public string Title
        {
            get => _title.text;
            set => _title.text = value;
        }

        public string Message
        {
            get => _message.text;
            set
            {
                _message.text = value;
                //AdaptMaxHeight();
            }
        }

        public string CancelText
        {
            get => _closeText.text;
            set => _closeText.text = value;
        }

        public string ExecuteText
        {
            get => _executeText.text;
            set => _executeText.text = value;
        }

        public string Execute2Text
        {
            get => _execute2Text.text;
            set => _execute2Text.text = value;
        }

        public void Open(int eventCount)
        {
            _execute.gameObject.SetActive(eventCount > 0);
            _execute2.gameObject.SetActive(eventCount > 1);
            //AdaptMaxHeight();
            gameObject.SetActive(true);
        }
        public void Close()
        {
            CloseEvent.Invoke();
            gameObject.SetActive(false);
        }
        public void Execute() => ExecuteEvent.Invoke();
        public void Execute2() => Execute2Event.Invoke();

        public void Show(string title, string message, UdonEvent callback, UdonEvent callback2, string closeText, string executeText, string execute2Text, bool showClose = true, bool showExecute = true, bool showExecute2 = true)
        {
            Title = title;
            Message = message;
            ExecuteEvent = callback;
            Execute2Event = callback2;
            _closeText.text = closeText;
            _executeText.text = executeText;
            _execute2Text.text = execute2Text;
            _close.gameObject.SetActive(showClose);
            _execute.gameObject.SetActive(showExecute);
            _execute2.gameObject.SetActive(showExecute2);
            gameObject.SetActive(true);
        }

        public void Show(string title, string message, UdonEvent callback, string closeText, string executeText, bool showClose = true, bool showExecute = true)
            => Show(title, message, callback, null, closeText, executeText, "", showClose, showExecute, false);

        public void AdaptMaxHeight()
        {
            float contentHeight = _scrollRect.content.sizeDelta.y;
            bool over = contentHeight > _maxHeight;
            _scrollRect.vertical = over;
            _scrollRectTransform.sizeDelta = new Vector2(_scrollRectTransform.sizeDelta.x, over ? _maxHeight : contentHeight);
        }
    }
}