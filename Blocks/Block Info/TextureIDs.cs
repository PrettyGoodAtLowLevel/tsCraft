namespace OurCraft.Blocks
{
    //contains all the texture ids for all block faces
    public static class TextureIDs
    {
        //---------terrain blocks--------------

        //grass
        public static readonly int grassTopTex = 2;
        public static readonly int snowGrassSideTex = 1;
        public static readonly int dirtTex = 50;
        public static readonly int gravelTex = 49;
        public static readonly int grassSideTex = 3;
        public static readonly int snowTex = 172;
        public static readonly int iceTex = 173;
        public static readonly int sandTex = 37;
        public static readonly int cactusTopTex = 175;
        public static readonly int cactusSideTex = 176;
        public static readonly int cactusBottomTex = 177;

        //stones
        public static readonly int stoneTex = 19;
        public static readonly int cobbleStoneTex = 26;

        //water        
        public static readonly int waterTex = 387;

        //------tree blocks------------

        //oak
        public static readonly int oakLogSideTex = 99;
        public static readonly int oakLogTopTex = 100;      
        public static readonly int oakLeavesTex = 157;
        public static readonly int oakPlanksTex = 53;

        //spruce
        public static readonly int spruceLogSideTex = 101;
        public static readonly int spruceLogTopTex = 102;
        public static readonly int spruceLeavesTex = 158;
        public static readonly int sprucePlanksTex = 54;

        //birch
        public static readonly int birchLogSideTex = 103;
        public static readonly int birchLogTopTex = 104;
        public static readonly int birchLeavesTex = 159;
        public static readonly int birchPlanksTex = 55;

        //jungle
        public static readonly int jungleLogSideTex = 105;
        public static readonly int jungleLogTopTex = 106;
        public static readonly int jungleLeavesTex = 160;
        public static readonly int junglePlanksTex = 56;

        //x shaped blocks
        public static readonly int roseTex = 68;
        public static readonly int xGrassTex = 138;
        public static readonly int deadBushTex = 136;

        //--------building blocks-----------
        public static readonly int glassTex = 152;
        public static readonly int redstoneBlockTex = 117;
        public static readonly int emeraldBlockTex = 116;
        public static readonly int lapizBlockTex = 115;
        public static readonly int whiteGlassTex = 367;
        public static readonly int purpleGlassTex = 368;
    }

    public static class TextureRegistry
    {
        private static readonly Dictionary<string, int> textureMap = new(StringComparer.OrdinalIgnoreCase)
        {
            //-------- Terrain Blocks --------
            { "Grass_Top", TextureIDs.grassTopTex },
            { "Snow_Grass_Side", TextureIDs.snowGrassSideTex },
            { "Dirt", TextureIDs.dirtTex },
            { "Gravel", TextureIDs.gravelTex },
            { "Grass_Side", TextureIDs.grassSideTex },
            { "Snow", TextureIDs.snowTex },
            { "Ice", TextureIDs.iceTex },
            { "Sand", TextureIDs.sandTex },
            { "Cactus_Top", TextureIDs.cactusTopTex },
            { "Cactus_Side", TextureIDs.cactusSideTex },
            { "Cactus_Bottom", TextureIDs.cactusBottomTex },

            //-------- Stone Blocks --------
            { "Stone", TextureIDs.stoneTex },
            { "Cobblestone", TextureIDs.cobbleStoneTex },

            // -------- Water --------
            { "Water", TextureIDs.waterTex },

            //-------- Oak Tree --------
            { "Oak_Log_Side", TextureIDs.oakLogSideTex },
            { "Oak_Log_Top", TextureIDs.oakLogTopTex },
            { "Oak_Leaves", TextureIDs.oakLeavesTex },
            { "Oak_Planks", TextureIDs.oakPlanksTex },

            //-------- Spruce Tree --------
            { "Spruce_Log_Side", TextureIDs.spruceLogSideTex },
            { "Spruce_Log_Top", TextureIDs.spruceLogTopTex },
            { "Spruce_Leaves", TextureIDs.spruceLeavesTex },
            { "Spruce_Planks", TextureIDs.sprucePlanksTex },

            //-------- Birch Tree --------
            { "Birch_Log_Side", TextureIDs.birchLogSideTex },
            { "Birch_Log_Top", TextureIDs.birchLogTopTex },
            { "Birch_Leaves", TextureIDs.birchLeavesTex },
            { "Birch_Planks", TextureIDs.birchPlanksTex },

            //-------- Jungle Tree --------
            { "Jungle_Log_Side", TextureIDs.jungleLogSideTex },
            { "Jungle_Log_Top", TextureIDs.jungleLogTopTex },
            { "Jungle_Leaves", TextureIDs.jungleLeavesTex },
            { "Jungle_Planks", TextureIDs.junglePlanksTex },

            //-------- X-Shaped Plants --------
            { "Rose", TextureIDs.roseTex },
            { "Grass_X", TextureIDs.xGrassTex },
            { "Dead_Bush", TextureIDs.deadBushTex },

            //-------- Building Blocks --------
            { "Glass", TextureIDs.glassTex },
            { "White_Glass", TextureIDs.whiteGlassTex },
            { "Purple_Glass", TextureIDs.purpleGlassTex },
            { "Redstone_Block", TextureIDs.redstoneBlockTex},
            { "Emerald_Block", TextureIDs.emeraldBlockTex},
            { "Lapiz_Block", TextureIDs.lapizBlockTex},
        };

        //returns the integer texture ID for a given name.
        //throws if not found.
        public static int GetTextureID(string name)
        {
            if (!textureMap.TryGetValue(name, out int id))
                throw new KeyNotFoundException($"Texture name not found in registry: '{name}'");
            return id;
        }

        //returns true if the texture name exists.
        public static bool HasTexture(string name) => textureMap.ContainsKey(name);
    }

}