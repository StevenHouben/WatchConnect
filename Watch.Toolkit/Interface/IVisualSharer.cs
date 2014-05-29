using System.Security.Cryptography.X509Certificates;

namespace Watch.Toolkit.Interface
{
    public interface IVisualSharer
    {
        object GetVisual();
        object GetThumbnail();
        void SendThumbnail(object thumbnail);
        void SendVisual(object visual);
    }
}
