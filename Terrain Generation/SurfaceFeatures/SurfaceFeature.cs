using OpenTK.Mathematics;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;

namespace OurCraft.Terrain_Generation.SurfaceFeatures
{
    //less than 16x16 size structures, can fit multiple times in a 32 by 32 chunk
    //each surface feature has a size so that the chunk can check if it can fit or not
    //it also checks if it is place able on the block you are trying to place on
    //it also has a overrideable method for how it places itself in the world
    public abstract class SurfaceFeature
    {
        public string Name { get; set; } = "new Surface Feature";
        public BlockState PlaceOn { get; set; }
        public BlockState AltPlaceOn { get; set; }

        public bool CanPlaceUnderWater { get; set; } = false; //not going to be implemented for now

        //checks if a surface feature can fit inside of a chunk or a position
        public virtual bool CanPlaceFeature(Vector3i startPos, Chunk chunk)
        {
            return false;
        }

        //place a feature at a certain position in a chunk, with a determinstic randomness for variation
        public virtual void PlaceFeature(Vector3i startPos, Chunk chunk)
        {

        }
    }

    //represents a biomes surface feature and how many times it can be placed
    public class BiomeSurfaceFeature
    {
        public SurfaceFeature feature;
        public int chance = 0; //0 out of 1 billion blocks (not accounting for the different axis's)

        public BiomeSurfaceFeature(SurfaceFeature feature, int chance)
        {
            this.feature = feature;
            this.chance = chance;
        }
    }
}
