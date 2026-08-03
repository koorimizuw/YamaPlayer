using UnityEngine;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
  [RequireComponent(typeof(YamaPlayerModuleDefinition))]
  public abstract class YamaPlayerModule : YamaPlayerListener
  {
    [SerializeField, HideInInspector] protected Controller _controller;

    protected virtual bool IsSyncedModule => false;

    public virtual void Start()
    {
      if (!Utilities.IsValid(_controller))
      {
        PrintError($"Controller is not set in module {gameObject.name}");
        return;
      }
      _controller.AddListener(this);
    }

    public override void TakeOwnership()
    {
      base.TakeOwnership();
      if (IsSyncedModule && Utilities.IsValid(_controller))
      {
        _controller.TakeOwnership();
      }
    }

    public override void AfterOwnerChanged()
    {
      if (IsSyncedModule && Utilities.IsValid(_controller) && Networking.IsOwner(_controller.gameObject))
      {
        TakeOwnership();
      }
    }
  }
}