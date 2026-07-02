using OurCraft.Graphics;
using OpenTK.Mathematics;
using OurCraft.World;

namespace OurCraft.Blocks.Meshing.ModelShapes
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

        //add block shape
        public override void AddBlockMesh(Vector3 pos, NeighborBlocks nb, ChunkMeshData mesh, ChunkSectionNeighbors nc)
        {
            BlockMeshBuilder.BuildXGrass(pos, (ushort)Tex, mesh, nc);
        }
    }
}
