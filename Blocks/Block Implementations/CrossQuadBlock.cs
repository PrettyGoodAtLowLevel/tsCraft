using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Properties;
using OurCraft.World;

namespace OurCraft.Blocks.Block_Implementations
{
    //x shaped blocks like flowers and tall grass
    public class CrossQuadBlock : Block
    {
        public CrossQuadBlock(string name, BlockShape shape): base(name, shape)
        {
            IsRenderSolid = false;
        }

        //nothing special just add the block on the face the player is looking at
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world)
        {
            world.SetBlock(globalPos + hitNormal, DefaultState);
        }

        //light can pass through non full blocks
        public override bool IsLightPassable(BlockState state)
        {
            return true;
        }

        //light can pass through this block
        public override int GetSkyLightAttenuation(BlockState state)
        {
            return 0;
        }
    }
}
