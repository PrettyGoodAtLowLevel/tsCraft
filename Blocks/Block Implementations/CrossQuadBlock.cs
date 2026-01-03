using OurCraft.Graphics;
using OurCraft.World;
using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //x shaped blocks like flowers and tall grass
    public class CrossQuadBlock : Block
    {
        public CrossQuadBlock(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        {
            IsSolid = false;
        }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            world.SetBlock(globalPos + hitNormal, DefaultState);
        }

        public override void UpdateBlockState(Vector3 globalPos, BlockState thisBlock, Chunkmanager world)
        { }

        //light can pass through non full blocks
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //these are not light sources
        public override bool IsLightSource(BlockState state)
        {
            return false;
        }

        //light can pass through this block
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return 0;
        }
    }
}
