using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OurCraft.Blocks;
using OurCraft.Blocks.Block_Properties;
using OurCraft.Terrain_Generation;
using OurCraft.utility;
using OurCraft.World;
using static OurCraft.Physics.VoxelPhysics;

namespace OurCraft.Entities.Components
{
    //allows for player to interact with the world
    public class PlayerInteractions : Component
    {
        int currentBlockID = 0;
        readonly float reach = 4.0f;

        internal override void Register()
        {
            BaseSystem<PlayerInteractions>.Register(this);
        }

        internal override void Unregister()
        {
            BaseSystem<PlayerInteractions>.Unregister(this);
        }

        public override void OnCreation()
        {
            currentBlockID = BlockRegistry.GetBlockID("Grass Block");
        }

        public override void OnUpdate(ChunkManager world, double time, KeyboardState kb, MouseState ms)
        {
            ScrollBlocks(ms);
            HandleBlockInteractions(world, ms);
            DebugInteractions(world, kb);
        }

        void HandleBlockInteractions(ChunkManager world, MouseState ms)
        {
            bool hitBlock = RaycastVoxel(Transform.position, Transform.Forward, reach, (x, y, z) => world.GetBlockState(new Vector3(x, y, z)) != Block.AIR, out VoxelRaycastHit hit);

            //gameplay
            if (ms.IsButtonPressed(MouseButton.Left))
            {
                if (hitBlock)
                {
                    world.SetBlock(hit.blockPos, Block.AIR);
                }
            }
            if (ms.IsButtonPressed(MouseButton.Right))
            {
                if (hitBlock)
                {
                    //get blocks
                    BlockState bottom, top, front, back, left, right;

                    bottom = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, -1, 0));
                    top = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 1, 0));
                    front = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 0, 1));
                    back = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(0, 0, -1));
                    right = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(1, 0, 0));
                    left = world.GetBlockState(hit.blockPos + hit.faceNormal + new Vector3(-1, 0, 0));

                    //try to add block
                    BlockData.GetBlock(currentBlockID).PlaceBlockState(hit.blockPos, hit.faceNormal,
                    bottom, top, front, back, right, left,
                    world.GetBlockState(hit.blockPos), world);
                }
            }
            //debug block and light value
            if (ms.IsButtonPressed(MouseButton.Middle))
            {
                if (hitBlock)
                {
                    Console.Clear();
                    Vector3i hitBlockPos = hit.blockPos + hit.faceNormal;
                    ushort light = world.GetLight(hitBlockPos);
                    Vector3i blockLight = VoxelMath.UnpackLight16Block(light);
                    byte skyLight = VoxelMath.UnpackLight16Sky(light);
                    Console.WriteLine("Sampling light at pos: (" + hitBlockPos.X + ", " + hitBlockPos.Y + ", " + hitBlockPos.Z + ")");
                    Console.WriteLine("Light R: " + blockLight.X + " Light G: " + blockLight.Y + " Light B: " + blockLight.Z);
                    Console.WriteLine("SkyLight: " + skyLight);
                    Console.WriteLine("\nDebugging BlockState at: (" + hit.blockPos.X + ", " + hit.blockPos.Y + ", " + hit.blockPos.Z + ")");
                    BlockState state = world.GetBlockState(hit.blockPos);
                    state.DebugState();
                }
            }
        }

        void ScrollBlocks(MouseState ms)
        {            
            if (ms.ScrollDelta.Y < 0) TryCurrentBlockDecrease();
            if (ms.ScrollDelta.Y > 0) TryCurrentBlockIncrease();
        }

        void TryCurrentBlockIncrease()
        {
            if (currentBlockID < BlockData.MAXBLOCKID) currentBlockID++;
            Console.Clear();
            Console.WriteLine("currentBlock: " + BlockData.GetBlock(currentBlockID).GetBlockName());
        }

        void TryCurrentBlockDecrease()
        {
            if (currentBlockID > 1) currentBlockID--;
            Console.Clear();
            Console.WriteLine("currentBlock: " + BlockData.GetBlock(currentBlockID).GetBlockName());
        }

        void DebugInteractions(ChunkManager world, KeyboardState ks)
        {
            //debugging
            if (ks.IsKeyPressed(Keys.G))
            {
                Console.Clear();
                ChunkCoord coord = world.GetPlayerChunk();
                Chunk? chunk = world.GetChunk(coord);
                if (chunk != null)
                {
                    Console.WriteLine(chunk.GetState());
                }
                world.Debug();
            }

            if (ks.IsKeyPressed(Keys.R))
            {
                Console.Clear();
                NoiseRouter.DebugPrint((int)Transform.position.X, (int)Transform.position.Z);
            }
        }
    }
}
