using System;
using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Yamadev.YamaStream.Modules.PermissionManagement
{
  public enum PlayerPermission
  {
    Viewer,
    Editor,
    Admin,
    Owner
  }

  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  [DefaultExecutionOrder(-900)]
  public class PermissionManagement : YamaPlayerModule
  {
    [SerializeField] private PlayerPermission _defaultPermission = PlayerPermission.Editor;
    [SerializeField] private string[] _ownerList = new string[] { };
    [SerializeField] private bool _grantPermissionToInstanceOwner = true;
    [SerializeField] private bool _grantPermissionToInstanceMaster = true;
    [UdonSynced] private string _permissionString;
    private DataDictionary _permission = new DataDictionary();
    private YamaPlayerListener[] _listeners = new YamaPlayerListener[0];

    public PlayerPermission DefaultPermission => _defaultPermission;
    public string[] OwnerList => _ownerList;
    public bool GrantPermissionToInstanceOwner => _grantPermissionToInstanceOwner;
    public bool GrantPermissionToInstanceMaster => _grantPermissionToInstanceMaster;

    public override void Start()
    {
      base.Start();
      if (Networking.IsMaster) Initialize();
    }

    public DataDictionary PermissionData
    {
      get => _permission;
      set
      {
        _permission = value;

        if (IsObjectOwner && !_controller.IsLocal) RequestSerialization();
        int len = _listeners.Length;
        for (int i = 0; i < len; i++) _listeners[i].SendCustomEvent("AfterPermissionChanged");
        PrintLog("Permission updated.");
      }
    }

    public PlayerPermission PlayerPermission
    {
      get
      {
        if (!IsLocalPlayerValid) return PlayerPermission.Viewer;
        return GetPermissionByPlayerId(LocalPlayer.playerId);
      }
    }

    private void Initialize()
    {
      if (!IsLocalPlayerValid) return;

      var dict = new DataDictionary();
      dict.Add(LocalPlayer.playerId.ToString(), InitializePlayerPermission(LocalPlayer));
      TakeOwnership();
      PermissionData = dict;
    }

    private DataDictionary InitializePlayerPermission(VRCPlayerApi player)
    {
      int permission = (int)_defaultPermission;
      if (GrantPermissionToInstanceOwner && player.isInstanceOwner) permission = (int)PlayerPermission.Owner;
      if (GrantPermissionToInstanceMaster && player.isMaster) permission = (int)PlayerPermission.Owner;
      if (Array.IndexOf(_ownerList, player.displayName) >= 0) permission = (int)PlayerPermission.Owner;

      DataDictionary result = new DataDictionary();
      result.Add("displayName", player.displayName);
      result.Add("permission", permission);

      return result;
    }

    public void AddListener(YamaPlayerListener listener)
    {
      if (!Utilities.IsValid(listener) || Array.IndexOf(_listeners, listener) >= 0) return;
      _listeners = _listeners.Add(listener);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
      if (!IsObjectOwner) return;

      var dict = PermissionData;
      if (dict.ContainsKey(player.playerId.ToString())) return;
      dict.Add(player.playerId.ToString(), InitializePlayerPermission(player));
      PermissionData = dict;
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
      if (!IsObjectOwner) return;
      var dict = PermissionData;
      if (!dict.ContainsKey(player.playerId.ToString())) return;
      dict.Remove(player.playerId.ToString());
      PermissionData = dict;
    }

    public override void OnPreSerialization()
    {
      if (!VRCJson.TrySerializeToJson(_permission, JsonExportType.Minify, out var json))
      {
        PrintWarning("Failed to serialize permission data.");
        return;
      }
      _permissionString = json.String;
    }

    public override void OnDeserialization()
    {
      if (string.IsNullOrEmpty(_permissionString)) return;
      if (!VRCJson.TryDeserializeFromJson(_permissionString, out DataToken result)) return;
      if (result.TokenType != TokenType.DataDictionary) return;

      var dict = result.DataDictionary;
      var count = dict.Count;
      for (int i = 0; i < count; i++)
      {
        var entry = dict.GetValues()[i];
        if (entry.TokenType != TokenType.DataDictionary) continue;
        var innerDict = entry.DataDictionary;
        if (!innerDict.TryGetValue("permission", out DataToken permToken)) continue;
        innerDict.SetValue("permission", (int)permToken.Double);
      }

      PermissionData = dict;
    }

    public void SetPermission(int index, PlayerPermission permission)
    {
      if ((int)PlayerPermission < (int)PlayerPermission.Admin) return;
      if (index < 0 || index >= PermissionData.Count) return;

      var dict = PermissionData;
      DataToken key = dict.GetKeys()[index];
      int value = (int)permission;
      dict[key].DataDictionary["permission"] = value;
      PermissionData = dict;
    }

    public PlayerPermission GetPermissionByPlayerId(int playerId)
    {
      if (!PermissionData.ContainsKey(playerId.ToString())) return _defaultPermission;

      PermissionData.TryGetValue(playerId.ToString(), out var result);
      return (PlayerPermission)result.DataDictionary["permission"].Int;
    }
  }
}
