namespace OurCraft.Physics
{
    //contains aabb refrences
    public sealed class CollisionShape
    {
        public readonly bool complexCollision;
        public readonly AABB[] aabbs;

        public CollisionShape(AABB[] boxes)
        { 
            this.aabbs = boxes;
            this.complexCollision = aabbs.Length > 1;
        }
    }
}
