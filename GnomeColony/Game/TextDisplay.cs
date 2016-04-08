using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Game
{
    /// <summary>
    /// Renders a grid of characters.
    /// </summary>
    public class TextDisplay
    {
        private class Line
        {
            internal VertexBuffer buffer;
            internal bool dirty = true;
            internal Gem.Console.ConsoleVertex[] verts;
        }
        
        Line[] lines;
        IndexBuffer indexBuffer;
        Effect effect;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Gem.Gui.BitmapFont Font { get; private set; }

        public TextDisplay(int width, int height, Gem.Gui.BitmapFont font, GraphicsDevice device, ContentManager content)
        {
            this.Width = width;
            this.Height = height;

            this.Font = font;
            effect = content.Load<Effect>("draw-console");


            //Prepare the index buffer.
            var indicies = new short[width * 6];
            for (int i = 0; i < width; ++i)
            {
                indicies[i * 6 + 0] = (short)(i * 4 + 0);
                indicies[i * 6 + 1] = (short)(i * 4 + 1);
                indicies[i * 6 + 2] = (short)(i * 4 + 2);
                indicies[i * 6 + 3] = (short)(i * 4 + 2);
                indicies[i * 6 + 4] = (short)(i * 4 + 3);
                indicies[i * 6 + 5] = (short)(i * 4 + 0);
            }

            indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indicies.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indicies);

            //Prepare vertex buffers
            lines = new Line[height];
        
            for (int y = 0; y < height; ++y)
            {
                lines[y] = new Line();
                lines[y].verts = new Gem.Console.ConsoleVertex[width * 4];
                for (int i = 0; i < width; ++i)
                {
                    lines[y].verts[i * 4 + 0].Position = new Vector3(i * font.glyphWidth, y * font.glyphHeight, 0);
                    lines[y].verts[i * 4 + 1].Position = new Vector3((i + 1) * font.glyphWidth, y * font.glyphHeight, 0);
                    lines[y].verts[i * 4 + 2].Position = new Vector3((i + 1) * font.glyphWidth, (y + 1) * font.glyphHeight, 0);
                    lines[y].verts[i * 4 + 3].Position = new Vector3(i * font.glyphWidth, (y + 1) * font.glyphHeight, 0);

                    lines[y].verts[i * 4 + 0].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 1].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 2].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 3].FGColor = Color.White.ToVector4();
                    lines[y].verts[i * 4 + 0].BGColor = Color.Black.ToVector4();
                    lines[y].verts[i * 4 + 1].BGColor = Color.Black.ToVector4();
                    lines[y].verts[i * 4 + 2].BGColor = Color.Black.ToVector4();
                    lines[y].verts[i * 4 + 3].BGColor = Color.Black.ToVector4();
                }
                lines[y].buffer = new VertexBuffer(device, typeof(Gem.Console.ConsoleVertex), lines[y].verts.Length, BufferUsage.None);
            }
        }

        public void SetChar(char character, int Column, int Row, Color? fg = null, Color? bg = null)
        {
            if (Column < 0 || Column >= Width) return;
            if (Row < 0 || Row >= Height) return;

            //Find character in font
            var charX = character % Font.Columns;
            var charY = character / Font.Columns;

            //Map glyph to quad
            lines[Row].verts[Column * 4 + 0].TextureCoordinate = new Vector2(
                (float)(charX * Font.fgWidth), 
                (float)(charY * Font.fgHeight));
            lines[Row].verts[Column * 4 + 1].TextureCoordinate = new Vector2(
                (float)((charX + 1) * Font.fgWidth), 
                (float)(charY * Font.fgHeight));
            lines[Row].verts[Column * 4 + 2].TextureCoordinate = new Vector2(
                (float)((charX + 1) * Font.fgWidth),
                (float)((charY + 1) * Font.fgHeight));
            lines[Row].verts[Column * 4 + 3].TextureCoordinate = new Vector2(
                (float)(charX * Font.fgWidth), 
                (float)((charY + 1) * Font.fgHeight));

            if (fg != null)
            {
                lines[Row].verts[Column * 4 + 0].FGColor = fg.Value.ToVector4();
                lines[Row].verts[Column * 4 + 1].FGColor = fg.Value.ToVector4();
                lines[Row].verts[Column * 4 + 2].FGColor = fg.Value.ToVector4();
                lines[Row].verts[Column * 4 + 3].FGColor = fg.Value.ToVector4();
            }

            if (bg != null)
            {
                lines[Row].verts[Column * 4 + 0].BGColor = bg.Value.ToVector4();
                lines[Row].verts[Column * 4 + 1].BGColor = bg.Value.ToVector4();
                lines[Row].verts[Column * 4 + 2].BGColor = bg.Value.ToVector4();
                lines[Row].verts[Column * 4 + 3].BGColor = bg.Value.ToVector4();
            }


            lines[Row].dirty = true;
        }

        public void SetString(String S, int Column, int Row, Color? FG = null, Color? BG = null)
        {
            foreach (var c in S)
            {
                SetChar(c, Column, Row, FG, BG);
                Column += 1;
            }
        }

        public void Draw(GraphicsDevice device, Viewport Port)
        {
            var _viewport = device.Viewport;
            device.Viewport = Port;

            effect.Parameters["Texture"].SetValue(Font.fontData);
            effect.Parameters["Projection"].SetValue(
                Matrix.CreateOrthographicOffCenter(0, Width * Font.glyphWidth, Height * Font.glyphHeight, 0, -1, 1));
            effect.Parameters["View"].SetValue(Matrix.Identity);
            effect.Parameters["World"].SetValue(Matrix.Identity);

            effect.CurrentTechnique = effect.Techniques[0];

            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.AlphaBlend;

            device.Indices = indexBuffer;

            effect.CurrentTechnique.Passes[0].Apply();

            foreach (var line in lines)
            {
                if (line.dirty)
                {
                    line.buffer.SetData(line.verts);
                    line.dirty = false;
                }
                device.SetVertexBuffer(line.buffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Width * 4, 0, Width * 2);
            }

            device.SetVertexBuffer(null);

            device.Viewport = _viewport;
        }
    }
}
