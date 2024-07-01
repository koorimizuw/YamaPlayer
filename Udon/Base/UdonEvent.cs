
using UdonSharp;

namespace Yamadev.YamaStream
{
    public class UdonEvent: UdonSharpBehaviour
    {
        public static UdonEvent Empty()
        {
            object[] result = new object[] { null, null };
            return (UdonEvent)(object)result;
        }

        public static UdonEvent New(UdonSharpBehaviour udon, string callback)
        {
            object[] result = new object[] { udon, callback };
            return (UdonEvent)(object)result;
        }
    }

    public static class UdonEventExtentions
    {
        public static void Invoke(this UdonEvent _udonEvent) 
        {
            UdonSharpBehaviour udon = (UdonSharpBehaviour)((object[])(object)_udonEvent)[0];
            string callback = (string)((object[])(object)_udonEvent)[1];
            if (udon != null) udon.SendCustomEvent(callback);
        }
    }
}