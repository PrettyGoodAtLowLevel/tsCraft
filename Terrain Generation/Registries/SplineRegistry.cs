using OurCraft.Utility;
using System.Text.Json;

namespace OurCraft.Terrain_Generation.Registries
{
    //contains all the spline data information for world gen
    public static class SplineRegistry
    {
        //determines land vs ocean
        public static readonly SplineGraph regionSpline = SplineJson.LoadSpline("RegionSpline.json");

        //determine hills, low zones, and highlands
        public static readonly SplineGraph erosionSpline = SplineJson.LoadSpline("ErosionSpline.json");

        //determine where rivers are placed in the world
        public static readonly SplineGraph riverSpline = SplineJson.LoadSpline("RiverSpline.json");

        //determine how amplified the terrain gets
        public static readonly SplineGraph weirdnessSpline = SplineJson.LoadSpline("WeirdnessSpline.json");

        //extra amplifier for really weird fantasy terrain
        public static readonly SplineGraph fractureSpline = SplineJson.LoadSpline("FractureSpline.json");

        //decides what amplification are given to caves based on 2d noise
        public static readonly SplineGraph caveSizeSpline = SplineJson.LoadSpline("CaveSizeSpline.json");

        //decides how big caves are based on y position, higher = smaller caves
        public static readonly SplineGraph caveOpenSpline = SplineJson.LoadSpline("CaveOpenSpline.json");

        //extra amplifier for high erosion areas
        public static readonly SplineGraph erosionAmplificationSpline = SplineJson.LoadSpline("ErosionAmplificationSpline.json");

        //makes sure rivers dont look too harsh ontop of mountains
        public static readonly SplineGraph riverFactorSpline = SplineJson.LoadSpline("RiverFactorSpline.json");

        //clamps noise values to temperature levels
        public static readonly SplineGraph temperatureSpline = SplineJson.LoadSpline("TemperatureSpline.json");

        //clamps noise values to humidity levels
        public static readonly SplineGraph humiditySpline = SplineJson.LoadSpline("HumiditySpline.json");

        //clamps noise values to vegetation levels
        public static readonly SplineGraph vegetationSpline = SplineJson.LoadSpline("VegetationSpline.json");
    }

    //json representation of spline graph
    public class SplineJson
    {
        private static readonly string splinesFilePath = FileConstants.WORLD_GEN_DATA_PATH + "Splines/";

        public List<SplinePointJson> Points { get; set; } = [];

        public static SplineGraph LoadSpline(string fileName)
        {
            //Load JSON
            string path = splinesFilePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            SplineJson config = JsonSerializer.Deserialize<SplineJson>(json, options)!;

            //convert to actual spline
            List<SplinePoint> points = [];
            foreach(var point in config.Points) points.Add(new SplinePoint(point.X, point.Y));
            
            SplineGraph spline = new SplineGraph(points);
            return spline;
        }
    }

    //represents json version of a 2d point on a spline graph
    public class SplinePointJson
    { 
        public float X { get; set; }
        public float Y { get; set; }
    }   
}