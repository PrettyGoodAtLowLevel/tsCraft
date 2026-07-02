namespace OurCraft.Blocks.Meshing.ModelShapes
{
    //void block shape, doesnt exist
    public class EmptyBlockShape : BlockShape
    {
        //get air side
        public override FaceType GetBlockFace(CubeFaces faceSide, BlockState state)
        {
            return FaceType.AIR;
        }
    }
}