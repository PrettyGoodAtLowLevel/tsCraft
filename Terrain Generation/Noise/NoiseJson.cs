using FastNoiseLiteRef;
using OurCraft.Utility;
using System.Text.Json;

namespace OurCraft.Terrain_Generation.Noise
{
    //noise file represented in json
    public class NoiseJson
    {
        private static readonly string noiseFilePath = FileConstants.WORLD_GEN_DATA_PATH + "Noises/";

        public string NoiseType { get; set; } = "";
        public string FractalType { get; set; } = "";
        public string DomainWarpType { get; set; } = "";
        public float Frequency { get; set; } = 0;
        public int Octaves { get; set; } = 0;
        public float Warp { get; set; } = 0;

        //constructs a noise json class from a json file configuration
        public static NoiseJson Load(string fileName, bool debug = false)
        {
            string path = noiseFilePath + fileName;
            string json = File.ReadAllText(path);

            //allow case-insensitive JSON property matching
            var options = new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true };

            var result = JsonSerializer.Deserialize<NoiseJson>(json, options);

            if (debug)
            {
                if (result == null) Console.WriteLine("Deserialization failed!");
                else Console.WriteLine("Loaded noise config successfully!");
            }

            //if thing is null return default noise json fast noise lite
            if (result == null)
            {
                NoiseJson temp = new()
                {
                    NoiseType = "OpenSimplex2",
                    FractalType = "FBm",
                    DomainWarpType = "OpenSimplex2",
                    Frequency = 0.01f,
                    Octaves = 0,
                    Warp = 0
                };
                return temp;
            }

            return result;
        }

        //grab json file and cast it to regular fast noise lite
        public static FastNoiseLite JsonToFastNoise(string fileName, int seed)
        {
            NoiseJson json = Load(fileName);
            FastNoiseLite noise = new();

            noise.SetSeed(seed);
            noise.SetNoiseType(NoiseTypeFromString(json.NoiseType));
            noise.SetFractalType(FractalTypeFromString(json.FractalType));
            noise.SetDomainWarpType(DomainWarpTypeFromString(json.DomainWarpType));

            noise.SetFrequency(json.Frequency);
            noise.SetFractalOctaves(json.Octaves);
            noise.SetDomainWarpAmp(json.Warp);

            return noise;
        }

        //get the current fractal type from json string value
        static FastNoiseLite.FractalType FractalTypeFromString(string name)
        {
            return (FastNoiseLite.FractalType)Enum.Parse(typeof(FastNoiseLite.FractalType), name);
        }

        //same thing but for the regular noise type
        static FastNoiseLite.NoiseType NoiseTypeFromString(string name)
        {
            return (FastNoiseLite.NoiseType)Enum.Parse(typeof(FastNoiseLite.NoiseType), name);
        }

        //same thing but for domain warp type
        static FastNoiseLite.DomainWarpType DomainWarpTypeFromString(string name)
        {
            return (FastNoiseLite.DomainWarpType)Enum.Parse(typeof(FastNoiseLite.DomainWarpType), name);
        }
    }
}