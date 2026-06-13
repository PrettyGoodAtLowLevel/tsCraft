using OpenTK.Mathematics;
using OurCraft.Physics;
using System.Text.Json;

namespace OurCraft.Graphics.EntityRendering.ModelLoading
{
    //loads block bench models
    public static class BlockBenchModelLoader
    {
        //loads a 3d model in the blockbench bedrock entity json format
        public static EntityModel? Load(string modelPath, string texturePath)
        {
            GeoRoot? geoRoot = GeoRoot.LoadGeo(modelPath);
            if (geoRoot == null || geoRoot.MinecraftGeometry == null)
            {
                Console.WriteLine($"Model '{modelPath}' could not be found!");
                return null;
            }

            GeoGeometry geo = geoRoot.MinecraftGeometry[0];
            EntityModel model = new();

            if (geo.description == null) return null;
            if (geo.bones == null) return null;

            float texW = geo.description.texture_width;
            float texH = geo.description.texture_height;
            
            Dictionary<string, Transform> boneMap = [];
            Dictionary<string, Vector3d> pivotMap = [];

            //store absolute pivots first
            AssignPivots(geo.bones, pivotMap);

            //create transforms using parent-relative positions
            AssignBoneRotations(geo.bones, pivotMap, boneMap);

            //assign hierarchy
            Transform root = new();
            AssignBoneHeirarchy(geo.bones, model, root, boneMap);

            //build cube meshes
            CreateCubes(geo.bones, model, boneMap, pivotMap, texW, texH);

            //create texture 
            model.texture = new Texture();
            model.texture.Load(texturePath);

            model.root = root;
            return model;
        }

        //creates pivot maps
        private static void AssignPivots(GeoBone[] bones, Dictionary<string, Vector3d> pivotMap)
        {
            foreach (var bone in bones)
            {
                if (bone.pivot == null) continue;
                //negate x in order to match block bench conventions
                pivotMap[bone.name] = new Vector3d(-bone.pivot[0] / 16.0, bone.pivot[1] / 16.0, bone.pivot[2] / 16.0);
            }
        }

        //creates bone rotations and adds to bone map
        private static void AssignBoneRotations(GeoBone[] bones, Dictionary<string, Vector3d> pivotMap, Dictionary<string, Transform> boneMap)
        {
            foreach (var bone in bones)
            {
                Transform t = new();
                Vector3d pivot = pivotMap[bone.name];

                if (!string.IsNullOrEmpty(bone.parent) && pivotMap.TryGetValue(bone.parent, out Vector3d parentPivot)) t.localPosition = pivot - parentPivot;
                else t.localPosition = pivot;

                if (bone.rotation != null)
                {
                    float x = MathHelper.DegreesToRadians(bone.rotation[0]);
                    float y = MathHelper.DegreesToRadians(bone.rotation[1]);
                    float z = MathHelper.DegreesToRadians(bone.rotation[2]);

                    //block bench uses different coord system than openTK, so must mess with rotations
                    Quaternion qx = Quaternion.FromAxisAngle(-Vector3.UnitX, x);
                    Quaternion qy = Quaternion.FromAxisAngle(-Vector3.UnitY, y);
                    Quaternion qz = Quaternion.FromAxisAngle(Vector3.UnitZ, z);

                    //bedrock uses ZYX order
                    t.localRotation = qz * qy * qx;
                }

                boneMap[bone.name] = t;
            }
        }

        //creates heirarchy of bones through parenting
        private static void AssignBoneHeirarchy(GeoBone[] bones, EntityModel model, Transform root, Dictionary<string, Transform> boneMap)
        {
            foreach (var bone in bones)
            {
                Transform t = boneMap[bone.name];
                if (!string.IsNullOrEmpty(bone.parent)) t.parent = boneMap[bone.parent];
                else t.parent = root;

                model.bones.Add(t);
            }
        }

        //long ass method, creates all the cubes in the bone heirarchy of a bb model
        private static void CreateCubes(GeoBone[] bones, EntityModel model, Dictionary<string, Transform> boneMap, Dictionary<string, Vector3d> pivotMap, float texW, float texH)
        {
            foreach (var bone in bones)
            {
                Transform boneTransform = boneMap[bone.name];
                if (bone.cubes == null) continue;
                Vector3d pivot = pivotMap[bone.name];

                foreach (var cube in bone.cubes)
                {
                    if (cube.size == null || cube.origin == null) continue;
                    EntityMeshTransform mesh = new();
                    mesh.transform.parent = boneTransform;

                    //cube origin in world/model space
                    Vector3d bonePivot = pivotMap[bone.name];
                    Vector3d cubeOrigin = new(-cube.origin[0] / 16.0, cube.origin[1] / 16.0, cube.origin[2] / 16.0);

                    //default pivot = cube center if not specified
                    Vector3d cubePivot;
                    if (cube.pivot != null) cubePivot = new Vector3d(-cube.pivot[0] / 16.0, cube.pivot[1] / 16.0, cube.pivot[2] / 16.0);
                    else cubePivot = cubeOrigin + new Vector3d(-cube.size[0] / 32.0, cube.size[1] / 32.0, cube.size[2] / 32.0);

                    //pivot relative to bone
                    mesh.transform.localPosition = cubePivot - bonePivot;

                    //apply rotation around pivot
                    if (cube.rotation != null)
                    {
                        float x = MathHelper.DegreesToRadians(cube.rotation[0]);
                        float y = MathHelper.DegreesToRadians(cube.rotation[1]);
                        float z = MathHelper.DegreesToRadians(cube.rotation[2]);

                        Quaternion qx = Quaternion.FromAxisAngle(-Vector3.UnitX, x);
                        Quaternion qy = Quaternion.FromAxisAngle(-Vector3.UnitY, y);
                        Quaternion qz = Quaternion.FromAxisAngle(Vector3.UnitZ, z);

                        mesh.transform.localRotation = qz * qy * qx;
                    }

                    //create cube
                    Vector3 size = new Vector3(cube.size[0], cube.size[1], cube.size[2]) / 16f;
                    float inf = cube.inflate / 16f;
                    size += Vector3.One * inf * 2f;

                    mesh.min = -size / 2f;
                    mesh.max = size / 2f;

                    if (cube.uv is JsonElement elem && elem.ValueKind == JsonValueKind.Object)
                    {
                        Vector2[,] faceUVs = BlockBenchTextureHelper.BuildPerFaceUVs(elem, texW, texH);
                        mesh.CreateMesh(faceUVs);
                        model.meshes.Add(mesh);
                    }
                }
            }
        }
    }
}