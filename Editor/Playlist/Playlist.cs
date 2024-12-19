using System;
using System.Collections.Generic;
using Yamadev.YamaStream.Script;

namespace Yamadev.YamaStream.Editor
{
    [Serializable]
    public class Playlist
    {
        public bool Active = true;
        public string Name;
        public List<PlaylistTrack> Tracks = new();
        public string YoutubeListId;
        public bool IsEdit;
    }
}