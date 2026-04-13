using System.Text.Json;

namespace OurCraft.Blocks
{
    //textures are represented in grid numbers, 0 corresponds to the frist texture in the 32 by 32 grid,
    //32 corresponds to the first texture of the second row
    public static class TextureRegistry
    {
        //all the texture names mapped out to their texture atlas grid id
        private static readonly Dictionary<string, int> textureMap = new(StringComparer.OrdinalIgnoreCase){};

        //loads all textures
        public static void InitTextures()
        {
            AddTexture("Natural/GrassTopTex.json");
            AddTexture("Natural/SnowGrassSideTex.json");
            AddTexture("Natural/DirtTex.json");
            AddTexture("Natural/GravelTex.json");
            AddTexture("Natural/GrassSideTex.json");
            AddTexture("Natural/SnowTex.json");
            AddTexture("Natural/IceTex.json");
            AddTexture("Natural/SandTex.json");

            AddTexture("Natural/CactusTopTex.json");
            AddTexture("Natural/CactusSideTex.json");
            AddTexture("Natural/CactusBottomTex.json");

            AddTexture("Natural/StoneTex.json");
            AddTexture("Building/CobbleStoneTex.json");
            AddTexture("Natural/WaterTex.json");

            AddTexture("Logs/OakLogSideTex.json");
            AddTexture("Logs/OakLogTopTex.json");
            AddTexture("Leaves/OakLeavesTex.json");
            AddTexture("Planks/OakPlanksTex.json");

            AddTexture("Logs/SpruceLogSideTex.json");
            AddTexture("Logs/SpruceLogTopTex.json");
            AddTexture("Leaves/SpruceLeavesTex.json");
            AddTexture("Planks/SprucePlanksTex.json");

            AddTexture("Logs/BirchLogSideTex.json");
            AddTexture("Logs/BirchLogTopTex.json");
            AddTexture("Leaves/BirchLeavesTex.json");
            AddTexture("Planks/BirchPlanksTex.json");

            AddTexture("Logs/JungleLogSideTex.json");
            AddTexture("Logs/JungleLogTopTex.json");
            AddTexture("Leaves/JungleLeavesTex.json");
            AddTexture("Planks/JunglePlanksTex.json");

            AddTexture("Natural/RoseTex.json");
            AddTexture("Natural/GrassXTex.json");
            AddTexture("Natural/DeadBushTex.json");
            AddTexture("Building/GlassTex.json");

            AddTexture("Building/RedstoneBlockTex.json");
            AddTexture("Building/EmeraldBlockTex.json");
            AddTexture("Building/LapizBlockTex.json");
        }

        public static bool HasTexture(string name) => textureMap.ContainsKey(name);

        public static int GetTextureID(string name)
        {
            if (!textureMap.TryGetValue(name, out int id)) throw new KeyNotFoundException($"Texture name not found in registry: '{name}'");
            return id;
        }

        public static void AddTexture(string fileName)
        {
            TextureJson json = TextureJson.LoadTexture(fileName);
            textureMap.Add(json.Handle, json.ID);
        }
    }

    //represents json texture ids in c#
    public class TextureJson
    {
        public string Handle { get; set; } = "";
        public int ID { get; set; } = 0;

        public static TextureJson LoadTexture(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Resources/Textures/IDs/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<TextureJson>(json, options);

            if (result == null)
            {
                Console.WriteLine($"Texture '{fileName}' not found!");
                return new TextureJson { Handle = "Empty", ID = 0 };
            }
            return result;
        }
    }
}