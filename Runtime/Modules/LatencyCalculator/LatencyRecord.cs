
using UnityEngine;
using UdonSharp;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class LatencyRecord : UdonSharpBehaviour
    {
        [SerializeField] LatencyManager _manager;
        [UdonSynced, FieldChangeCallback(nameof(ServerTime))] int _serverTime;
        [UdonSynced] string _records;
        DataDictionary _recordsDict = new DataDictionary();

        void Start()
        {
            if (Networking.LocalPlayer == null || Networking.LocalPlayer.playerId != Index) return;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            // RequestRecord();
        }

        public int Index => transform.GetSiblingIndex();
        public DataDictionary Records => _recordsDict;

        public int ServerTime
        {
            get => _serverTime;
            set
            {
                if (Networking.LocalPlayer.playerId > Index || value == 0) return;
                _serverTime = value;
                int delay = Networking.GetServerTimeInMilliseconds() - value;
                _manager.UpdateLatencyRecords(Index, delay);
            }
        }

        public void RequestRecord()
        {
            _serverTime = Networking.GetServerTimeInMilliseconds();
            RequestSerialization();
        }

        public void SendRecord()
        {
            VRCJson.TrySerializeToJson(_recordsDict, JsonExportType.Minify, out var result);
            _records = result.String;
            _serverTime = 0;
            RequestSerialization();
        }

        public override void OnDeserialization()
        {
            if (VRCJson.TryDeserializeFromJson(_records, out var result))
            {
                _recordsDict = result.DataDictionary;
                _manager.OnReceiveRecords();
                return;
            }
            _recordsDict = new DataDictionary();
        }
    }
}