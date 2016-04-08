using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Game
{
    public struct Vertex : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    public interface IMesh
    {
        void Render(GraphicsDevice Device);
    }

    public partial class Mesh : IMesh
    {
        public Vertex[] verticies;
        public short[] indicies;
        public short[] lineIndicies;
        public Object Tag;

        public int VertexCount { get { return verticies.Length; } }

        public Vertex GetVertex(int i)
        {
            return verticies[i];
        }

        public void PrepareLineIndicies()
        {
            lineIndicies = new short[indicies.Length * 2];
            for (int i = 0; i < indicies.Length; i += 3)
            {
                lineIndicies[i * 2] = indicies[i];
                lineIndicies[i * 2 + 1] = indicies[i + 1];
                lineIndicies[i * 2 + 2] = indicies[i + 1];
                lineIndicies[i * 2 + 3] = indicies[i + 2];
                lineIndicies[i * 2 + 4] = indicies[i + 2];
                lineIndicies[i * 2 + 5] = indicies[i];
            }
        }

        public void Render(GraphicsDevice Device)
        {
            if (verticies.Length != 0)
                Device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verticies, 0, verticies.Length,
                    indicies, 0, indicies.Length / 3);
        }

        public static Mesh CreateFromPolygon(Vector2[] Points, Vector2[] TexCoords)
        {
            var result = new Mesh();
            result.verticies = Points.Select((p, i) => new Vertex { Position = new Vector3(p, 0.0f), TextureCoordinate = TexCoords[i] }).ToArray();
            result.indicies = new short[(Points.Length - 2) * 3];
            for (short i = 1, triangle = 0; i < Points.Length - 1; i += 1, triangle += 3)
            {
                result.indicies[triangle] = 0;
                result.indicies[triangle + 1] = i;
                result.indicies[triangle + 2] = (short)(i + 1);
            }

            return result;
        }

        public static Mesh CreateSpriteQuad()
        {
            var result = new Mesh();
            result.verticies = new Vertex[4];

            result.verticies[0].Position = new Vector3(0.0f, 0.0f, 0);
            result.verticies[1].Position = new Vector3(1.0f, 0.0f, 0);
            result.verticies[2].Position = new Vector3(1.0f, 1.0f, 0);
            result.verticies[3].Position = new Vector3(0.0f, 1.0f, 0);

            result.verticies[0].TextureCoordinate = new Vector2(0.0f, 0.0f);
            result.verticies[1].TextureCoordinate = new Vector2(1.0f, 0.0f);
            result.verticies[2].TextureCoordinate = new Vector2(1.0f, 1.0f);
            result.verticies[3].TextureCoordinate = new Vector2(0.0f, 1.0f);

            result.indicies = new short[] { 0, 2, 1, 3, 2, 0 };
            return result;
        }

        public static Mesh Merge(params Mesh[] parts)
        {
            var result = new Mesh();

            result.verticies = new Vertex[parts.Sum((p) => p.verticies.Length)];
            result.indicies = new short[parts.Sum((p) => p.indicies.Length)];

            int vCount = 0;
            int iCount = 0;
            foreach (var part in parts)
            {
                for (int i = 0; i < part.verticies.Length; ++i) result.verticies[i + vCount] = part.verticies[i];
                for (int i = 0; i < part.indicies.Length; ++i) result.indicies[i + iCount] = (short)(part.indicies[i] + vCount);
                vCount += part.verticies.Length;
                iCount += part.indicies.Length;
            }

            return result;
        }

        public static Mesh Copy(Mesh mesh)
        {
            var result = new Mesh();

            result.verticies = new Vertex[mesh.verticies.Length];
            for (int i = 0; i < mesh.verticies.Length; ++i) result.verticies[i] = mesh.verticies[i];
            result.indicies = new short[mesh.indicies.Length];
            CopyIndicies(result.indicies, 0, mesh.indicies);
            return result;
        }

        public static void CopyIndicies(short[] into, int at, short[] source)
        {
            for (int i = 0; i < source.Length; ++i)
                into[at + i] = source[i];
        }

        public static void Transform(Mesh mesh, Matrix m, int start, int count)
        {
            if (start < 0) start = mesh.verticies.Length - start;
            for (int i = start; i < start + count; ++i)
                mesh.verticies[i].Position = Vector3.Transform(mesh.verticies[i].Position, m);
        }

        public static Mesh TransformCopy(Mesh mesh, Matrix m, int start, int count)
        {
            var result = Copy(mesh);
            Transform(result, m, start, count);
            return result;
        }

        public static void Transform(Mesh mesh, Matrix m)
        {
            Transform(mesh, m, 0, mesh.verticies.Length);
        }

        public static Mesh TransformCopy(Mesh mesh, Matrix m)
        {
            return TransformCopy(mesh, m, 0, mesh.verticies.Length);
        }

        public static void MorphEx(Mesh mesh, Func<Vertex, Vertex> func)
        {
            for (int i = 0; i < mesh.verticies.Length; ++i)
                mesh.verticies[i] = func(mesh.verticies[i]);
        }

    }
}