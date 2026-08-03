using UdonSharp;
using UnityEngine;

namespace Yamadev.YamaStream.UI
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
  public class IndexTrigger : UdonSharpBehaviour
  {
    [SerializeField] private UdonSharpBehaviour _udon;
    [SerializeField] private string _variableName;
    [SerializeField] private string _variableValue;
    [SerializeField] private object _variableObject;
    [SerializeField] private string _eventName;

    public void OnButtonClick()
    {
      if (!string.IsNullOrEmpty(_variableValue))
      {
        _udon.SetProgramVariable(_variableName, _variableValue);
      }
      else
      {
        _udon.SetProgramVariable(_variableName, _variableObject);
      }

      _udon.SendCustomEvent(_eventName);
    }
  }
}