using System.Collections.Generic;
using UnityEngine;

namespace Yamadev.YamaStream
{
  public class PlaylistManager : MonoBehaviour
  {
    public List<PlaylistItem> GetPlaylists()
    {
      var results = new List<PlaylistItem>();
      for (int i = 0; i < transform.childCount; i++)
      {
        var item = transform.GetChild(i).GetComponent<PlaylistItem>();
        if (item != null) results.Add(item);
      }
      return results;
    }
  }
}