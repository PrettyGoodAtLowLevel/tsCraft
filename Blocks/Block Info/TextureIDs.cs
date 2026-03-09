namespace OurCraft.Blocks
{
    //textures are represented in grid numbers, 0 corresponds to the frist texture in the 32 by 32 grid,
    //32 corresponds to the first texture of the second row
    public static class TextureRegistry
    {
        //all the texture names mapped out to their texture atlas grid id
        private static readonly Dictionary<string, int> textureMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Grass_Top", 2 }, { "Snow_Grass_Side", 1 }, { "Dirt", 50 }, 
            { "Gravel", 49 },  { "Grass_Side", 3 },
            { "Snow", 172 }, { "Ice", 173 }, { "Sand", 37 }, 
            { "Cactus_Top", 175 }, { "Cactus_Side", 176 }, { "Cactus_Bottom", 177 }, 
            { "Stone", 19 }, { "Cobblestone", 26 }, { "Water", 387 }, 

            { "Oak_Log_Side", 99 },  { "Oak_Log_Top", 100 }, { "Oak_Leaves", 157 },  { "Oak_Planks", 53 }, 
            { "Spruce_Log_Side", 101 },  { "Spruce_Log_Top", 102 }, { "Spruce_Leaves", 158 }, { "Spruce_Planks", 54 }, 
            { "Birch_Log_Side", 103 }, { "Birch_Log_Top", 104 }, { "Birch_Leaves", 159 }, { "Birch_Planks", 55 }, 
            { "Jungle_Log_Side", 105 }, { "Jungle_Log_Top", 106 }, { "Jungle_Leaves", 160 }, { "Jungle_Planks", 56 }, 

            { "Rose", 68 }, { "Grass_X", 138 },{ "Dead_Bush", 136 }, 
            { "Glass", 152 }, 
            { "Redstone_Block", 117 }, { "Emerald_Block", 116 }, { "Lapiz_Block", 115 },
        };

        //returns the integer texture ID for a given name.
        public static int GetTextureID(string name)
        {
            if (!textureMap.TryGetValue(name, out int id)) throw new KeyNotFoundException($"Texture name not found in registry: '{name}'");
            return id;
        }

        //returns true if the texture name exists.
        public static bool HasTexture(string name) => textureMap.ContainsKey(name);
    }

}