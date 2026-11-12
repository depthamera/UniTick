
namespace SamplePuzzle
{
    public struct DoorState
    {
        public SimId Id;
        public SimId OpenedBy;
        public bool Open;

        public DoorState(SimId id, SimId openedBy, bool open)
        {
            Id = id;
            OpenedBy = openedBy;
            Open = open;
        }
           
    }
}
