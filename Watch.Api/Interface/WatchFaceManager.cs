using System;
using System.Collections.Generic;
using Watch.Api.Interface;

namespace WatchInterface.Visuals
{
    public class WatchFaceManager
    {
        readonly Dictionary<Guid,IWatchFace> _faces = new Dictionary<Guid, IWatchFace>(); 

        public void AddFace(IWatchFace face)
        {
            _faces.Add(face.Id,face);
        }

        public void RemoveFace(IWatchFace face)
        {
            _faces.Remove(face.Id);
        }

        public IWatchFace FindFace(Guid id)
        {
            return _faces[id];
        }
    }
}
