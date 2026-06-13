using OpenTK.Mathematics;

namespace OurCraft.Physics
{
    //contains all local space collision shapes
    public static class CollisionShapeData
    {
        //empty shape, use for non collision detecting blocks
        public static readonly CollisionShape Empty = new CollisionShape
        (boxes: [new AABB(Vector3d.Zero, Vector3d.Zero)]);

        //default full block collision shape
        public static readonly CollisionShape FullBlock = new CollisionShape
        (boxes: [ new AABB(Vector3d.Zero, Vector3d.One) ]);

        //bottom slab
        public static readonly CollisionShape BottomHalfSlab = new CollisionShape
        (boxes: [ new AABB(Vector3d.Zero, new Vector3d(1, 0.5, 1)) ]);

        //top slab
        public static readonly CollisionShape TopHalfSlab = new CollisionShape
        (boxes: [ new AABB(new Vector3d(0, 0.5, 0), Vector3d.One) ]);
    }
}