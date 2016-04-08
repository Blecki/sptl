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
        private static short[] CubeIndicies = new short[] {
             0, 1, 2, 
                3, 0, 2,

                0, 4, 1,
                4, 5, 1,
            
                1, 5, 2,
                5, 6, 2,

                2, 6, 3,
                6, 7, 3,

                3, 7, 0,
                7, 4, 0,

                4, 6, 5,
                7, 6, 4
        };

        public static Mesh CreateCube()
        {
            var result = new Mesh();
            result.verticies = new Vertex[8];
            result.verticies[0].Position = new Vector3( -0.5f, -0.5f, -0.5f);
            result.verticies[1].Position = new Vector3(  0.5f, -0.5f, -0.5f);
            result.verticies[2].Position = new Vector3(  0.5f,  0.5f, -0.5f);
            result.verticies[3].Position = new Vector3( -0.5f,  0.5f, -0.5f);

            result.verticies[4].Position = new Vector3( -0.5f, -0.5f,  0.5f);
            result.verticies[5].Position = new Vector3(  0.5f, -0.5f,  0.5f);
            result.verticies[6].Position = new Vector3(  0.5f,  0.5f,  0.5f);
            result.verticies[7].Position = new Vector3( -0.5f,  0.5f,  0.5f);

            result.indicies = CubeIndicies;

            return result;
        }

        public static Mesh CreateSlantedCube(float Slant)
        {
            var result = new Mesh();
            result.verticies = new Vertex[8];
            result.verticies[0].Position = new Vector3(-0.5f, -0.5f, -0.5f);
            result.verticies[1].Position = new Vector3(0.5f, -0.5f, -0.5f);
            result.verticies[2].Position = new Vector3(0.5f, 0.5f, -0.5f);
            result.verticies[3].Position = new Vector3(-0.5f, 0.5f, -0.5f);

            result.verticies[4].Position = new Vector3(-0.5f, -0.5f, 0.5f);
            result.verticies[5].Position = new Vector3(0.5f, -0.5f, 0.5f);
            result.verticies[6].Position = new Vector3(0.5f, 0.5f, 0.5f + Slant);
            result.verticies[7].Position = new Vector3(-0.5f, 0.5f, 0.5f + Slant);

            result.indicies = CubeIndicies;

            return result;
        }

        public static Mesh CreateWedge(float Height)
        {
            var result = new Mesh();
            result.verticies = new Vertex[6];

            result.verticies[0].Position = new Vector3(-0.5f, -0.5f, -0.5f);
            result.verticies[1].Position = new Vector3(0.5f, -0.5f, -0.5f);
            result.verticies[2].Position = new Vector3(0.5f, 0.5f, -0.5f);
            result.verticies[3].Position = new Vector3(-0.5f, 0.5f, -0.5f);

            result.verticies[4].Position = new Vector3(0.5f, 0.5f, -0.5f + Height);
            result.verticies[5].Position = new Vector3(-0.5f, 0.5f, -0.5f + Height);

            result.indicies = new short[]
            {
                0, 1, 2, // bottom
                3, 0, 2,

                4, 5, 2, // back
                2, 5, 3,

                0, 4, 1, // top
                4, 0, 5,

                0, 3, 5, // side
                2, 1, 4, // side
            };

            return result;
        }
    }
}