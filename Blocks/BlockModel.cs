using System.Text.Json;
using OpenTK.Mathematics;

namespace OurCraft.Blocks
{
    //represents block model json format to c#
    public class BlockModel
    {
        //not important
        public string Name { get; set; } = "";

        //determines which render pass it goes to
        public bool IsTranslucent { get; set; } = false;

        //usually blocks will use ambient occlusion
        public bool AOSupport { get; set; } = true;

        //which axis matches to which face culling type
        public Dictionary<string, string> FaceCull { get; set; } = [];

        //cuboids
        public List<Element> Elements { get; set; } = [];

        //refrences some cuboid shape of a block model
        public class Element
        {
            //start pos
            public float[] From { get; set; } = [];

            //end pos
            public float[] To { get; set; } = [];

            //face types, cullable or not, textures
            public Dictionary<string, Face> Faces { get; set; } = [];
        }

        //represents a face of a cuboid for a block model
        public class Face
        {
            //which texture id
            public string Texture { get; set; } = "";

            //uv mapping
            public float[] UV { get; set; } = [];

            //can this face be culled or not
            public bool Cullable { get; set; } = false;

            //which direction is this face culled on
            public string CullAxis { get; set; } = "";

            //should use ao? normally true
            public bool AO { get; set; } = true;
        }

        //actually load the thing
        public static BlockModel Load(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Resources/BlockModels/{fileName}";
            string json = File.ReadAllText(path);

            //allow case-insensitive JSON property matching
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }; 
            var result = JsonSerializer.Deserialize<BlockModel>(json, options);

            //just in case thing doesnt exist return empty model, will break meshing though
            if (result == null)
            {
                Console.WriteLine("Block Model does not exist in file directory: " + path);
                return new BlockModel();
            }

            return result;
        }
    }
}