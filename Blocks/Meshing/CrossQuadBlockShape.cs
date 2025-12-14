using OurCraft.Blocks.Block_Properties;
using OurCraft.Graphics;
using OurCraft.Graphics.Voxel_Lighting;
using OpenTK.Mathematics;

namespace OurCraft.Blocks.Meshing
{
    //x shaped block, for flowers
    //doesnt require a model since they are harder to represent in json cuboids, but are really easy to make programatically
    public class CrossQuadBlockShape : BlockShape
    {
        public int Tex { get; set; } = 0;

        //we want all faces on a cross quad block to be visible
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.INDENTED;
        }

        public override void AddBlockMesh(Vector3 pos, BlockState bottom, BlockState top, BlockState front, BlockState back, BlockState right, BlockState left, ChunkMeshData mesh, BlockState state, LightingData aOData, ushort thisLight)
        {
            BlockModelMeshBuilder.BuildXShapeBlock(pos, Tex, mesh, thisLight);
        }
    }
}
