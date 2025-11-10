using OpenTK.Mathematics;

namespace OurCraft.Blocks
{
    public class CachedBlockModel
    {
        public bool IsTranslucent;
        public string Name = "";

        //face culling type for each direction (indexed 0–5)
        public FaceType[] FaceCull = []; // maps to enum { FullBlock = 0, Partial, Air, ... }

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
            cached.Cuboids = new List<CachedCuboid>();
            foreach (var c in jsonModel.Elements)
            {
                //map the bounding box
                CachedCuboid cuboid = new CachedCuboid();
                cuboid.From = new Vector3(c.From[0] / 16f, c.From[1] / 16f, c.From[2] / 16f);
                cuboid.To = new Vector3(c.To[0] / 16f, c.To[1] / 16f, c.To[2] / 16f);

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

                    cuboid.Faces[(byte)faceIndex] = cachedFace;
                }

                cached.Cuboids.Add(cuboid);
            }
            return cached;
        }        
    }


    public class CachedCuboid
    {
        public Vector3 From; //0–1 range (converted from 0–16)
        public Vector3 To;
        public CachedFace[] Faces = []; //index by CubeFaces enum
    }

    public class CachedFace
    {
        public int TextureID;    //integer lookup into texture atlas
        public Vector4 UV;       //normalized 0–1 UVs
        public bool Cullable;    //whether to test neighbor for culling
    }

}

