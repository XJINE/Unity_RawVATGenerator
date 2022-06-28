using UnityEngine;
using System.Collections.Generic;

namespace VATGenerator
{
    // NOTE:
    // Texture(x, y) = (Position, Time)
    // Origin (0, 0) is lower left.

    public class VertexAnimationTexture : System.IDisposable
    {
        #region Field

        public readonly string    Name;
        public readonly float     LengthSec;
        public readonly float     FrameCounts;
        public readonly Texture2D PosTex;
        public readonly Texture2D NmlTex;
        public readonly Bounds    Bounds;

        #endregion Field

        #region Constructor

        public VertexAnimationTexture(SkinnedMeshRenderer renderer,
                                      Animation           animation,
                                      AnimationState      animationState,
                                      float               fps = 30f)
        {
            Name      = animationState.name;
            LengthSec = animationState.length;

            animationState.speed = 0;
            animation.Play(animationState.name);

            var verticesList = new List<Vector3[]>();
            var normalsList  = new List<Vector3[]>();

            var deltaTime    = 1f / fps;
            var meshPosition = renderer.localToWorldMatrix;
            var meshNormal   = renderer.worldToLocalMatrix.transpose;
            
            for (float t = 0; t < LengthSec + deltaTime; t += deltaTime)
            {
                var tempMesh = new Mesh();

                animationState.time = Mathf.Clamp(t, 0f, LengthSec);
                animation.Sample();
                renderer.BakeMesh(tempMesh);

                var vertices = tempMesh.vertices;
                var normals  = tempMesh.normals;

                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = meshPosition.MultiplyPoint3x4(vertices[i]);
                    normals[i]  = meshNormal.MultiplyVector(normals[i]);
                }

                verticesList.Add(vertices);
                normalsList.Add(normals);

                Object.DestroyImmediate(tempMesh);
            }

            FrameCounts = verticesList.Count - 1;

            // Get Bounds

            var firstVertices = verticesList[0];
            var firstVertex   = firstVertices[0];

            var minX = firstVertex.x;
            var minY = firstVertex.y;
            var minZ = firstVertex.z;
            var maxX = firstVertex.x;
            var maxY = firstVertex.y;
            var maxZ = firstVertex.z;

            foreach (var vertices in verticesList)
            {
                foreach (var vertex in vertices)
                {
                    minX = Mathf.Min(minX, vertex.x);
                    minY = Mathf.Min(minY, vertex.y);
                    minZ = Mathf.Min(minZ, vertex.z);
                    maxX = Mathf.Max(maxX, vertex.x);
                    maxY = Mathf.Max(maxY, vertex.y);
                    maxZ = Mathf.Max(maxZ, vertex.z);
                }
            }

            var scale  = new Vector4(maxX - minX, maxY - minY, maxZ - minZ, 1f);
            var offset = new Vector4(minX, minY, minZ, 1f);
            Bounds = new Bounds (offset + scale * 0.5f, scale);

            // Get Texture2D

            var texWidth  = Mathf.NextPowerOfTwo(firstVertices.Length);
            var texHeight = Mathf.NextPowerOfTwo(verticesList.Count);

            PosTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false, true);
            NmlTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAFloat, false, true);

            for (var y = 0; y < verticesList.Count; y++)
            {
                var vertices = verticesList[y];
                var normals  = normalsList[y];

                for (var x = 0; x < vertices.Length; x++)
                {
                    var pos = vertices[x];
                    var nml = normals[x];

                    var posColor = new Color(pos.x, pos.y, pos.z);
                    var nmlColor = new Color(nml.x, nml.y, nml.z);

                    PosTex.SetPixel(x, y, posColor);
                    NmlTex.SetPixel(x, y, nmlColor);
                }
            }

            PosTex.Apply();
            NmlTex.Apply();
        }

        #endregion Constructor

        #region Method

        public void Dispose()
        {
            Object.DestroyImmediate(PosTex, true);
            Object.DestroyImmediate(NmlTex, true);
        }

        #endregion Method
    }
}