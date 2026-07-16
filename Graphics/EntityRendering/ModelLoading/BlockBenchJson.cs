using OurCraft.Utility;
using System.Text.Json;
using System.Text.Json.Serialization;

//contains json representations of the block bench bedrock entity format
namespace OurCraft.Graphics.EntityRendering.ModelLoading
{
    public class GeoRoot
    {
        private static readonly string geoFilePath = FileConstants.ASSETS_PATH;

        [JsonPropertyName("minecraft:geometry")]
        public GeoGeometry[]? MinecraftGeometry { get; set; }

        public static GeoRoot? LoadGeo(string fileName)
        {
            string json = File.ReadAllText(geoFilePath + fileName);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            GeoRoot? root = JsonSerializer.Deserialize<GeoRoot>(json, options);

            return root;
        }
    }

    public class GeoGeometry
    {
        public GeoDescription? description { get; set; }
        public GeoBone[]? bones { get; set; }
    }

    public class GeoDescription
    {
        public int texture_width { get; set; } = 0;
        public int texture_height { get; set; } = 0;
    }

    public class GeoBone
    {
        public string name { get; set; } = "";
        public string parent { get; set; } = "";
        public float[]? pivot { get; set; }
        public float[]? rotation { get; set; }
        public GeoCube[]? cubes { get; set; }
    }

    public class GeoCube
    {
        public float[]? origin { get; set; }
        public float[]? size { get; set; }
        public object? uv { get; set; }
        public float[]? pivot { get; set; }
        public float[]? rotation { get; set; }
        public float inflate { get; set; }
    }
}