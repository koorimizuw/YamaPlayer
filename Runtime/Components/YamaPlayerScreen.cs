using UnityEngine;
using UnityEngine.UI;

namespace Yamadev.YamaStream
{
  [DisallowMultipleComponent]
  [AddComponentMenu("YamaStream/YamaPlayer Screen")]
  public class YamaPlayerScreen : MonoBehaviour
  {
    public Controller controller;
    public string property = "_MainTex";

    public Object Reference
    {
      get
      {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
          return renderer;
        }
        else
        {
          var rawImage = GetComponent<RawImage>();
          if (rawImage != null)
          {
            return rawImage;
          }
        }
        return null;
      }
    }

    public ScreenType Type
    {
      get
      {
        switch (Reference)
        {
          case Renderer renderer:
            return ScreenType.Renderer;
          case RawImage rawImage:
            return ScreenType.RawImage;
          default:
            return ScreenType.Unknown;
        }
      }
    }

    public Shader Shader
    {
      get
      {
        switch (Type)
        {
          case ScreenType.Renderer:
            return (Reference as Renderer).sharedMaterial?.shader;
          case ScreenType.RawImage:
            return (Reference as RawImage).material?.shader;
          case ScreenType.Material:
            return (Reference as Material).shader;
          default:
            return null;
        }
      }
    }
  }
}