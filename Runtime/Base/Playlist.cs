
using UdonSharp;

namespace Yamadev.YamaStream
{
    public abstract class Playlist : UdonSharpBehaviour
    {
        public virtual string PlaylistName { get; }
        public virtual int Length { get; }
        public virtual bool IsLoading => false;
        public virtual bool Loaded => true;
        public virtual Track GetTrack(int index) { return null; }
        public virtual void AddTrack(Track track) { }
        public virtual void Remove(int index) { }
    }
}