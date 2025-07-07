using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TGC.MonoGame.Samples.Physics.Bepu;


/*
create a new project in Visual Studio
nuget install BepuPhysics
nuget install BepuUtilities
add callback files and using TGC.MonoGame.Samples.Physics.Bepu

*/
namespace BepuPhysicsSimpleExample
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;

        private Model _cube;
        private Model _floor;
        private Microsoft.Xna.Framework.Matrix _world = Microsoft.Xna.Framework.Matrix.Identity;
        private Microsoft.Xna.Framework.Matrix _view;
        private Microsoft.Xna.Framework.Matrix _projection;

        private Simulation _simulation;    //BepuPhysics
        private BufferPool _bufferPool;    //BepuPhysics
        private BodyHandle _boxBodyHandle;         //BepuPhysics

        private SimpleThreadDispatcher _threadDispatcher; //BepuPhysics 

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _view = Microsoft.Xna.Framework.Matrix.CreateLookAt(new Vector3(0, 0, 50), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            _projection = Microsoft.Xna.Framework.Matrix.CreatePerspectiveFieldOfView(Microsoft.Xna.Framework.MathHelper.ToRadians(45), 1920f / 1080f, 0.1f, 100f);

            _cube = Content.Load<Model>("cube");
            _floor = Content.Load<Model>("floor100");


            _bufferPool = new BufferPool(); //BepuPhysics
            _simulation = Simulation.Create(_bufferPool, new NarrowPhaseCallbacks(new SpringSettings(30, 1)), 
                new PoseIntegratorCallbacks(new System.Numerics.Vector3(0, -1, 0)), new SolveDescription(8, 1)); //BepuPhysics


            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            _threadDispatcher = new SimpleThreadDispatcher(targetThreadCount);  //BepuPhysics
            
            _simulation.Statics.Add(new StaticDescription(new System.Numerics.Vector3(0, -0.5f, 0), _simulation.Shapes.Add(new Box(20000, 1, 20000)))); //BepuPhysics

            
            var boxShape = new Box(1, 1, 1);                  //BepuPhysics
            var boxInertia = boxShape.ComputeInertia(0.4f);   //BepuPhysics
            var boxIndex = _simulation.Shapes.Add(boxShape);  //BepuPhysics
            var position = new System.Numerics.Vector3(0, 10, 0);  //BepuPhysics

            var bodyDescription = BodyDescription.CreateDynamic(position, boxInertia, 
                                            new CollidableDescription(boxIndex, 0.1f), new BodyActivityDescription(0.01f));  //BepuPhysics
            bodyDescription.Pose.Orientation = QuaternionEx.CreateFromAxisAngle(new System.Numerics.Vector3(0, 1, 1), 
                                                                                BepuUtilities.MathHelper.ToRadians(-50));  //BepuPhysics
            var _boxBodyHandle = _simulation.Bodies.Add(bodyDescription);   //BepuPhysics

            foreach (ModelMesh mesh in _cube.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _simulation.Timestep(1 / 60f, _threadDispatcher); //BepuPhysics

            var pose = _simulation.Bodies.GetBodyReference(_boxBodyHandle).Pose; //BepuPhysics
            var position = pose.Position; //BepuPhysics
            var quaternion = pose.Orientation; //BepuPhysics
            _world = Microsoft.Xna.Framework.Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W)) *
                                                Microsoft.Xna.Framework.Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z)); //BepuPhysics
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);


            foreach (ModelMesh mesh in _floor.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Microsoft.Xna.Framework.Matrix.CreateTranslation(0, -1, 0);
                    effect.View = _view;
                    effect.Projection = _projection;
                }
                mesh.Draw();
            }
            foreach (ModelMesh mesh in _cube.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _world;
                    effect.View = _view;
                    effect.Projection = _projection;

                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
