
using UnityEngine;
using UdonSharp;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Yamadev.YamaStream
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LatencyManager : UdonSharpBehaviour
    {
        [SerializeField] Controller _controller;
        int _lastPlayerId = 1;

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            _lastPlayerId = Mathf.Max(player.playerId, _lastPlayerId);
        }

        public void UpdateLatencyRecords(int playerId, int delay)
        {
            LatencyRecord target = transform.GetChild(Networking.LocalPlayer.playerId).GetComponent<LatencyRecord>();
            if (!target.Records.ContainsKey(playerId.ToString())) 
                target.Records.Add(playerId.ToString(), (double)delay);
            else target.Records.SetValue(playerId.ToString(), (double)delay);
            target.SendRecord();
        }

        public void RequestRecord()
        {
            LatencyRecord target = transform.GetChild(Networking.LocalPlayer.playerId).GetComponent<LatencyRecord>();
            target.RequestRecord();
        }

        public void OnReceiveRecords()
        {
            _controller.UpdateNetworkDelay();
        }

        public int GetDelayWithTwoPlayers(int player1, int player2)
        {
            DataToken result;
            if (player1 < player2)
            {
                LatencyRecord target = transform.GetChild(player1).GetComponent<LatencyRecord>();
                target.Records.TryGetValue(player2.ToString(), TokenType.Double, out result);
            }
            else
            {
                LatencyRecord target = transform.GetChild(player2).GetComponent<LatencyRecord>();
                target.Records.TryGetValue(player1.ToString(), TokenType.Double, out result);
            }
            if (result.TokenType == TokenType.Error) return -1;

            return (int)result.Double;
        }

        public DataDictionary GetMyLatencyRecords()
        {
            DataDictionary results = new DataDictionary();
            for (int i = 1; i <= _lastPlayerId; i++)
            {
                if (i == Networking.LocalPlayer.playerId) continue;
                int record = GetDelayWithTwoPlayers(Networking.LocalPlayer.playerId, i);
                if (record >= 0) results.Add(i, record);

            }
            return results;
        }

        public float GetServerDelayseconds()
        {
            DataDictionary records = GetMyLatencyRecords();
            if (records.Count < 2) return 0f;
            for (int x = records.Count - 1; x >= 0; x--)
            {
                for (int y = records.Count - 1; y >= 0; y--)
                {
                    if (x == y) continue;
                    int playerX = records.GetKeys()[x].Int;
                    int playerY = records.GetKeys()[y].Int;
                    int delayXY = GetDelayWithTwoPlayers(playerX, playerY);
                    if (delayXY != -1)
                    {
                        int delayX = records[playerX].Int;
                        int delayY = records[playerY].Int;

                        return (delayX + delayY - delayXY) / 2f / 1000f;
                    }
                }
            }
            return 0f;
        }
    }
}