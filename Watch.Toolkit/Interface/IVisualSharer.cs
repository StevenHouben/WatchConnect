namespace Watch.Toolkit.Interface
{
    public interface IVisualSharer
    {
        object GetVisual(int id);
        object GetThumbnail(int id);
        void RemoveThumbnail(int id);
        void SendThumbnail(object thumbnail,int id, double x, double y);
    }
}
