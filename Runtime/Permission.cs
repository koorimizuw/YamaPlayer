
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Permission : UdonSharpBehaviour
    {
        [SerializeField] Controller _controller;
        [SerializeField] PlayerPermission _defaultPermission = PlayerPermission.Editor;
        [SerializeField] string[] _ownerList = new string[] { };
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
            if (_ownerList.Length == 0) return player.isInstanceOwner;
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

        public PlayerPermission PlayerPermission => GetPermissionByPlayerId(Networking.LocalPlayer.playerId);

        DataDictionary initlizePlayerPermission(VRCPlayerApi player)
        {
            DataDictionary result = new DataDictionary();
            result.Add("displayName", player.displayName);
            result.Add("permission", IsPlayerOwner(player) ? (int)PlayerPermission.Owner : (int)_defaultPermission);
            return result;
        }

        void Initialize()
        {
            if (Networking.LocalPlayer == null) return;
            _permission.Add(Networking.LocalPlayer.playerId.ToString(), initlizePlayerPermission(Networking.LocalPlayer));
            _initialized = true;
            PermissionData = _permission;
            this.SyncVariables();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (_permission.ContainsKey(player.playerId.ToString())) return;
            _permission.Add(player.playerId.ToString(), initlizePlayerPermission(player));
            PermissionData = _permission;
            this.SyncVariables();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject)) return;
            if (!_permission.ContainsKey(player.playerId.ToString())) return;
            _permission.Remove(player.playerId.ToString());
            PermissionData = _permission;
            this.SyncVariables();
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
            this.SyncVariables();
        }

        public PlayerPermission GetPermissionByPlayerId(int playerId)
        {
            if (!_permission.ContainsKey(playerId.ToString())) return _defaultPermission;
            _permission.TryGetValue(playerId.ToString(), out var result);
            return (PlayerPermission)result.DataDictionary["permission"].Int;
        }
    }
}