using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.Graphics.Voxel_Lighting;

namespace OurCraft.Blocks.Meshing
{
    //interpets the slab type and adds the correct model based on that
    public class SlabBlockModelShape : BlockShape
    {
        public SlabBlockModelShape()
        {
            IsFullOpaqueBlock = false;
        }
        public CachedBlockModel cachedModelDouble = new();
        public CachedBlockModel cachedModelTop = new();
        public CachedBlockModel cachedModelBottom = new();

        //add slab type mesh
        public override void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, LightingData lightData)
        {
            SlabType type = nb.thisState.GetProperty(SlabBlock.SLAB_TYPE);

            switch (type)
            {
                case SlabType.Double:
                    BlockMeshBuilder.BuildFromCachedModel(cachedModelDouble, pos, nb, mesh, lightData); break;
                case SlabType.Top:
                    BlockMeshBuilder.BuildFromCachedModel(cachedModelTop, pos, nb, mesh, lightData); break;
                default:
                    BlockMeshBuilder.BuildFromCachedModel(cachedModelBottom, pos, nb, mesh, lightData); break;
            }
        }

        //find which slab type it is, then return the respective face
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            SlabType type = SlabBlock.SLAB_TYPE.Decode(state.MetaData);

            switch (type)
            {
                case SlabType.Double: return cachedModelDouble.FaceCull[(byte)faceSide];
                case SlabType.Top: return cachedModelTop.FaceCull[(byte)faceSide];
                default: return cachedModelBottom.FaceCull[(byte)faceSide];
            }        
        }
    }
}