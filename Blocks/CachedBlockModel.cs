using OpenTK.Mathematics;

namespace OurCraft.Blocks
{
    //takes a block model and converts it into raw vertex values for optmized speed when meshing
    public class CachedBlockModel
    {
        public bool IsTranslucent;
        public string Name = "";

        //face culling type for each direction (indexed 0–5)
        public FaceType[] FaceCull = []; //maps to enum { FullBlock = 0, Partial, Air, ... }

        //pre-baked cuboids
        public List<CachedCuboid> Cuboids = [];

        //converts a normal json block model to a faster block model used for meshing
        public static CachedBlockModel BakeBlockModel(BlockModel jsonModel)
        {
            //create new cached block model
            var cached = new CachedBlockModel();
            cached.IsTranslucent = jsonModel.IsTranslucent;
            cached.Name = jsonModel.Name;
            cached.FaceCull = new FaceType[6];

            //setup the face culling
            foreach (var kvp in jsonModel.FaceCull)
            {               
                CubeFaces faceIndex = BlockShape.FaceNameToCubeFace(kvp.Key);
                cached.FaceCull[(byte)faceIndex] = BlockShape.FaceTypeFromString(kvp.Value);     
            }
            //now map the cuboid elements properly
            cached.Cuboids = [];
            foreach (var c in jsonModel.Elements)
            {
                //map the bounding box
                CachedCuboid cuboid = new CachedCuboid()
                {
                    From = new Vector3(c.From[0] / 16f, c.From[1] / 16f, c.From[2] / 16f),
                    To = new Vector3(c.To[0] / 16f, c.To[1] / 16f, c.To[2] / 16f),                   
                };

                //create bounds
                Vector3 from = cuboid.From;
                Vector3 to = cuboid.To;

                //calculate final vertices
                //bottom
                cuboid.bv0p = new Vector3(from.X, from.Y, from.Z);
                cuboid.bv1p = new Vector3(to.X, from.Y, from.Z);
                cuboid.bv2p = new Vector3(to.X, from.Y, to.Z);
                cuboid.bv3p = new Vector3(from.X, from.Y, to.Z);

                //top
                cuboid.tv0p = new Vector3(from.X, to.Y, to.Z);
                cuboid.tv1p = new Vector3(to.X, to.Y, to.Z);
                cuboid.tv2p = new Vector3(to.X, to.Y, from.Z);
                cuboid.tv3p = new Vector3(from.X, to.Y, from.Z);

                //front
                cuboid.fv0p = new Vector3(from.X, from.Y, to.Z);
                cuboid.fv1p = new Vector3(to.X, from.Y, to.Z);
                cuboid.fv2p = new Vector3(to.X, to.Y, to.Z);
                cuboid.fv3p = new Vector3(from.X, to.Y, to.Z);

                //back
                cuboid.bcv0p = new Vector3(to.X, from.Y, from.Z);
                cuboid.bcv1p = new Vector3(from.X, from.Y, from.Z);
                cuboid.bcv2p = new Vector3(from.X, to.Y, from.Z);
                cuboid.bcv3p = new Vector3(to.X, to.Y, from.Z);

                //right
                cuboid.rv0p = new Vector3(to.X, from.Y, to.Z);
                cuboid.rv1p = new Vector3(to.X, from.Y, from.Z);
                cuboid.rv2p = new Vector3(to.X, to.Y, from.Z);
                cuboid.rv3p = new Vector3(to.X, to.Y, to.Z);

                //left
                cuboid.lv0p = new Vector3(from.X, from.Y, from.Z);
                cuboid.lv1p = new Vector3(from.X, from.Y, to.Z);
                cuboid.lv2p = new Vector3(from.X, to.Y, to.Z);
                cuboid.lv3p = new Vector3(from.X, to.Y, from.Z);


                //map the faces for the elements
                cuboid.Faces = new CachedFace[6];
                foreach (var facePair in c.Faces)
                {
                    CubeFaces faceIndex = BlockShape.FaceNameToCubeFace(facePair.Key);
                    var face = facePair.Value;
                    
                    //lookup texture ids and match the uvs properly
                    CachedFace cachedFace = new CachedFace
                    {
                        TextureID = BlockShape.GetTextureID(face.Texture),                      
                        UV = new Vector4(face.UV[0] / 16f, face.UV[1] / 16f, face.UV[2] / 16f, face.UV[3] / 16f),
                        Cullable = face.Cullable
                    };

                    float u0 = BlockMeshHelper.GetTextureX(cachedFace.TextureID) + cachedFace.UV.X * BlockMeshHelper.NormalizedBlockTextureX();
                    float v0 = BlockMeshHelper.GetTextureY(cachedFace.TextureID) + cachedFace.UV.Y * BlockMeshHelper.NormalizedBlockTextureY();
                    float u1 = BlockMeshHelper.GetTextureX(cachedFace.TextureID) + cachedFace.UV.Z * BlockMeshHelper.NormalizedBlockTextureX();
                    float v1 = BlockMeshHelper.GetTextureY(cachedFace.TextureID) + cachedFace.UV.W * BlockMeshHelper.NormalizedBlockTextureY();

                    cachedFace.UV = new Vector4(u0, v0, u1, v1);
                    cuboid.Faces[(byte)faceIndex] = cachedFace;
                }

                cached.Cuboids.Add(cuboid);
            }
            return cached;
        }        
    }

    //contains raw vertex positions, with a collection of texture faces
    public class CachedCuboid
    {
        public Vector3 From; //0–1 range (converted from 0–16)
        public Vector3 To;

        //the vertices of this cuboid
        public Vector3 bv0p;
        public Vector3 bv1p;
        public Vector3 bv2p;
        public Vector3 bv3p;

        public Vector3 tv0p;
        public Vector3 tv1p;
        public Vector3 tv2p;
        public Vector3 tv3p;

        public Vector3 fv0p;
        public Vector3 fv1p;
        public Vector3 fv2p;
        public Vector3 fv3p;

        public Vector3 bcv0p;
        public Vector3 bcv1p;
        public Vector3 bcv2p;
        public Vector3 bcv3p;

        public Vector3 rv0p;
        public Vector3 rv1p;
        public Vector3 rv2p;
        public Vector3 rv3p;

        public Vector3 lv0p;
        public Vector3 lv1p;
        public Vector3 lv2p;
        public Vector3 lv3p;

        public CachedFace[] Faces = []; //index by CubeFaces enum
    }

    //contains the texture data and if the face is cullable or not
    public class CachedFace
    {
        public int TextureID;    //integer lookup into texture atlas
        public Vector4 UV;       //normalized 0–1 UVs
        public bool Cullable;    //whether to test neighbor for culling
    }

}