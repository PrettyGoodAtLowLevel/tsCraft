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

        //initializes the bits for a Log block implementation
        static BlockLog()
        {
            var layout = new PropertyLayoutBuilder();
            AXIS = layout.AddEnum<Axis>();
        }

        //adds the properties to THIS instance of a block
        public BlockLog(string name, BlockShape shape, ushort id) :
        base(name, shape, id)
        {
            Properties.Add(AXIS);
        }

        //just add regular block to chunk, switch axis based on hit normal
        public override void PlaceBlockState(Vector3 globalPos, Vector3 hitNormal, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, BlockState thisBlock, ChunkManager world)
        {
            Axis axis = Axis.Y;
            if (Math.Abs(hitNormal.Y) > 0.5f) axis = Axis.Y;
            else if (Math.Abs(hitNormal.X) > 0.5f) axis = Axis.X;
            else if (Math.Abs(hitNormal.Z) > 0.5f) axis = Axis.Z;

            var stateToPlace = DefaultState.With(AXIS, axis); 
            world.SetBlock(globalPos + hitNormal, stateToPlace);
        }

        //interprets the axis of the log block
        public override void DebugState(BlockState thisBlock)
        {
            base.DebugState(thisBlock);
            Axis axis = thisBlock.GetProperty(AXIS);
            Console.WriteLine(", Axis: " + axis.ToString());
        }

        //log is a solid block, most likely
        public override bool IsLightPassable(BlockState state)
        {
            return false;
        }

        //log is not a light source
        public override bool IsLightSource(BlockState state)
        {
            return false;
        }
    }
}
