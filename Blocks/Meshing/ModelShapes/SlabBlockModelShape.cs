using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.World;

namespace OurCraft.Blocks.Meshing.ModelShapes
{
    //interpets the slab type and adds the correct model based on that
    public class SlabBlockModelShape : BlockShape
    {       
        public CachedBlockModel cachedModelDouble = new();
        public CachedBlockModel cachedModelTop = new();
        public CachedBlockModel cachedModelBottom = new();

        public SlabBlockModelShape()
        {
            IsFullOpaqueBlock = false;
        }

        //add slab type mesh
        public override void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, ChunkSectionNeighbors nc)
        {
            SlabType type = nb.thisState.GetProperty(SlabBlock.SLAB_TYPE);

            switch (type)
            {
                case SlabType.Double: BlockMeshBuilder.BuildFromCachedModel(cachedModelDouble, pos, nb, mesh, nc); break;
                case SlabType.Top: BlockMeshBuilder.BuildFromCachedModel(cachedModelTop, pos, nb, mesh, nc); break;
                default: BlockMeshBuilder.BuildFromCachedModel(cachedModelBottom, pos, nb, mesh, nc); break;
            }
        }

        //find which slab type it is, then return the respective face
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            SlabType type = SlabBlock.SLAB_TYPE.Decode(state.MetaData);

            return type switch
            {
                SlabType.Double => cachedModelDouble.FaceCull[(byte)faceSide],
                SlabType.Top => cachedModelTop.FaceCull[(byte)faceSide],
                _ => cachedModelBottom.FaceCull[(byte)faceSide],
            };
        }
    }
}