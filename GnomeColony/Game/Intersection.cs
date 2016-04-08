using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Game
{
    public class Intersection
    {
        public static bool PolygonPolygon(Vector2[] P0, Vector2[] P1)
        {
            for (int v = 0; v < P0.Length; ++v)
            {
                Vector2 B = P0[v];
                Vector2 A = P0[v == 0 ? (P0.Length - 1) : (v - 1)];

                Vector2 Edge = B - A;
                Vector2 Normal = new Vector2(-Edge.Y, Edge.X);

                float dotMin1, dotMax1, dotMin2, dotMax2;

                dotMin1 = dotMax1 = Vector2.Dot(Normal, P0[0]);
                for (int i = 1; i < P0.Length; ++i)
                {
                    float dot = Vector2.Dot(Normal, P0[i]);
                    if (dot < dotMin1) dotMin1 = dot;
                    if (dot > dotMax1) dotMax1 = dot;
                }

                dotMin2 = dotMax2 = Vector2.Dot(Normal, P1[0]);
                for (int i = 1; i < P1.Length; ++i)
                {
                    float dot = Vector2.Dot(Normal, P1[i]);
                    if (dot < dotMin2) dotMin2 = dot;
                    if (dot > dotMax2) dotMax2 = dot;
                }

                if (dotMax1 <= dotMin2 || dotMin1 > dotMax2) return false;
            }

            for (int v = 0; v < P1.Length; ++v)
            {
                Vector2 B = P1[v];
                Vector2 A = P1[v == 0 ? (P1.Length - 1) : v - 1];

                Vector2 Edge = B - A;
                Vector2 Normal = new Vector2(-Edge.Y, Edge.X);

                float dotMin1, dotMax1, dotMin2, dotMax2;

                dotMin1 = dotMax1 = Vector2.Dot(Normal, P0[0]);
                for (int i = 1; i < P0.Length; ++i)
                {
                    float dot = Vector2.Dot(Normal, P0[i]);
                    if (dot < dotMin1) dotMin1 = dot;
                    if (dot > dotMax1) dotMax1 = dot;
                }

                dotMin2 = dotMax2 = Vector2.Dot(Normal, P1[0]);
                for (int i = 1; i < P1.Length; ++i)
                {
                    float dot = Vector2.Dot(Normal, P1[i]);
                    if (dot < dotMin2) dotMin2 = dot;
                    if (dot > dotMax2) dotMax2 = dot;
                }

                if (dotMax1 <= dotMin2 || dotMin1 > dotMax2) return false;
            }

            return true;
        }

        public static bool PolygonWithAABB(Vector2[] P0, Gem.AABB AABB)
        {
            Vector2[] P = new Vector2[4];
            P[0] = new Vector2(AABB.X, AABB.Y);
            P[1] = new Vector2(AABB.X + AABB.Width, AABB.Y);
            P[2] = new Vector2(AABB.X + AABB.Width, AABB.Y + AABB.Height);
            P[3] = new Vector2(AABB.X, AABB.Y + AABB.Height);
            return PolygonPolygon(P0, P);

        }

    }
}
