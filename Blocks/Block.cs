using OpenTK.Mathematics;
using OurCraft.Rendering;
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

        //get other block info
        public string GetBlockName() { return name; }
    }
}