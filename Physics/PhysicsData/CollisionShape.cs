namespace OurCraft.Physics.PhysicsData
{
    //contains aabb refrences
    public sealed class CollisionShape
    {
        public readonly bool complexCollision;
        public readonly AABB[] aabbs;

        public CollisionShape(AABB[] boxes)
        {
            aabbs = boxes;
            complexCollision = aabbs.Length > 1;
        }
    }
}
