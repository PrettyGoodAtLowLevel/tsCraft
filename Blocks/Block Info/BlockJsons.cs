using OurCraft.Utility;

//all json representations of different block types in c#
namespace OurCraft.Blocks.Block_Info
{
    //regular cube like block
    public class DefaultBlockJson
    {
        public string Name { get; set; } = "";
        public string ModelPath { get; set; } = "";
        public bool IsOpaque { get; set; } = false;
        public bool IsTranslucent { get; set; } = false;

        public bool DetectsCollision { get; set; } = false;
        public bool PhysicsSolid { get; set; } = false;
        public bool IsFluid { get; set; } = false;
        public float Friction { get; set; } = PhysicsConstants.DEFAULT_FRICTION;
        public float Bounce { get; set; } = PhysicsConstants.DEFAULT_BOUNCE;
        public float WallFriction { get; set; } = PhysicsConstants.DEFAULT_FRICTION;

        public int LightR { get; set; } = 0;
        public int LightG { get; set; } = 0;
        public int LightB { get; set; } = 0;
        public int SkyLightAttenuation { get; set; } = 0;

        public bool IsLightSource { get; set; } = false;
        public bool IsLightOpaque { get; set; } = false;      
    }

    //same as default, but with 3 models and axis property
    public class LogBlockJson
    {
        public string Name { get; set; } = "";
        public string ModelX { get; set; } = "";
        public string ModelY { get; set; } = "";
        public string ModelZ { get; set; } = "";
        public bool IsOpaque { get; set; } = false;
        public bool IsTranslucent { get; set; } = false;

        public bool DetectsCollision { get; set; } = false;
        public bool PhysicsSolid { get; set; } = false;
        public bool IsFluid { get; set; } = false;
        public float Friction { get; set; } = PhysicsConstants.DEFAULT_FRICTION;
        public float Bounce { get; set; } = PhysicsConstants.DEFAULT_BOUNCE;
        public float WallFriction { get; set; } = PhysicsConstants.DEFAULT_FRICTION;

        public int LightR { get; set; } = 0;
        public int LightG { get; set; } = 0;
        public int LightB { get; set; } = 0;
        public int SkyLightAttenuation { get; set; } = 0;

        public bool IsLightSource { get; set; } = false;
        public bool IsLightOpaque { get; set; } = false;
    }

    public class SlabBlockJson
    {
        public string Name { get; set; } = "";
        public string ModelBottom { get; set; } = "";
        public string ModelTop { get; set; } = "";
        public string ModelDouble { get; set; } = "";
        public float Friction { get; set; } = PhysicsConstants.DEFAULT_FRICTION;
        public float WallFriction { get; set; } = 0;
        public float Bounce { get; set; } = PhysicsConstants.DEFAULT_BOUNCE;
    }

    public class CrossBlockJson
    {
        public string Name { get; set; } = "";
        public string TextureName { get; set; } = "";
    }

}