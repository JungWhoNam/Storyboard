
namespace VMail.Viewer
{
    public interface IViewer
    {
        void OpenMessage(Story message);

        void SetState(Page page);

        void SetState(Transition t);

    }
}