namespace OurCraft.Blocks.Block_Info
{
    public class FullBlockJson
    {
        public string Name { get; set; } = "";
        public string ModelPath { get; set; } = "";
    }

    public class CrossBlockJson
    {
        public string Name { get; set; } = "";
        public string TextureName { get; set; } = "";
    }

    public class FullLightBlockJson
    {
        public string Name { get; set; } = "";
        public string ModelPath { get; set; } = "";
        public int LightR { get; set; } = 0;
        public int LightG { get; set; } = 0;
        public int LightB { get; set; } = 0;
    }

    public class LogBlockJson
    {
        public string Name { get; set; } = "";
        public string ModelX { get; set; } = "";
        public string ModelY { get; set; } = "";
        public string ModelZ { get; set; } = "";
    }

    public class SlabBlockJson
    {
        public string Name { get; set; } = "";
        public string ModelBottom { get; set; } = "";
        public string ModelTop { get; set; } = "";
        public string ModelDouble { get; set; } = "";
    }
}