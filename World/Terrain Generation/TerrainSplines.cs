using OurCraft.utility;

namespace OurCraft.World.Terrain_Generation
{
    //contains all the spline points for terrain generation
    //these splines allow us to manipulate values on a custom graph
    //these are useful since not every noise conributes to something linearly
    //things like rivers could only sample middle values of noise to create carves in the terrain
    //some noises could be mainly stagnant untill reaching the high values, which it skyrockets
    //you see the point
    public static class TerrainSplines
    {
        //determines land vs ocean
        public static readonly SplineGraph regionSpline = new
        ([
            new SplinePoint(-1.0f, 95.0f),   
            new SplinePoint(-0.4f, 95.0f),
            new SplinePoint(-0.235f, 120f),
            new SplinePoint(-0.15f, 130f),
            new SplinePoint(1.0f, 130f),
        ]);

        //determine hills, low zones, and highlands
        public static readonly SplineGraph erosionSpline = new
        ([
            new SplinePoint(-1.0f, 40),
            new SplinePoint(-0.75f, 35),
            new SplinePoint(-0.35f, 0f),
            new SplinePoint(0.15f, 0f),
            new SplinePoint(0.5f, 20),
            new SplinePoint(1.0f, 30),
        ]);

        //determine where rivers are placed in the world
        public static readonly SplineGraph riverSpline = new
        ([
            new SplinePoint(-1.0f, 0.0f),
            new SplinePoint(-0.15f, 0.0f),
            new SplinePoint(-0.05f, -5f),
            new SplinePoint(0.05f, -10f),
            new SplinePoint(0.2f, 0.0f),
            new SplinePoint(1.0f, 0.0f),
        ]);

        //determine how amplified the terrain gets
        public static readonly SplineGraph weirdnessSpline = new
        ([
            new SplinePoint(-1.0f, 50.0f),
            new SplinePoint(-0.75f, 50.0f),
            new SplinePoint(-0.5f, 6.5f),
            new SplinePoint(0.5f, 6.5f),
            new SplinePoint(0.75f, 50.0f),
            new SplinePoint(1.0f, 50.0f),
        ]);

        //extra amplifier for really weird fantasy terrain
        public static readonly SplineGraph fractureSpline = new
        ([
            new SplinePoint(-1.0f, 0.0f),
            new SplinePoint(-0.75f, 0.0f),
            new SplinePoint(0.65f, 0.0f),
            new SplinePoint(1.0f, 225.0f),
        ]);

        //makes sure rivers dont look too harsh ontop of mountains
        public static readonly SplineGraph riverFactorSpline = new
        ([
            new SplinePoint(-1.0f, 0.0f),
            new SplinePoint(-0.55f, 0.25f),
            new SplinePoint(-0.35f, 1.0f),
            new SplinePoint(0.15f, 1.0f),
            new SplinePoint(0.3f, 0.25f),
            new SplinePoint(1.0f, 0.0f),
        ]);

        //clamps noise values to temperature levels
        public static readonly SplineGraph temperatureSpline = new
        ([
            new SplinePoint(-1.0f, 0f),
            new SplinePoint(-0.6f, 0f),
            new SplinePoint(-0.6f, 1f),
            new SplinePoint(-0.2f, 1f),
            new SplinePoint(-0.2f, 2f),
            new SplinePoint(0.2f, 2f),
            new SplinePoint(0.2f, 3f),
            new SplinePoint(0.6f, 3f),
            new SplinePoint(0.6f, 4f),
            new SplinePoint(1.0f, 4f),
        ]);

        //clamps noise values to humidity levels
        public static readonly SplineGraph humiditySpline = new
        ([
            new SplinePoint(-1.0f, 0f),
            new SplinePoint(-0.6f, 0f),
            new SplinePoint(-0.6f, 1f),
            new SplinePoint(-0.2f, 1f),
            new SplinePoint(-0.2f, 2f),
            new SplinePoint(0.2f, 2f),
            new SplinePoint(0.2f, 3f),
            new SplinePoint(0.6f, 3f),
            new SplinePoint(0.6f, 4f),
            new SplinePoint(1.0f, 4f),
        ]);

        //clamps noise values to vegetation levels
        public static readonly SplineGraph vegetationSpline = new
        ([
            new SplinePoint(-1.0f, 0f),
            new SplinePoint(-0.33f, 0f),
            new SplinePoint(-0.33f, 1f),
            new SplinePoint(0.33f, 1f),
            new SplinePoint(0.33f, 2f),
            new SplinePoint(1.0f, 2f),
        ]);
    }
}