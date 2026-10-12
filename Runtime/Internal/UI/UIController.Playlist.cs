using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace Yamadev.YamaStream.UI
{
  public partial class UIController
  {
    [Header("Playlist - Settings")]
    [SerializeField] private bool _defaultPlaylistOpen;

    [Header("Playlist - UI Components")]
    [SerializeField] private Text _currentPlaylistNameText;
    [SerializeField] private LoopScroll _playlistsListScroll;
    [SerializeField] private LoopScroll _playlistTracksScroll;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(GenerateQueueView))] private Toggle _queueTabToggle;
    [SerializeField, RegisterEvent(nameof(Toggle.onValueChanged), nameof(GenerateHistoryView))] private Toggle _historyTabToggle;
    [SerializeField] private Toggle _playlistsTabToggle;
    private int _playlistIndex = -1;
    private int _playlistTrackIndex = -1;

    private bool IsQueuePage => Utilities.IsValid(_queueTabToggle) && _queueTabToggle.isOn;
    private bool IsHistoryPage => Utilities.IsValid(_historyTabToggle) && _historyTabToggle.isOn;

    public void GeneratePlaylistView()
    {
      if (!Utilities.IsValid(_playlistsListScroll)) return;
      _playlistsListScroll.SetUp(_controller.Playlists.Length, this, nameof(UpdatePlaylistsContent));
    }

    public void UpdatePlaylistsContent()
    {
      for (int i = 0; i < _playlistsListScroll.LineCount; i++)
      {
        if (_playlistsListScroll.Indexes[i] == _playlistsListScroll.LastIndexes[i] || _playlistsListScroll.Indexes[i] == -1) continue;
        var cell = _playlistsListScroll.GetComponent<ScrollRect>().content.GetChild(i);
        var playlist = _controller.Playlists[_playlistsListScroll.Indexes[i]];

        var n = cell.Find("Text");
        if (Utilities.IsValid(n))
        {
          var name = n.GetComponent<Text>();
          if (Utilities.IsValid(name))
            name.text = _controller.Playlists[_playlistsListScroll.Indexes[i]].PlaylistName;
        }

        var tr = cell.Find("TrackCount");
        if (Utilities.IsValid(tr))
        {
          var trackCount = tr.GetComponent<Text>();
          if (Utilities.IsValid(trackCount))
            trackCount.text = playlist.TrackCount > 0 ? $"{GetTranslation("label.total")} {playlist.TrackCount} {GetTranslation("label.tracks")}" : string.Empty;
        }

        var trigger = cell.GetComponent<IndexTrigger>();
        if (Utilities.IsValid(trigger)) trigger.SetProgramVariable("_variableObject", _playlistsListScroll.Indexes[i]);
      }
    }

    public void GenerateQueueView()
    {
      if (!IsQueuePage || !Utilities.IsValid(_playlistTracksScroll)) return;
      _playlistTracksScroll.SetUp(_controller.Queue.TrackCount, this, nameof(UpdatePlaylistTracksContent));
    }

    public void GenerateHistoryView()
    {
      if (!IsHistoryPage || !Utilities.IsValid(_playlistTracksScroll)) return;
      _playlistTracksScroll.SetUp(_controller.History.TrackCount, this, nameof(UpdatePlaylistTracksContent));
    }

    public void GeneratePlaylistsView()
    {
      if (!Utilities.IsValid(_playlistsTabToggle) || !_playlistsTabToggle.isOn || !Utilities.IsValid(_playlistsListScroll) || _playlistIndex < 0) return;
      _playlistsListScroll.SetUp(_controller.Playlists.Length, this, nameof(UpdatePlaylistsContent));
    }

    public void GeneratePlaylistTracks()
    {
      if (!Utilities.IsValid(_playlistsListScroll) || !Utilities.IsValid(_playlistTracksScroll)) return;
      if (!IsQueuePage && !IsHistoryPage && _playlistIndex < 0) return;

      int trackCount;
      if (IsQueuePage)
      {
        trackCount = _controller.Queue.TrackCount;
      }
      else if (IsHistoryPage)
      {
        trackCount = _controller.History.TrackCount;
      }
      else if (_playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length)
      {
        trackCount = _controller.Playlists[_playlistIndex].TrackCount;
      }
      else return;

      if (Utilities.IsValid(_currentPlaylistNameText) && _playlistsTabToggle.isOn)
      {
        _currentPlaylistNameText.text = _controller.Playlists[_playlistIndex].PlaylistName;
      }

      _playlistTracksScroll.SetUp(trackCount, this, nameof(UpdatePlaylistTracksContent));
    }

    public void UpdatePlaylistTracksContent()
    {
      object[][] tracks;
      if (IsQueuePage)
      {
        tracks = _controller.Queue.Tracks;
      }
      else if (IsHistoryPage)
      {
        tracks = _controller.History.Tracks;
      }
      else if (_playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length)
      {
        tracks = _controller.Playlists[_playlistIndex].Tracks;
      }
      else return;

      for (int i = 0; i < _playlistTracksScroll.LineCount; i++)
      {
        if (_playlistTracksScroll.Indexes[i] == _playlistTracksScroll.LastIndexes[i] || _playlistTracksScroll.Indexes[i] == -1) continue;
        if (_playlistTracksScroll.Indexes[i] >= tracks.Length) continue;

        var track = tracks[_playlistTracksScroll.Indexes[i]];
        var trackTitle = TrackUtils.GetTitle(track);
        var trackUrl = TrackUtils.GetUrl(track);
        bool isPlaying = !IsQueuePage && !IsHistoryPage && _playlistIndex == _controller.ActivePlaylistIndex && _playlistTracksScroll.Indexes[i] == _controller.PlayingTrackIndex;

        var cell = _playlistTracksScroll.GetComponent<ScrollRect>().content.GetChild(i);
        var info = cell.Find("Info");
        if (Utilities.IsValid(info))
        {
          var ti = info.Find("Title");
          if (Utilities.IsValid(ti))
          {
            var title = ti.GetComponent<Text>();
            if (Utilities.IsValid(title))
            {
              title.text = string.IsNullOrEmpty(trackTitle) ? trackUrl.Get() : trackTitle;
              title.color = isPlaying ? _primaryColor : Color.white;
            }
          }
          var u = info.Find("Url");
          if (Utilities.IsValid(u))
          {
            var urlText = u.GetComponent<Text>();
            if (Utilities.IsValid(urlText))
              urlText.text = string.IsNullOrEmpty(trackTitle) ? string.Empty : trackUrl.Get();
          }
          var no = info.Find("No");
          if (Utilities.IsValid(no))
          {
            var numberText = no.GetComponent<Text>();
            if (Utilities.IsValid(numberText))
            {
              numberText.text = $"{_playlistTracksScroll.Indexes[i] + 1}";
              numberText.gameObject.SetActive(!isPlaying);
            }
          }
          var playingMark = info.Find("PlayingMark");
          if (Utilities.IsValid(playingMark)) playingMark.gameObject.SetActive(isPlaying);
        }
        var actions = cell.Find("Actions");
        if (Utilities.IsValid(actions))
        {
          var upMark = actions.Find("Up");
          if (Utilities.IsValid(upMark)) upMark.gameObject.SetActive(IsQueuePage);
          var downMark = actions.Find("Down");
          if (Utilities.IsValid(downMark)) downMark.gameObject.SetActive(IsQueuePage);
          var removeMark = actions.Find("Remove");
          if (Utilities.IsValid(removeMark)) removeMark.gameObject.SetActive(IsQueuePage);
          var copyUrl = actions.Find("Copy");
          if (Utilities.IsValid(copyUrl))
          {
            var urlTransform = copyUrl.Find("URL");
            if (Utilities.IsValid(urlTransform))
            {
              var trackUrlText = urlTransform.GetComponent<InputField>();
              if (Utilities.IsValid(trackUrlText))
              {
                copyUrl.gameObject.SetActive(!IsQueuePage);
                trackUrlText.text = trackUrl.Get();
              }
            }
          }
          var addMark = actions.Find("Add");
          if (Utilities.IsValid(addMark)) addMark.gameObject.SetActive(!IsQueuePage);
          var PlayMark = actions.Find("Play");
          if (Utilities.IsValid(PlayMark)) PlayMark.gameObject.SetActive(!IsQueuePage);
        }
        var ani = cell.GetComponent<Animator>();
        if (Utilities.IsValid(ani)) ani.SetTrigger("Reset");
        var trigger = cell.GetComponent<IndexTrigger>();
        if (Utilities.IsValid(trigger)) trigger.SetProgramVariable("_variableObject", _playlistTracksScroll.Indexes[i]);
      }
    }

    public void RemoveFromQueue()
    {
      if (!InvokeBeforeEvent("BeforeUserRemoveTrackFromQueue")) return;
      if (!_playlistTracksScroll || _playlistTrackIndex < 0) return;

      _controller.Queue.TakeOwnership();
      if (_playlistTrackIndex < _controller.Queue.TrackCount) _controller.Queue.RemoveTrack(_playlistTrackIndex);
    }

    public void AddPlaylistTrackToQueue()
    {
      if (!InvokeBeforeEvent("BeforeUserAddTrackToQueue")) return;
      if (!_playlistTracksScroll || _playlistTrackIndex < 0) return;

      object[] track;
      if (IsHistoryPage)
      {
        track = _controller.History.GetTrack(_playlistTrackIndex);
      }
      else if (_playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length)
      {
        track = _controller.Playlists[_playlistIndex].GetTrack(_playlistTrackIndex);
      }
      else return;

      _controller.TakeOwnership();
      _controller.Queue.AddTrack(track);
    }

    public void MoveUp()
    {
      if (!InvokeBeforeEvent("BeforeUserMoveTrackUp")) return;
      _controller.TakeOwnership();
      _controller.Queue.MoveUp(_playlistTrackIndex);
    }

    public void MoveDown()
    {
      if (!InvokeBeforeEvent("BeforeUserMoveTrackDown")) return;
      _controller.TakeOwnership();
      _controller.Queue.MoveDown(_playlistTrackIndex);
    }

    public void PlayPlaylistTrack()
    {
      if (!InvokeBeforeEvent("BeforeUserPlayTrack")) return;
      if (!_playlistTracksScroll || _playlistTrackIndex < 0) return;

      if (IsHistoryPage)
      {
        _controller.TakeOwnership();
        _controller.PlayTrackFromHistory(_playlistTrackIndex);
        return;
      }

      if (_playlistIndex >= 0 && _playlistIndex < _controller.Playlists.Length)
      {
        _controller.TakeOwnership();
        _controller.PlayTrack(_controller.Playlists[_playlistIndex], _playlistTrackIndex);
        GeneratePlaylistTracks();
      }
    }
  }
}