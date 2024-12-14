
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace Yamadev.YamaStream.Editor
{
    public static class DefineSymbol
    {
        public static List<string> GetSymbols(BuildTargetGroup targetGroup)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup).Split(';').Select(s => s.Trim()).ToList();
        }
        
        public static bool Contains(BuildTargetGroup targetGroup, string symbol)
        {
            List<string> symbols = GetSymbols(targetGroup);
            return symbols.Contains(symbol);
        }

        public static void AddSymbol(BuildTargetGroup targetGroup, string symbol)
        {
            List<string> symbols = GetSymbols(targetGroup);
            if (!symbols.Contains(symbol)) symbols.Insert(0, symbol);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", symbols.ToArray()));
        }

        public static void AddSymbol(BuildTargetGroup[] targetGroups, string symbol)
        {
            foreach (var group in targetGroups) AddSymbol(group, symbol);
        }
    }
}