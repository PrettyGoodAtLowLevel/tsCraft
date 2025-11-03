namespace OurCraft.Rendering
{
    //contains data of all the surrounding blocks for ambient occlusion
    public struct VoxelAOData
    {
        //top face
        public bool topBackLeft, topBackRight, topFrontLeft, topFrontRight;
        public bool topFront, topBack, topLeft, topRight;

        // Bottom face
        public bool bottomBackLeft, bottomBackRight, bottomFrontLeft, bottomFrontRight;
        public bool bottomFront, bottomBack, bottomLeft, bottomRight;

        //side corners (used when combining faces)
        public bool backLeft, backRight, frontLeft, frontRight;

        public VoxelAOData() { }
    }

    //contains data of the ao blocks for one face of ambient occlusion
    public struct VoxelAOFace
    {
        public bool front, back, left, right;
        public bool topLeft, topRight, bottomLeft, bottomRight;

        public VoxelAOFace() { }
    }

    //contains nice helper methods when adding block ambient occlusion
    public static class VoxelAOHelper
    {
        //returns a number to darken a vertex based on how many neighbors there are for a block
        public static byte GetAOByte(bool side1, bool side2, bool corner)
        {
            byte level = 0;
            if (side1) level += 75;
            if (side2) level += 75;
            if (corner) level += 75;
            return level;
        }

        //gets the top, bottom, front, back, right, and left face ambient occlusion from the full block data
        public static VoxelAOFace TopFaceFromCube(VoxelAOData d)
        {
            var f = new VoxelAOFace();
            f.left = d.topLeft;
            f.right = d.topRight;
            f.front = d.topFront;
            f.back = d.topBack;

            f.topLeft = d.topFrontLeft;
            f.topRight = d.topFrontRight;
            f.bottomLeft = d.topBackLeft;
            f.bottomRight = d.topBackRight;
            return f;
        }

        public static VoxelAOFace BottomFaceFromCube(VoxelAOData d)
        {
            var f = new VoxelAOFace();
            f.left = d.bottomLeft;
            f.right = d.bottomRight;
            f.front = d.bottomFront;
            f.back = d.bottomBack;

            f.topLeft = d.bottomFrontLeft;
            f.topRight = d.bottomFrontRight;
            f.bottomLeft = d.bottomBackLeft;
            f.bottomRight = d.bottomBackRight;
            return f;
        }

        public static VoxelAOFace FrontFaceFromCube(VoxelAOData d)
        {
            var f = new VoxelAOFace();
            f.left = d.frontLeft;
            f.right = d.frontRight;

            f.front = d.topFront;
            f.back = d.bottomFront;

            f.topLeft = d.topFrontLeft;
            f.topRight = d.topFrontRight;
            f.bottomLeft = d.bottomFrontLeft;
            f.bottomRight = d.bottomFrontRight;
            return f;
        }

        public static VoxelAOFace BackFaceFromCube(VoxelAOData d)
        {
            var f = new VoxelAOFace();
            f.left = d.backLeft;
            f.right = d.backRight;

            f.front = d.topBack;
            f.back = d.bottomBack;

            f.topLeft = d.topBackLeft;
            f.topRight = d.topBackRight;
            f.bottomLeft = d.bottomBackLeft;
            f.bottomRight = d.bottomBackRight;
            return f;
        }

        public static VoxelAOFace RightFaceFromCube(VoxelAOData d)
        {
            var f = new VoxelAOFace();
            f.left = d.frontRight;
            f.right = d.backRight;

            f.front = d.topRight;
            f.back = d.bottomRight;

            f.topLeft = d.topFrontRight;
            f.topRight = d.topBackRight;
            f.bottomLeft = d.bottomFrontRight;
            f.bottomRight = d.bottomBackRight;
            return f;
        }

        public static VoxelAOFace LeftFaceFromCube(VoxelAOData d)
        {
            var f = new VoxelAOFace();
            f.left = d.frontLeft;
            f.right = d.backLeft;

            f.front = d.topLeft;
            f.back = d.bottomLeft;

            f.topLeft = d.topFrontLeft;
            f.topRight = d.topBackLeft;
            f.bottomLeft = d.bottomFrontLeft;
            f.bottomRight = d.bottomBackLeft;
            return f;
        }

        //gets all of the vertex colors from a ambient occlusion face
        public static byte[] GetAoBytes(VoxelAOFace f)
        {
            byte bl = GetAOByte(f.back, f.left, f.bottomLeft);
            byte br = GetAOByte(f.back, f.right, f.bottomRight);
            byte tl = GetAOByte(f.front, f.left, f.topLeft);
            byte tr = GetAOByte(f.front, f.right, f.topRight);
            return new byte[] { bl, br, tl, tr };
        }
    }
}
