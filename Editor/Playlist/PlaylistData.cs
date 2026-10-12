using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Yamadev.YamaStream.Editor
{
  [Serializable]
  public class PlaylistData
  {
    public bool active;
    public string name;
    public List<PlaylistTrack> tracks;
    public string youtubeListId;

    [JsonIgnore] public PlaylistItem originalItem;
    [JsonIgnore] public bool isNameEditing;

    public PlaylistData()
    {
      active = true;
      tracks = new List<PlaylistTrack>();
      youtubeListId = "";
    }

    public PlaylistData(PlaylistItem item)
    {
      originalItem = item;
      active = item.gameObject.activeSelf;
      name = item.playlistName;
      tracks = item.tracks?.ToList() ?? new List<PlaylistTrack>();
      youtubeListId = item.youtubePlaylistId ?? "";
      isNameEditing = false;
    }
  }
}