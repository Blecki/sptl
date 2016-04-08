﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gem.Geo
{
    public partial class Gen
    {
        public static Mesh CreateUnitPolygon(int sides)
        {
            return CreateUnitPolygon(sides, Vector3.UnitY, Vector3.UnitZ);
        }

        public static Mesh CreateUnitPolygon(int sides, Vector3 radial, Vector3 axis)
        {
            var result = new Mesh();
            result.verticies = new Vertex[sides + 1];
            result.verticies[0].Position = new Vector3(0, 0, 0);
            for (int i = 0; i < sides; ++i)
            {
                var matrix = Matrix.CreateFromAxisAngle(axis, MathHelper.ToRadians((360.0f / sides) * i));
                result.verticies[i + 1].Position = Vector3.Transform(radial, matrix);
            }

            result.indicies = new short[3 * sides];
            for (int i = 0; i < sides; ++i)
            {
                result.indicies[i * 3] = 0;
                result.indicies[(i * 3) + 2] = (short)(i + 1);
                result.indicies[(i * 3) + 1] = (short)(i == (sides - 1) ? 1 : (i + 2));
            }

            return result;
        }

        public static Mesh CreateQuad()
        {
            var result = new Mesh();
            result.verticies = new Vertex[4];

            result.verticies[0].Position = new Vector3(-0.5f, -0.5f, 0);
            result.verticies[1].Position = new Vector3(0.5f, -0.5f, 0);
            result.verticies[2].Position = new Vector3(0.5f, 0.5f, 0);
            result.verticies[3].Position = new Vector3(-0.5f, 0.5f, 0);

            for (int i = 0; i < 4; ++i)
            {
                result.verticies[i].TextureCoordinate =
                    new Vector2(result.verticies[i].Position.X + 0.5f, 1.0f - (result.verticies[i].Position.Y + 0.5f));
                result.verticies[i].Normal = -Vector3.UnitZ;
            }

            result.indicies = new short[] { 0, 2, 1, 3, 2, 0 };
            return result;
        }

        public static Mesh CreateSlantedQuad(float Slant)
        {
            var result = new Mesh();
            result.verticies = new Vertex[4];

            result.verticies[0].Position = new Vector3(-0.5f, -0.5f, 0);
            result.verticies[1].Position = new Vector3(0.5f, -0.5f, 0);
            result.verticies[2].Position = new Vector3(0.5f, 0.5f, Slant);
            result.verticies[3].Position = new Vector3(-0.5f, 0.5f, Slant);

            for (int i = 0; i < 4; ++i)
            {
                result.verticies[i].TextureCoordinate =
                    new Vector2(result.verticies[i].Position.X + 0.5f, result.verticies[i].Position.Y + 0.5f);
                result.verticies[i].Normal = -Vector3.UnitZ;
            }

            result.indicies = new short[] { 0, 2, 1, 3, 2, 0 };
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

            result.verticies[0].TextureCoordinate = new Vector2(0.0f, 1.0f);
            result.verticies[1].TextureCoordinate = new Vector2(1.0f, 1.0f);
            result.verticies[2].TextureCoordinate = new Vector2(1.0f, 0.0f);
            result.verticies[3].TextureCoordinate = new Vector2(0.0f, 0.0f);

            for (int i = 0; i < 4; ++i)
            {
                result.verticies[i].Normal = -Vector3.UnitZ;
            }

            result.indicies = new short[] { 0, 2, 1, 3, 2, 0 };
            return result;
        }
    }
}