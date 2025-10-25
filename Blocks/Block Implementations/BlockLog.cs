using OpenTK.Mathematics;
using OurCraft.Rendering;
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

        //default constructor
        public BlockLog(string name, int bm, int t, int f, int b, int r, int l, ushort id) : base(name, bm, t, f, b, r, l, id) { }
        public BlockLog(string name, int tt, int st, ushort id) : base(name, tt, tt, st, st, st, st, id) { }

        //mesh implementation for a log block
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state)
        {
            Axis axis = state.GetProperty(AXIS);

            //add mesh type based on axis
            if (axis == Axis.X) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, state, rightFaceTex, leftFaceTex, frontFaceTex, backFaceTex, topFaceTex, topFaceTex, mesh);
            else if (axis == Axis.Z) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, state, rightFaceTex, leftFaceTex, topFaceTex, topFaceTex, frontFaceTex, backFaceTex, mesh);
            else if (axis == Axis.Y) BlockMeshBuilder.BuildFullBlock(pos, bottom, top, front, back, right, left, state, topFaceTex, topFaceTex, frontFaceTex, backFaceTex, rightFaceTex, leftFaceTex, mesh);
        }

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
    }
}
