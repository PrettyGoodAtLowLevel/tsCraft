using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.Meshing;
using System.Text.Json;

namespace OurCraft.Blocks.Block_Info
{
    //provides helpers for loading in blocks
    public static class BlockLoader
    {
        public static FullBlock RegisterFullBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<FullBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            FullBlockModelShape model = new FullBlockModelShape()
            {
                IsTranslucent = false, IsFullOpaqueBlock = true,
                cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelPath))
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            FullBlock block = new FullBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static WaterBlock RegisterWaterBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<FullBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            FullBlockModelShape model = new FullBlockModelShape()
            {
                IsTranslucent = true, IsFullOpaqueBlock = false,
                cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelPath))
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            WaterBlock block = new WaterBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static FullLightBlock RegisterFullLightBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<FullLightBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            FullBlockModelShape model = new FullBlockModelShape()
            {
                IsTranslucent = false, IsFullOpaqueBlock = true,
                cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelPath))
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            FullLightBlock block = new FullLightBlock(result.Name, model, new Vector3i(result.LightR, result.LightG, result.LightB));

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static CrossQuadBlock RegisterCrossBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<CrossBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            CrossQuadBlockShape model = new CrossQuadBlockShape()
            {
                IsFullOpaqueBlock = false, IsTranslucent = false,
                Tex = TextureRegistry.GetTextureID(result.TextureName)
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            CrossQuadBlock block = new CrossQuadBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static LeavesBlock RegisterLeavesBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<FullBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            FullBlockModelShape model = new FullBlockModelShape()
            {
                IsTranslucent = false, IsFullOpaqueBlock = true,
                cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelPath))
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            LeavesBlock block = new LeavesBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        } 

        public static GlassBlock RegisterGlassBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<FullBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            FullBlockModelShape model = new FullBlockModelShape()
            {
                IsTranslucent = false, IsFullOpaqueBlock = false,
                cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelPath))
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            GlassBlock block = new GlassBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static BlockLog RegisterLogBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<LogBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            BlockLogModelShape model = new BlockLogModelShape()
            {
                IsTranslucent = false, IsFullOpaqueBlock = true,
                cachedModelX = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelX)),
                cachedModelY = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelY)),
                cachedModelZ = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelZ)),
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            BlockLog block = new BlockLog(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static SlabBlock RegisterSlabBlock(string fileName)
        {
            string path = $"C:/Users/alial/OneDrive/Desktop/OurCraft/Data/Blocks/{fileName}";
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<SlabBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            SlabBlockModelShape model = new SlabBlockModelShape()
            {
                IsTranslucent = false, IsFullOpaqueBlock = false,
                cachedModelBottom = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelBottom)),
                cachedModelTop = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelTop)),
                cachedModelDouble = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelDouble)),
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            SlabBlock block = new SlabBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static AirBlock RegisterAirBlock(string name)
        {
            EmptyBlockShape model = new EmptyBlockShape()
            { IsFullOpaqueBlock = false, IsTranslucent = false, };

            ushort id = (ushort)BlockRegistry.BlockCount;
            AirBlock block = new AirBlock(name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);

            return block;
        }
    }
}
