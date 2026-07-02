using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.World;

namespace OurCraft.Blocks.Meshing.ModelShapes
{
    //interpets the log blockstate and adds the correct model based on that
    public class BlockLogModelShape : BlockShape
    {
        public CachedBlockModel cachedModelX = new();
        public CachedBlockModel cachedModelY = new();
        public CachedBlockModel cachedModelZ = new();

        public BlockLogModelShape()
        {
            IsFullOpaqueBlock = false;
        }

        //add log type mesh
        public override void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, ChunkSectionNeighbors nc)
        {
            Axis axis = nb.thisState.GetProperty(BlockLog.AXIS);

            switch (axis)
            {
                case Axis.X: BlockMeshBuilder.BuildFromCachedModel(cachedModelX, pos, nb, mesh, nc); break;
                case Axis.Y: BlockMeshBuilder.BuildFromCachedModel(cachedModelY, pos, nb, mesh, nc); break;
                default: BlockMeshBuilder.BuildFromCachedModel(cachedModelZ, pos, nb, mesh, nc); break;
            }
        }

        //all block log models share the same face culling
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return cachedModelY.FaceCull[(byte)faceSide];
        }
    }
}
