using OpenTK.Mathematics;
using OurCraft.Blocks.Block_Implementations;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Blocks.BlockTypes;
using OurCraft.Blocks.Meshing;
using OurCraft.Blocks.Meshing.ModelShapes;
using OurCraft.Utility;
using System.Security.Cryptography;
using System.Text.Json;

namespace OurCraft.Blocks.Block_Info
{
    //provides helpers for loading in blocks, most code is extremely similar so no documentation needed
    public static class BlockLoader
    {
        private readonly static string blockFilePath = FileConstants.BLOCK_DATA_PATH;

        //what the air block in our game will be registered as
        public static AirBlock RegisterAirBlock(string name)
        {
            EmptyBlockShape model = new EmptyBlockShape() { IsFullOpaqueBlock = false, IsTranslucent = false, };

            ushort id = (ushort)BlockRegistry.BlockCount;
            AirBlock block = new AirBlock(name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        //most common block in game, full cube block with one state
        public static DefaultBlock RegisterDefaultBlock(string fileName)
        {
            string path = blockFilePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<DefaultBlockJson>(json, options);

            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            FullBlockModelShape model = new FullBlockModelShape()
            {
                IsTranslucent = result.IsTranslucent,
                IsFullOpaqueBlock = !result.IsTranslucent && result.IsOpaque,
                cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelPath))
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            DefaultBlock block = new DefaultBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            block.detectsCollision = result.DetectsCollision;
            block.physicsSolid = result.PhysicsSolid;
            block.isFluid = result.IsFluid;

            block.friction = result.Friction;
            block.bounce = result.Bounce;
            block.wallFriction = result.WallFriction;

            block.isLightSource = result.IsLightSource;
            block.blocksLight = result.IsLightOpaque;
            block.blockLightLevel = new Vector3i(result.LightR, result.LightG, result.LightB);
            if (result.IsLightOpaque) block.skyLightAttenuation = LightConstants.MAX_ATTENUATION;
            else block.skyLightAttenuation = result.SkyLightAttenuation;

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static ChestBlock RegisterChestBlock(string fileName)
        {
            string path = blockFilePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<DefaultBlockJson>(json, options);
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            FullBlockModelShape model = new FullBlockModelShape()
            {
                IsTranslucent = result.IsTranslucent,
                IsFullOpaqueBlock = !result.IsTranslucent && result.IsOpaque,
                cachedModel = CachedBlockModel.BakeBlockModel(BlockModel.Load(result.ModelPath))
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            ChestBlock block = new ChestBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            block.detectsCollision = result.DetectsCollision;
            block.physicsSolid = result.PhysicsSolid;
            block.isFluid = result.IsFluid;

            block.friction = result.Friction;
            block.bounce = result.Bounce;
            block.wallFriction = result.WallFriction;

            block.isLightSource = result.IsLightSource;
            block.blocksLight = result.IsLightOpaque;
            block.blockLightLevel = new Vector3i(result.LightR, result.LightG, result.LightB);
            if (result.IsLightOpaque) block.skyLightAttenuation = LightConstants.MAX_ATTENUATION;
            else block.skyLightAttenuation = result.SkyLightAttenuation;

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static CrossQuadBlock RegisterCrossBlock(string fileName)
        {
            string path = blockFilePath + fileName;
            string json = File.ReadAllText(path);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<CrossBlockJson>(json, options);

            //no invalid block
            if (result == null) throw new Exception("Block does not exist in file directory: " + path);

            CrossQuadBlockShape model = new CrossQuadBlockShape()
            {
                IsFullOpaqueBlock = false,
                IsTranslucent = false,
                Tex = BlockTextureManager.GetTextureIndex(result.TextureName)
            };

            ushort id = (ushort)BlockRegistry.BlockCount;
            CrossQuadBlock block = new CrossQuadBlock(result.Name, model);

            block.SetID(id);
            block.StateContainer = BlockStateExtensions.GenerateStates(block);

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static BlockLog RegisterLogBlock(string fileName)
        {
            string path = blockFilePath + fileName;
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

            block.detectsCollision = result.DetectsCollision;
            block.physicsSolid = result.PhysicsSolid;
            block.isFluid = result.IsFluid;

            block.friction = result.Friction;
            block.bounce = result.Bounce;
            block.wallFriction = result.WallFriction;

            block.isLightSource = result.IsLightSource;
            block.blocksLight = result.IsLightOpaque;
            block.blockLightLevel = new Vector3i(result.LightR, result.LightG, result.LightB);
            if (result.IsLightOpaque) block.skyLightAttenuation = LightConstants.MAX_ATTENUATION;
            else block.skyLightAttenuation = result.SkyLightAttenuation;

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }

        public static SlabBlock RegisterSlabBlock(string fileName)
        {
            string path = blockFilePath + fileName;
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

            block.friction = result.Friction;
            block.bounce = result.Bounce;
            block.wallFriction = result.WallFriction;

            BlockRegistry.AddBlockList(block);
            BlockRegistry.AddBlockRegistry(block.GetBlockName(), id);
            return block;
        }
    }
}
