using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Rendering;

namespace OurCraft.Blocks.Meshing
{
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
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, VoxelAOData aOData,
        ushort topLight, ushort bottomLight, ushort frontLight, ushort backLight, ushort rightLight, ushort leftLight)
        {
            SlabType type = SlabBlock.SLAB_TYPE.Decode(thisState.MetaData);

            switch (type)
            {
                case SlabType.Double:
                    BlockModelMeshBuilder.BuildFromCachedModel(cachedModelDouble, pos, bottom, top, front, back, right, left, thisState, mesh, aOData,
                    topLight, bottomLight, frontLight, backLight, rightLight, leftLight);
                    break;
                case SlabType.Top:
                    BlockModelMeshBuilder.BuildFromCachedModel(cachedModelTop, pos, bottom, top, front, back, right, left, thisState, mesh, aOData,
                    topLight, bottomLight, frontLight, backLight, rightLight, leftLight);
                    break;
                default:
                    BlockModelMeshBuilder.BuildFromCachedModel(cachedModelBottom, pos, bottom, top, front, back, right, left, thisState, mesh, aOData,
                    topLight, bottomLight, frontLight, backLight, rightLight, leftLight);
                    break;
            }
        }

        //find which slab type it is, then return the respective face
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            SlabType type = SlabBlock.SLAB_TYPE.Decode(state.MetaData);

            switch (type)
            {
                case SlabType.Double:
                    return cachedModelDouble.FaceCull[(byte)faceSide];
                case SlabType.Top:
                    return cachedModelTop.FaceCull[(byte)faceSide];
                default:
                    return cachedModelBottom.FaceCull[(byte)faceSide];
            }        
        }
    }
}