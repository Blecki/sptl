using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;
using Gem.Render;

namespace Game
{
    public partial class Play : IScreen
    {
        public interface ITransientRenderItem
        {
            void Render(GraphicsDevice Device, Effect DiffuseEffect, Play Game);
        }

        private List<ITransientRenderItem> TransientRenderItems = new List<ITransientRenderItem>();

        public void AddTransientRenderItem(ITransientRenderItem Item)
        {
            TransientRenderItems.Add(Item);
        }

        public void RemoveTransientRenderItem(ITransientRenderItem Item)
        {
            TransientRenderItems.Remove(Item);
        }

        public Gem.Input Input { get; set; }
        public Main Main { get; set; }

        protected EpisodeContentManager Content;
        private Effect DiffuseEffect;
        private Effect ShadowEffect;
        private Effect LightEffect;
        private Effect BlitEffect;
        private Texture2D LightTexture;

        private RenderTarget2D DiffuseBuffer;
        private RenderTarget2D ShadowBuffer;
        private RenderTarget2D MainBuffer;

        public OrthographicCamera Camera;

        private Mesh Mesh;
        private Mesh ShadowMesh;
        private Texture2D ShadowGradient;
        private Texture2D Penumbra;
        private Texture2D Ugly;
        private Texture2D Blank;

        public Vector2 MousePosition;
        public Vector2 MouseWorldPosition;

        public TileSheet GuiSet;
        public TileSheet TileSet;
        public TileSheet WireSet;
        public TileTemplate[] TileTemplates = new TileTemplate[]
            {
            new TileTemplate
        {
            CastShadow = true,
            Solid = true,

            ShadowEdges = new ShadowEdge[]
            {
                new ShadowEdge(new Vector2(0,0), new Vector2(1,0), ShadowEdgeDirection.Up),
                new ShadowEdge(new Vector2(1,0), new Vector2(1,1), ShadowEdgeDirection.Right),
                new ShadowEdge(new Vector2(1,1), new Vector2(0,1), ShadowEdgeDirection.Down),
                new ShadowEdge(new Vector2(0,1), new Vector2(0,0), ShadowEdgeDirection.Left),
            },

            CollisionPoints = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            },

            TileIndex = 32
        },

        new TileTemplate
        {
            CastShadow = true,
            Solid = true,

            ShadowEdges = new ShadowEdge[]
            {
                new ShadowEdge(new Vector2(0,1), new Vector2(1, 0.5f), ShadowEdgeDirection.Internal),
                new ShadowEdge(new Vector2(1, 0.5f), new Vector2(1, 1), ShadowEdgeDirection.Right),
                new ShadowEdge(new Vector2(1, 1), new Vector2(0, 1), ShadowEdgeDirection.Down)
            },

            CollisionPoints = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 0.5f),
                new Vector2(1, 1)
            },

            TileIndex = 34
        },

        new TileTemplate
        {
            CastShadow = true,
            Solid = true,

            ShadowEdges = new ShadowEdge[]
            {
                new ShadowEdge(new Vector2(0, 0.5f), new Vector2(1, 0), ShadowEdgeDirection.Internal),
                new ShadowEdge(new Vector2(1, 0), new Vector2(1, 1), ShadowEdgeDirection.Right),
                new ShadowEdge(new Vector2(1, 1), new Vector2(0, 1), ShadowEdgeDirection.Down),
                new ShadowEdge(new Vector2(0, 1), new Vector2(0, 0.5f), ShadowEdgeDirection.Left)
            },

            CollisionPoints = new Vector2[]
            {
                new Vector2(0, 0.5f),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            },

            TileIndex = 35
        },

        new TileTemplate
        {
            CastShadow = true,
            Solid = true,

            ShadowEdges = new ShadowEdge[]
            {
                new ShadowEdge(new Vector2(0,0), new Vector2(1,0), ShadowEdgeDirection.Up),
                new ShadowEdge(new Vector2(1,0), new Vector2(1,1), ShadowEdgeDirection.Right),
                new ShadowEdge(new Vector2(1,1), new Vector2(0,1), ShadowEdgeDirection.Down),
                new ShadowEdge(new Vector2(0,1), new Vector2(0,0), ShadowEdgeDirection.Left),
            },

            CollisionPoints = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            },

            TileIndex = 64
        }
        };

        public ChunkedMap<Cell, MapChunkRenderBuffer<Cell>> Map;
        public ChunkedMap<Wire, MapChunkRenderBuffer<Wire>> WireMap;
        private int BottomTileSize = 16;
        private TileSheet PlayerTileSet;
        private Actor TestActor;

        private List<Module> Modules = new List<Module>();

        private TextDisplay DebugDisplay;

        #region Input States
        private Stack<InputState> InputStates = new Stack<InputState>();
        private InputState CurrentInputState { get { return InputStates.Count == 0 ? null : InputStates.Peek(); } }

        public void PushInputState(InputState Next)
        {
            if (CurrentInputState != null) CurrentInputState.Covered(this);
            InputStates.Push(Next);
            CurrentInputState.Entered(this);
        }

        public void PopInputState()
        {
            if (CurrentInputState != null)
            {
                CurrentInputState.Left(this);
                InputStates.Pop();
                if (CurrentInputState != null)
                    CurrentInputState.Exposed(this);
            }            
        }
        #endregion

        public class Light
        {
            public Vector2 Location;
            public Vector4 Color;
            public float Size;
        }

        private List<Light> Lights = new List<Light>();
        
        public Play()
        {
        }

        private void AddSlopeLine(int x, int y)
        {
            Map.GetCell(x, y).Tile = TileTemplates[1];
            Map.GetCell(x + 1, y).Tile = TileTemplates[2];
            for (int i = x + 2; i < Map.Width; ++i)
                Map.GetCell(i, y).Tile = TileTemplates[0];
        }

        public void Begin()
        {
            Content = new EpisodeContentManager(Main.EpisodeContent.ServiceProvider, "Content");
            DiffuseEffect = Content.Load<Effect>("diffuse");
            ShadowEffect = Content.Load<Effect>("shadow");
            LightEffect = Content.Load<Effect>("light");
            BlitEffect = Content.Load<Effect>("blit");

            Blank = new Texture2D(Main.GraphicsDevice, 1, 1);
            Blank.SetData(new Color[] { new Color(1, 1, 1, 1) });

            TileSet = new TileSheet(Content.Load<Texture2D>("tiles"), 16, 16);
            WireSet = new TileSheet(Content.Load<Texture2D>("wires"), 8, 8);
            GuiSet = new TileSheet(Content.Load<Texture2D>("gui"), 32, 32);

            StaticDevices.InitializeStaticDevices();

            Map = new ChunkedMap<Cell, MapChunkRenderBuffer<Cell>>(64, 64, 16, 16, 
                BottomTileSize * 2, BottomTileSize * 2,
                (x, y) => new Cell(), 
                grid => new MapChunkRenderBuffer<Cell>(grid, TileSet, c => (c.Tile == null ? -1 : c.Tile.TileIndex), BottomTileSize * 2, BottomTileSize * 2));

            WireMap = new ChunkedMap<Wire, MapChunkRenderBuffer<Wire>>(128, 128, 32, 32,
                BottomTileSize, BottomTileSize,
                (x, y) => new Wire(),
                grid => new MapChunkRenderBuffer<Wire>(grid, WireSet, 
                    w => new int[] {
                        (w.Connections == 0 ? -1 : (w.Signal == SimulationID ? w.Connections + 16 : w.Connections)),
                        (w.Cell == null ? -1 : w.Cell.TileIndex)
                    },
                    BottomTileSize, BottomTileSize));
            WireMap.ForEachCellInWorldRect(0, 0, 128 * BottomTileSize, 128 * BottomTileSize, (w, x, y) =>
            {
                w.Coordinate = new Coordinate(x, y);
            });

            for (var x = 0; x < 64; ++x)
                Map.GetCell(x, 32).Tile = TileTemplates[0];

            for (int y = 31, x = 32; y > 20; y -= 1, x += 2)
                AddSlopeLine(x, y);

            PlayerTileSet = new TileSheet(Content.Load<Texture2D>("player"), 32, 48);
            TestActor = new Player
            {
                Position = new Vector2(10, 10),
                TileSheet = PlayerTileSet,
                Width = 32,
                Height = 48
            };

            LightTexture = Content.Load<Texture2D>("light_t");
            Ugly = Content.Load<Texture2D>("ugly");

            DiffuseBuffer = new RenderTarget2D(Main.GraphicsDevice, 800, 600);
            ShadowBuffer = new RenderTarget2D(Main.GraphicsDevice, 800, 600);
            MainBuffer = new RenderTarget2D(Main.GraphicsDevice, 800, 600, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            Camera = new OrthographicCamera(Main.GraphicsDevice.Viewport);
            //Camera.Zoom(32);
            Camera.focus = new Vector2(0, 0);

            Penumbra = Content.Load<Texture2D>("penumbra");
            ShadowGradient = Content.Load<Texture2D>("shadow-gradient");

            Mesh = new Mesh();
            var q = Gem.Geo.Gen.CreateSpriteQuad();
            Gem.Geo.Gen.Transform(q, Matrix.CreateTranslation(-0.5f, -0.5f, 0));
            //Gem.Geo.Gen.Transform(q, Matrix.CreateScale(1024));
            Mesh.indicies = q.indicies;
            Mesh.verticies = q.verticies.Select(v => new Vertex { Position = v.Position, TextureCoordinate = v.TextureCoordinate }).ToArray();

            q = Gem.Geo.Gen.CreateSpriteQuad();
            //Gem.Geo.Gen.Transform(q, Matrix.CreateScale(4));
            Gem.Geo.Gen.Transform(q, Matrix.CreateTranslation(8, 8, 0));

            ShadowMesh = new Mesh();
            ShadowMesh.indicies = q.indicies;
            ShadowMesh.verticies = q.verticies.Select(v => new Vertex { Position = v.Position, TextureCoordinate = v.TextureCoordinate }).ToArray();

            Input.AddBinding("PENUMBRA", new KeyboardBinding(Keys.P, KeyBindingType.Held));
            Input.AddBinding("NOSHADOWS", new KeyboardBinding(Keys.O, KeyBindingType.Held));
            Input.AddBinding("LEFTCLICK", new MouseButtonBinding("LeftButton", KeyBindingType.Pressed));
            Input.AddBinding("LEFTPRESS", new MouseButtonBinding("LeftButton", KeyBindingType.Held));
            Input.AddBinding("RIGHT", new KeyboardBinding(Keys.D, KeyBindingType.Held));
            Input.AddBinding("LEFT", new KeyboardBinding(Keys.A, KeyBindingType.Held));
            Input.AddBinding("UP", new KeyboardBinding(Keys.W, KeyBindingType.Held));
            Input.AddBinding("DOWN", new KeyboardBinding(Keys.S, KeyBindingType.Held));
            Input.AddBinding("JUMP", new KeyboardBinding(Keys.Space, KeyBindingType.Held));
            Input.AddBinding("ROTATEDEVICE", new KeyboardBinding(Keys.Z, KeyBindingType.Pressed));
            Input.AddBinding("ESCAPE", new KeyboardBinding(Keys.Escape, KeyBindingType.Pressed));

            Lights.Add(new Light { Size = 64 * 16, Location = new Vector2(2, 2), Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f) });
            Lights.Add(new Light { Size = 32 * 16, Color = Vector4.One });


            DebugDisplay = new TextDisplay(50, 2, new Gem.Gui.BitmapFont(Content.Load<Texture2D>("small-font"), 6, 8, 6), Main.GraphicsDevice, Content);

            Modules.Add(new PhysicsModule(Map));
            Modules.Add(new EntityStateModule());

            foreach (var module in Modules) module.NewEntity(TestActor);

            PushInputState(new MainInputState(this, Main.GraphicsDevice, Content));
        }

        public void End()
        {
        }

        public void Update(float ElapsedSeconds)
        {
            MousePosition = Input.QueryAxis("MAIN");
            var unprojectedMPos = Camera.Unproject(new Vector3(MousePosition, 0));
            MouseWorldPosition = new Vector2(unprojectedMPos.X, unprojectedMPos.Y);

            if (CurrentInputState != null)
                CurrentInputState.Update(this);

            //if (Input.Check("LEFTCLICK"))
            //    Map[unprojectedMPos.X, unprojectedMPos.Y].Tile = 65;

            TestActor.Input = Input;

            foreach (var module in Modules) module.Update(ElapsedSeconds);

            Lights[0].Location = TestActor.Position;
            Camera.focus = TestActor.Position;

            DebugDisplay.SetString(String.Format("{0}, {1}             ", MouseWorldPosition.X, MouseWorldPosition.Y), 0, 0);
        }
        
        public void Draw(float elapsedSeconds)
        {
            var _viewport = Main.GraphicsDevice.Viewport;

            Main.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Main.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Main.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            Main.GraphicsDevice.SetRenderTarget(MainBuffer);
            Main.GraphicsDevice.Clear(Color.Black);

            Main.GraphicsDevice.SetRenderTarget(DiffuseBuffer);
            Main.GraphicsDevice.Clear(Color.White);

            DiffuseEffect.CurrentTechnique = DiffuseEffect.Techniques[0];

            DiffuseEffect.Parameters["World"].SetValue(Matrix.Identity);
            DiffuseEffect.Parameters["View"].SetValue(Camera.View);
            DiffuseEffect.Parameters["Projection"].SetValue(Camera.Projection);
            DiffuseEffect.Parameters["Texture"].SetValue(TileSet.Texture);
            DiffuseEffect.Parameters["DiffuseColor"].SetValue(Vector4.One);
            DiffuseEffect.Parameters["Alpha"].SetValue(1.0f);
            DiffuseEffect.Parameters["ClipAlpha"].SetValue(0.2f);
            DiffuseEffect.Parameters["UVTransform"].SetValue(Matrix.Identity);

            var topLeft = Camera.Unproject(Vector3.Zero);
            var bottomRight = Camera.Unproject(new Vector3(800, 600, 0));

            Map.ForEachChunkInWorldRect(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y,
                (chunk, x, y) =>
                {
                    DiffuseEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(x, y, 0));
                    DiffuseEffect.CurrentTechnique.Passes[0].Apply();
                    chunk.Render(Main.GraphicsDevice);
                });

            DiffuseEffect.Parameters["Texture"].SetValue(WireSet.Texture);

            WireMap.ForEachChunkInWorldRect(
                topLeft.X,
                topLeft.Y,
                bottomRight.X - topLeft.X,
                bottomRight.Y - topLeft.Y,
                (chunk, x, y) =>
                {
                    DiffuseEffect.Parameters["World"].SetValue(Matrix.CreateTranslation(x, y, 0));
                    DiffuseEffect.CurrentTechnique.Passes[0].Apply();
                    chunk.Render(Main.GraphicsDevice);
                });

            foreach (var module in Modules)
                module.RenderDiffuse(Main.GraphicsDevice, DiffuseEffect);

            foreach (var transient in TransientRenderItems)
                transient.Render(Main.GraphicsDevice, DiffuseEffect, this);

            #region Lights
            /*
            foreach (var light in Lights)
            {
                Main.GraphicsDevice.SetRenderTarget(ShadowBuffer);
                Main.GraphicsDevice.Clear(Color.Black);

                #region Shadows

                ShadowEffect.CurrentTechnique = ShadowEffect.Techniques[0];
                ShadowEffect.Parameters["World"].SetValue(Camera.World);
                ShadowEffect.Parameters["View"].SetValue(Camera.View);
                ShadowEffect.Parameters["Projection"].SetValue(Camera.Projection);

                // Limit this only to the span defined by the set of lights that can affect the screen.

                Map.ForEachCell(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y,
                (c, x, y) =>
                {
                    if (c.Tile != null && c.Tile.CastShadow)
                    {
                        var offset = new Vector2(x, y);
                        foreach (var shadowEdge in c.Tile.ShadowEdges)
                        {
                            if (shadowEdge.Direction == ShadowEdgeDirection.Internal)
                                DrawSegmentShadow(offset + shadowEdge.V0, offset + shadowEdge.V1, light.Location);
                            else
                            {
                                var neighborCoordinate = ShadowEdge.EdgeNeighbor(new Coordinate(x, y), shadowEdge.Direction);
                                var neighbor = Map[neighborCoordinate];
                                if (neighbor.Tile == null || !neighbor.Tile.CastShadow)
                                    DrawSegmentShadow(offset + shadowEdge.V0, offset + shadowEdge.V1, light.Location);
                                else
                                {
                                    var oppositeDirection = ShadowEdge.Opposite(shadowEdge.Direction);
                                    var adjacentEdge = neighbor.Tile.ShadowEdges.FirstOrDefault(e => e.Direction == oppositeDirection);
                                    if (adjacentEdge == null)
                                        DrawSegmentShadow(offset + shadowEdge.V0, offset + shadowEdge.V1, light.Location);
                                }
                            }
                        }
                            
                    }
                });

                #endregion


                Main.GraphicsDevice.SetRenderTarget(MainBuffer);
                LightEffect.CurrentTechnique = LightEffect.Techniques[0];

                LightEffect.Parameters["World"].SetValue(Matrix.CreateScale(light.Size) * Matrix.CreateTranslation(light.Location.X, light.Location.Y, 0));
                LightEffect.Parameters["View"].SetValue(Camera.View);
                LightEffect.Parameters["Projection"].SetValue(Camera.Projection);
                LightEffect.Parameters["Diffuse"].SetValue(DiffuseBuffer);
                LightEffect.Parameters["Shadow"].SetValue(ShadowBuffer);
                LightEffect.Parameters["Light"].SetValue(LightTexture);
                LightEffect.Parameters["Color"].SetValue(light.Color);

                LightEffect.CurrentTechnique.Passes[0].Apply();

                Mesh.Render(Main.GraphicsDevice);
            }
            */
            #endregion

            Main.GraphicsDevice.SetRenderTarget(null);
            BlitEffect.CurrentTechnique = BlitEffect.Techniques[0];
            BlitEffect.Parameters["World"].SetValue(Matrix.Identity);
            BlitEffect.Parameters["View"].SetValue(Matrix.Identity);
            BlitEffect.Parameters["Projection"].SetValue(Matrix.CreateOrthographic(1, 1, -1, 1));
            BlitEffect.Parameters["Diffuse"].SetValue(DiffuseBuffer /* Change to MainBuffer for lighting */);
            BlitEffect.CurrentTechnique.Passes[0].Apply();
            Mesh.Render(Main.GraphicsDevice);

            DebugDisplay.Draw(Main.GraphicsDevice, new Viewport(0, _viewport.Height - 64, _viewport.Width, 64));

            foreach (var inputState in InputStates)
                inputState.Render(Main.GraphicsDevice, DiffuseEffect, this);
        }

        private Vertex[] ShadowGeometryBuffer = new Vertex[4];

        private void DrawSegmentShadow(
           Vector2 P0, Vector2 P1,
           Vector2 LightPosition)
        {
            if (Input.Check("NOSHADOWS")) return;

            var normal = Gem.Math.Vector.EdgeNormal(P0, P1);
            var lightDirection = LightPosition - P0;
            if (Vector2.Dot(normal, lightDirection) <= 0) return;

            var v0 = new Vector2(P0.X, P0.Y); //Vector2.Transform(P0, Transform);
            var v1 = new Vector2(P1.X, P1.Y); //Vector2.Transform(P1, Transform);

            var A = 24.0;

            var F2 = (v1 - v0).LengthSquared();
            var H2 = (LightPosition - v0).LengthSquared();
            var G2 = (LightPosition - v1).LengthSquared();
            var F = (v1 - v0).Length();
            var H = (LightPosition - v0).Length();
            var G = (LightPosition - v1).Length();

            var CosI = (H2 - G2 - F2) / (-2 * G * F);
            var I = System.Math.Acos(CosI);
            var J = (0.5 * System.Math.PI) - I;

            var CosK = (G2 - F2 - H2) / (-2 * F * H);
            var K = System.Math.Acos(CosK);
            var L = (0.5 * System.Math.PI) - K;

            var C = A / System.Math.Cos(J);
            var E = A / System.Math.Cos(L);
            //var B = A / System.Math.Tan(I);
            //var D = A / System.Math.Tan(L);            

            var e0 = v0 - LightPosition;
            var e1 = v1 - LightPosition;

            e0.Normalize();
            e1.Normalize();

            var lv = v1 - v0;
            lv.Normalize();
            lv *= 16.0f;
            
            ShadowEffect.Parameters["Texture"].SetValue(ShadowGradient);

            ShadowEffect.CurrentTechnique.Passes[0].Apply();

                ShadowGeometryBuffer[0].Position = new Vector3(v0, 0.0f);
                ShadowGeometryBuffer[1].Position = new Vector3(v1, 0.0f);

                ShadowGeometryBuffer[2].Position = new Vector3(v0 + (e0 * (float)E), 0.0f);
                ShadowGeometryBuffer[3].Position = new Vector3(v1 + (e1 * (float)C), 0.0f);


                ShadowGeometryBuffer[0].TextureCoordinate = new Vector2(1, 0);
                ShadowGeometryBuffer[1].TextureCoordinate = new Vector2(0, 0);
                ShadowGeometryBuffer[2].TextureCoordinate = new Vector2(1, 1);
                ShadowGeometryBuffer[3].TextureCoordinate = new Vector2(0, 1);

                Main.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, ShadowGeometryBuffer, 0, 2);

                if (!Input.Check("PENUMBRA")) return;

                ShadowEffect.Parameters["Texture"].SetValue(Penumbra);
                ShadowEffect.CurrentTechnique.Passes[0].Apply();

                ShadowGeometryBuffer[0].Position = new Vector3(v0, 0.0f);
                ShadowGeometryBuffer[1].Position = new Vector3(v0 + (e0 * (float)E)/* + lv*/, 0.0f);
                ShadowGeometryBuffer[2].Position = new Vector3(v0 + (e0 * (float)E) - lv, 0.0f);

                ShadowGeometryBuffer[0].TextureCoordinate = new Vector2(0, 1);
                ShadowGeometryBuffer[1].TextureCoordinate = new Vector2(1, 0);
                ShadowGeometryBuffer[2].TextureCoordinate = new Vector2(0, 0);

                Main.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, ShadowGeometryBuffer, 0, 1);

                ShadowGeometryBuffer[0].Position = new Vector3(v1, 0.0f);
                ShadowGeometryBuffer[1].Position = new Vector3(v1 + (e1 * (float)C)/* - lv*/, 0.0f);
                ShadowGeometryBuffer[2].Position = new Vector3(v1 + (e1 * (float)C) + lv, 0.0f);

                ShadowGeometryBuffer[0].TextureCoordinate = new Vector2(0, 1);
                ShadowGeometryBuffer[1].TextureCoordinate = new Vector2(1, 0);
                ShadowGeometryBuffer[2].TextureCoordinate = new Vector2(0, 0);

                Main.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, ShadowGeometryBuffer, 0, 1);
        }

        private List<DeviceSignalActivation> _activations = new List<DeviceSignalActivation>();

        public void Simulate()
        {
            _activations = SimulateSignals(_activations);
        }
    }
}
