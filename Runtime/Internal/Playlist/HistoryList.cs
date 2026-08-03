using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System;

namespace Yamadev.YamaStream
{
  [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
  public class HistoryList : YamaPlayerListener
  {
    [SerializeField] private Controller _controller;
    [UdonSynced] private VideoPlayerType[] _videoPlayerTypes = new VideoPlayerType[0];
    [UdonSynced] private string[] _titles = new string[0];
    [UdonSynced] private VRCUrl[] _urls = new VRCUrl[0];
    [UdonSynced] private byte[] _extensionsBlob = new byte[0];
    [UdonSynced] private int[] _extensionOffsets = new int[0];
    private object[][] _tracks = new object[0][];

    private void Start()
    {
      if (!Utilities.IsValid(_controller))
      {
        PrintError($"Controller is not assigned to HistoryList: {gameObject.name}");
        return;
      }
      _controller.AddListener(this);
    }

    private void GenerateTracks()
    {
      var trackCount = _urls.Length;
      bool extensionsValid = _extensionsBlob != null && _extensionOffsets != null && _extensionOffsets.Length == trackCount + 1;
      _tracks = new object[trackCount][];
      for (int i = 0; i < trackCount; i++)
      {
        byte[] extension = null;
        if (extensionsValid)
        {
          int start = _extensionOffsets[i];
          int end = _extensionOffsets[i + 1];
          if (start >= 0 && end > start && end <= _extensionsBlob.Length)
          {
            extension = new byte[end - start];
            Buffer.BlockCopy(_extensionsBlob, start, extension, 0, extension.Length);
          }
        }
        _tracks[i] = extension == null
          ? TrackUtils.NewTrack(_videoPlayerTypes[i], _titles[i], _urls[i])
          : TrackUtils.NewTrackWithExtension(_videoPlayerTypes[i], _titles[i], _urls[i], extension);
      }
    }

    public object[][] Tracks => _tracks;

    public int TrackCount => _tracks.Length;

    public object[] GetTrack(int index)
    {
      if (index < 0 || index >= TrackCount) return TrackUtils.CreateEmptyTrack();
      return _tracks[index];
    }

    public void AddTrack(object[] track)
    {
      int currentLength = _tracks.Length;
      object[][] newTracks = new object[currentLength + 1][];
      for (int i = 0; i < currentLength; i++)
      {
        newTracks[i] = _tracks[i];
      }
      newTracks[currentLength] = track;
      _tracks = newTracks;

      if (Networking.IsOwner(_controller.gameObject) && !_controller.IsLocal)
      {
        RequestSerialization();
      }
      _controller.SendCustomVideoEvent(nameof(AfterHistoryUpdated));
    }

    public override void OnPreSerialization()
    {
      int trackCount = Tracks.Length;
      _videoPlayerTypes = new VideoPlayerType[trackCount];
      _titles = new string[trackCount];
      _urls = new VRCUrl[trackCount];
      _extensionOffsets = new int[trackCount + 1];

      int totalLength = 0;
      for (int i = 0; i < trackCount; i++)
      {
        object[] track = Tracks[i];
        _videoPlayerTypes[i] = TrackUtils.GetPlayerType(track);
        _titles[i] = TrackUtils.GetTitle(track);
        _urls[i] = TrackUtils.GetUrl(track);
        totalLength += TrackUtils.GetExtension(track).Length;
      }

      _extensionsBlob = new byte[totalLength];
      int cursor = 0;
      for (int i = 0; i < trackCount; i++)
      {
        _extensionOffsets[i] = cursor;
        byte[] extension = TrackUtils.GetExtension(Tracks[i]);
        Buffer.BlockCopy(extension, 0, _extensionsBlob, cursor, extension.Length);
        cursor += extension.Length;
      }
      _extensionOffsets[trackCount] = cursor;
    }

    public override void OnDeserialization()
    {
      GenerateTracks();
      _controller.SendCustomVideoEvent(nameof(AfterHistoryUpdated));
    }

    public override void TakeOwnership()
    {
      base.TakeOwnership();
      if (Utilities.IsValid(_controller)) _controller.TakeOwnership();
    }

    public override void AfterOwnerChanged()
    {
      if (Utilities.IsValid(_controller) && Networking.IsOwner(_controller.gameObject))
      {
        TakeOwnership();
      }
    }
  }
}
