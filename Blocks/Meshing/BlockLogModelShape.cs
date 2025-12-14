
using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.Graphics.Voxel_Lighting;

namespace OurCraft.Blocks.Meshing
{
    public class BlockLogModelShape : BlockShape
    {
        public BlockLogModelShape()
        {
            IsFullOpaqueBlock = false;
        }

        public CachedBlockModel cachedModelX = new();
        public CachedBlockModel cachedModelY = new();
        public CachedBlockModel cachedModelZ = new();

        //add log type mesh
        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState thisState, LightingData lightData, ushort thisLight)
        {
            Axis axis = BlockLog.AXIS.Decode(thisState.MetaData);

            switch (axis)
            {
                case Axis.X:
                    BlockModelMeshBuilder.BuildFromCachedModel(cachedModelX, pos, bottom, top, front, back, right, left, thisState, mesh, lightData);
                    break;
                case Axis.Y:
                    BlockModelMeshBuilder.BuildFromCachedModel(cachedModelY, pos, bottom, top, front, back, right, left, thisState, mesh, lightData);
                    break;
                default:
                    BlockModelMeshBuilder.BuildFromCachedModel(cachedModelZ, pos, bottom, top, front, back, right, left, thisState, mesh, lightData);
                    break;
            }
        }

        //all block log models share the same face culling
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return cachedModelY.FaceCull[(byte)faceSide];
        }
    }
}
