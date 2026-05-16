//contains all constant and widely used values

using OpenTK.Graphics.ES20;

namespace OurCraft.Utility
{
    public static class RenderingConstants
    {
        public const int SCREEN_WIDTH = 1920;
        public const int SCREEN_HEIGHT = 1080;

        public const int DEFAULT_FOV = 90;
        public const float DEFAULT_NEAR_PLANE = 0.01f;
        public const float DEFAULT_FAR_PLANE = 1500.0f;

        public const int BLOCK_TEXTURE_WIDTH = 32;
        public const int BLOCK_TEXTURE_HEIGHT = 16;

        public const float CAM_HEIGHT_OFFSET = 0.65f;
    }

    public static class MathConstants
    {
        public const double PI = 3.141592;
        public const double TAU = 6.283185;
        public const double E = 2.718281;

        public const double RAD2DEG = 180/PI;
        public const double DEG2RAD = PI/180;        
    }

    public static class WorldConstants
    {
        public const int CHUNK_HEIGHT_IN_SUBCHUNKS = 24;
        public const int CHUNK_WIDTH_IN_SUBCHUNKS = 2;
        public const int SUBCHUNK_SIZE = 16;

        public const int CHUNK_HEIGHT = SUBCHUNK_SIZE * CHUNK_HEIGHT_IN_SUBCHUNKS;
        public const int CHUNK_WIDTH = SUBCHUNK_SIZE * CHUNK_WIDTH_IN_SUBCHUNKS;       
    }

    public static class PhysicsConstants
    {
        public const double DEFAULT_TIME_SCALE = 1.0;      
        public const double MAX_ACCUM = 0.08;

        public const double PHYSICS_TICK = 0.02;
        public const double BLOCK_TICK = 0.05;

        public const double MAX_VEL_Y = 100.0;
        public const double MAX_VEL_XZ = 100.0;

        public const float DEFAULT_FRICTION = 10.0f;
        public const float DEFAULT_BOUNCE = 0.0f;
        public const double GRAVITY = 9.8;
    }

    public static class LightConstants
    {
        public const int MAX_LIGHT = 15;
        public const int MIN_LIGHT = 0;

        public const int MAX_SKY = 15;
        public const int MIN_SKY = 0;

        public const int MAX_R = 15;
        public const int MIN_R = 0;

        public const int MAX_G = 15;
        public const int MIN_G = 0;

        public const int MAX_B = 15;
        public const int MIN_B = 0;

        public const int MAX_ATTENUATION = 15;
        public const int LOW_ATTENUATION = 1;
        public const int NO_ATTENUATION = 0;
    }

    public static class WorldGenConstants
    {
        public const int DEFAULT_SEA_LEVEL = 126;
        public const int DEFAULT_MIN_HEIGHT = 90;
        public const int DEFAULT_MAX_HEIGHT = 320;
    }

    public static class FileConstants
    {
        public const string DEFAULT_PATH = "C:/Users/alial/OneDrive/Desktop/OurCraft/";

        public const string SHADERS_PATH = DEFAULT_PATH + "Shaders/";
        public const string DEBUG_SHADERS_PATH = SHADERS_PATH + "DebugDrawing/";
        public const string ENTITY_SHADERS_PATH = SHADERS_PATH + "EntityDrawing/";
        public const string POST_PROCESSING_PATH = SHADERS_PATH + "Post Processing/";

        public const string RESOURCES_PATH = DEFAULT_PATH + "Resources/";
        public const string DATA_PATH = DEFAULT_PATH + "Data/";

        public const string BLOCK_DATA_PATH = DATA_PATH + "Blocks/";
        public const string WORLD_GEN_DATA_PATH = DATA_PATH + "WorldGen/";

        public const string BLOCK_MODEL_PATH = RESOURCES_PATH + "BlockModels/";
        public const string TEXTURES_PATH = RESOURCES_PATH + "Textures/";
    }

}