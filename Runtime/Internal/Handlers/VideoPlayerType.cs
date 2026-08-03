namespace Yamadev.YamaStream
{
  public enum VideoPlayerType
  {
    UnityVideoPlayer,
    AVProVideoPlayer,
    ImageViewer,
    Other,
  }

  public static class VideoPlayerTypeExtensions
  {
    public static string GetString(this VideoPlayerType type)
    {
      switch (type)
      {
        case VideoPlayerType.UnityVideoPlayer: return "UnityVideoPlayer";
        case VideoPlayerType.AVProVideoPlayer: return "AVProVideoPlayer";
        case VideoPlayerType.ImageViewer: return "ImageViewer";
        case VideoPlayerType.Other: return "Other";
      }
      return string.Empty;
    }
  }
}
