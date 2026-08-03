using System;

namespace Yamadev.YamaStream.Editor
{
  internal static class EditorUtils
  {
    public static Type FindType(string typeName, bool useFullName = false, bool ignoreCase = false)
    {
      if (string.IsNullOrEmpty(typeName)) return null;
      StringComparison e = (ignoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
      foreach (var assemb in AppDomain.CurrentDomain.GetAssemblies())
        foreach (var t in assemb.GetTypes())
        {
          if (string.Equals(t.FullName, typeName, e)) return t;
          if (!useFullName && string.Equals(t.Name, typeName, e)) return t;
        }
      return null;
    }
  }
}
