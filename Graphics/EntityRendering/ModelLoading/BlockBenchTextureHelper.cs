using OpenTK.Mathematics;
using System.Text.Json;

namespace OurCraft.Graphics.EntityRendering.ModelLoading
{
    //contains helpers for importing per face uvs from block bench json models
    public static class BlockBenchTextureHelper
    {
        //builds all faces for a cuboid
        public static Vector2[,] BuildPerFaceUVs(JsonElement uv, float texW, float texH)
        {
            Vector2[,] faceUVs = new Vector2[6, 4];

            void Set(string key, int faceIndex)
            {
                if (!uv.TryGetProperty(key, out var face)) return;

                var uvArr = face.GetProperty("uv");
                var sizeArr = face.GetProperty("uv_size");

                float x = uvArr[0].GetSingle();
                float y = uvArr[1].GetSingle();
                float w = sizeArr[0].GetSingle();
                float h = sizeArr[1].GetSingle();

                var rect = MakeFace(x, y, w, h, texW, texH);
                //invert top and bottom textures
                if (key.Equals("down") || key.Equals("up")) rect = FlipVertical(FlipHorizontal(rect));
                for (int i = 0; i < 4; i++) faceUVs[faceIndex, i] = rect[i];
            }

            //match SetUpVertices order
            Set("down", 0);
            Set("up", 1);
            Set("south", 2);
            Set("north", 3);
            Set("east", 4);
            Set("west", 5);

            return faceUVs;
        }

        //creates a face based on openGL texture norms
        static Vector2[] MakeFace(float x, float y, float w, float h, float texW, float texH)
        {
            float u0 = x / texW;
            float v0 = 1f - (y / texH);

            float u1 = (x + w) / texW;
            float v1 = 1f - ((y + h) / texH);

            return
            [
                new Vector2(u0, v1),
                new Vector2(u1, v1),
                new Vector2(u1, v0),
                new Vector2(u0, v0),
            ];
        }

        //swaps uvs on x axis
        static Vector2[] FlipHorizontal(Vector2[] uv)
        {
            return
            [
                uv[1], //swap left/right
                uv[0],
                uv[3],
                uv[2]
            ];
        }

        //swaps uvs on the y axis
        static Vector2[] FlipVertical(Vector2[] uv)
        {
            return
            [
                uv[3],
                uv[2],
                uv[1],
                uv[0]
            ];
        }
    }
}
