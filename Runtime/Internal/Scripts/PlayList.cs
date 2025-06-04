
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Yamadev.YamaStream.Script
{
    [Serializable]
    public class PlaylistTrack
    {
        public VideoPlayerType Mode;
        public string Title;
        public string Url;
        public PlayableDirector PlayableDirector;
    }

    public class PlayList : MonoBehaviour
    {
        [SerializeField] string playListName;
        [SerializeField] PlaylistTrack[] tracks;

        public GameObject IwaSync3PlayList;
        public GameObject KienlPlayList;
        public string YouTubePlayListID;

        public string PlayListName => playListName;
        public PlaylistTrack[] Tracks => tracks;
    }
}