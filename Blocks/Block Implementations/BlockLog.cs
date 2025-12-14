using OpenTK.Mathematics;
using OurCraft.Graphics;
using OurCraft.World;
using OurCraft.Blocks.Block_Properties;

namespace OurCraft.Blocks.Block_Implementations
{
    //like a full block, but can be face in different axises
    public class BlockLog : FullBlock
    {
        //specifies the direction the log is oriented
        public static readonly EnumProperty<Axis> AXIS;

        //initializes the bits for a Log block
        static BlockLog()
        {
            var layout = new PropertyLayoutBuilder();
            AXIS = layout.AddEnum<Axis>();
        }

        public BlockLog(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        { }


        //just add regular block to chunk
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, Chunkmanager world)
        {
            if (hitNormal.Y != 0)
                world.SetBlock(globalPos + hitNormal, new BlockState((byte)id).WithProperty(AXIS, Axis.Y));

            else if (hitNormal.X != 0)
                world.SetBlock(globalPos + hitNormal, new BlockState((byte)id).WithProperty(AXIS, Axis.X));

            else if (hitNormal.Z != 0)
                world.SetBlock(globalPos + hitNormal, new BlockState((byte)id).WithProperty(AXIS, Axis.Z));
        }

        public override bool IsLightPassable(BlockState state)
        {
            return false;
        }

        public override bool IsLightSource(BlockState state)
        {
            return false;
        }
    }
}
