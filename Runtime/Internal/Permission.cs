using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Permission : YamaPlayerBehaviour
    {
        [SerializeField] Controller _controller;
        [SerializeField] PlayerPermission _defaultPermission = PlayerPermission.Editor;
        [SerializeField] string[] _ownerList = new string[] { };
        [SerializeField] bool _grantPermissionToInstanceOwner = true;
        [SerializeField] bool _grantPermissionToInstanceMaster = true;
        [UdonSynced] string _permissionString;
        DataDictionary _permission = new DataDictionary();
        bool _initialized = false;
        void Start()
        {
            if (!Networking.IsMaster) return;
            Initialize();
        }

        public bool IsPlayerOwner(VRCPlayerApi player)
        {
            if (_grantPermissionToInstanceOwner && player.isInstanceOwner) return true;
            if (_grantPermissionToInstanceMaster && player.isMaster) return true;
            return Array.IndexOf(_ownerList, player.displayName) >= 0;
        }

        public DataDictionary PermissionData
        {
            get => _permission;
            set
            {
                _permission = value;
                _controller.SendCustomVideoEvent(nameof(Listener.OnPermissionChanged));
            }
        }

        public PlayerPermission PlayerPermission =>
            _isLocalPlayerValid ? GetPermissionByPlayerId(_localPlayer.playerId) : PlayerPermission.Viewer;

        DataDictionary initlizePlayerPermission(VRCPlayerApi player)
        {
            DataDictionary result = new DataDictionary();
            result.Add("displayName", player.displayName);
            result.Add("permission", IsPlayerOwner(player) ? (int)PlayerPermission.Owner : (int)_defaultPermission);
            return result;
        }

        void Initialize()
        {
            if (_localPlayer == null) return;
            _permission.Add(_localPlayer.playerId.ToString(), initlizePlayerPermission(_localPlayer));
            _initialized = true;
            PermissionData = _permission;
            SyncVariables();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!_isObjectOwner) return;
            if (_permission.ContainsKey(player.playerId.ToString())) return;
            _permission.Add(player.playerId.ToString(), initlizePlayerPermission(player));
            PermissionData = _permission;
            SyncVariables();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!_isObjectOwner) return;
            if (!_permission.ContainsKey(player.playerId.ToString())) return;
            _permission.Remove(player.playerId.ToString());
            PermissionData = _permission;
            SyncVariables();
        }

        public override void OnPreSerialization()
        {
            VRCJson.TrySerializeToJson(_permission, JsonExportType.Minify, out var json);
            _permissionString = json.String;
        }

        public override void OnDeserialization()
        {
            VRCJson.TryDeserializeFromJson(_permissionString, out DataToken result);
            _permission = result.DataDictionary;
            for (int i = 0; i < _permission.Count; i++)
            {
                double value = _permission.GetValues()[i].DataDictionary["permission"].Double;
                _permission.GetValues()[i].DataDictionary.SetValue("permission", (int)value);
            }

            _initialized = true;
            PermissionData = _permission;
        }

        public void SetPermission(int index, PlayerPermission permission)
        {
            DataToken key = _permission.GetKeys()[index];
            int value = (int)permission;
            _permission[key].DataDictionary["permission"] = value;
            PermissionData = _permission;
            SyncVariables();
        }

        public PlayerPermission GetPermissionByPlayerId(int playerId)
        {
            if (!_permission.ContainsKey(playerId.ToString())) return _defaultPermission;
            _permission.TryGetValue(playerId.ToString(), out var result);
            return (PlayerPermission)result.DataDictionary["permission"].Int;
        }
    }
}