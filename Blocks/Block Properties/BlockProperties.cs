//contains a list of block state properties that blockstates can implement
namespace OurCraft.Blocks.Block_Properties
{
    //for log rotation
    public enum Axis : byte 
    {
        X = 0,
        Y = 1,
        Z = 2,
    }

    //for slab tytes
    public enum SlabType : byte 
    {
        Bottom = 0,
        Top = 1,
        Double = 2
    }
}
