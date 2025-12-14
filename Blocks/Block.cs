using OpenTK.Mathematics;
using OurCraft.Graphics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks
{
    //a refrence to mesh and texture data for a chunk to use when building their mesh
    //some blocks also have state properties that chunks hold onto for things like orientatation
    public abstract class Block
    {
        //block data
        protected string name = "";       
        protected ushort id = 0;
        public bool IsSolid = true;

        //texture ids
        public BlockShape blockShape;

        //ctr
        public Block(string name, BlockShape shape, ushort id)
        {
            this.name = name;
            this.id = id;
            blockShape = shape;       
        }

        //set id of block
        public void SetID(ushort id)
        {
            this.id = id;
        }

        //determines each blocks way of changing the chunk block state data when placed
        public virtual void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal,
        BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world) { }

        //determines each blocks way of changing the chunk block state data when placed
        public virtual void UpdateBlockState(Vector3 globalPos,
        BlockState thisBlock, Chunkmanager world) { }

        //determines if this block is a light source or not
        public virtual bool IsLightSource(BlockState state)
        {
            return false;
        }

        //determines if light can pass through this block
        public virtual bool IsLightPassable(BlockState state)
        {
            return false;
        }

        //gives you the light source value
        public virtual Vector3i GetLightSourceLevel(BlockState state)
        {
            return new Vector3i(0, 0, 0);
        }

        //gives you the skylight attenuation, 15 equals no light, 0 equals all light
        public virtual int GetLightAttenuation(BlockState state)
        {
            return 15;
        }

        //get other block info
        public string GetBlockName() { return name; }
    }
}