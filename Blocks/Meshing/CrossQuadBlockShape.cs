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

        public override void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, LightingData aOData)
        {
            BlockMeshBuilder.BuildXShapeBlock(pos, Tex, mesh, aOData.thisLight);
        }
    }
}
