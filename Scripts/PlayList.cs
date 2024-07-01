
using UnityEngine;

namespace Yamadev.YamaStream.Script
{

    public class PlayList : MonoBehaviour
    {
        [SerializeField]
        string playListName;
        [SerializeField]
        Track[] tracks;

        public GameObject IwaSync3PlayList;
        public GameObject KienlPlayList;
        public string YouTubePlayListID;

        public string PlayListName => playListName;
        public Track[] Tracks => tracks;
    }
}