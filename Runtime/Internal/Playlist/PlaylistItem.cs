
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Yamadev.YamaStream
{
  [Serializable]
  public class PlaylistTrack
  {
    [FormerlySerializedAs("Mode")] public VideoPlayerType playerType;
    [FormerlySerializedAs("Title")] public string title;
    [FormerlySerializedAs("Url")] public string url;
  }

  public class PlaylistItem : MonoBehaviour
  {
    [FormerlySerializedAs("playListName")] public string playlistName;
    public PlaylistTrack[] tracks;

    [FormerlySerializedAs("YouTubePlayListID")] public string youtubePlaylistId;
  }
}