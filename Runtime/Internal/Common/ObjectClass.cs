using UdonSharp;

namespace Yamadev.YamaStream
{
    public class ObjectClass : UdonSharpBehaviour { }

    public static class ObjectClassExtensions
    {
        public static T ForceCast<T>(this object[] objectArray)
        {
            return (T)(object)objectArray;
        }

        public static object[] UnPack(this ObjectClass oc)
        {
            return (object[])(object)oc;
        }
    }
}