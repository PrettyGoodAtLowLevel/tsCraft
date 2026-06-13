using OpenTK.Mathematics;
using OurCraft.Physics;

namespace OurCraft.Terrain_Generation
{
    //empty for now since i dont have proper structure save system
    //boilerplate class
    public abstract class Structure
    {
        //max distance between structures = 256 chunks, min = 4 chunks
        public const int MAX_CELL_SIZE = 8192;
        public const int MIN_CELL_SIZE = 128;

        public int cellSize = MIN_CELL_SIZE;
        public int cellChance = 2; //1/2 chance of spawning per cell
        public AABB localStructureBounds;

        public string name = "Unknown Structure";
        public StructurePlacementType placementType;      
        //public StructureSchematic structureFile;

        public Structure() { }

        public bool CanPlaceStructure(Vector3i startPos)
        {
            return true;
        }
    }

    //start position of a structure + what structure variation
    public readonly struct StructureStart
    {
        public readonly Structure Structure;
        public readonly int Seed;
        public readonly Vector3i StartPos;

        //ctor
        public StructureStart(Structure Structure, int Seed, Vector3i StartPos)
        {
            this.Structure = Structure;
            this.Seed = Seed;
            this.StartPos = StartPos;
        }
    }

    //defines how a structure is placed relative to surrounding terrain
    public enum StructurePlacementType
    {
        FLAT,     //use for very flat areas or super flat worlds
        ADAPTIVE, //most common, adapts to terrain
    }
}