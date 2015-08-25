using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ZombieAttack
{
    //GameObject Class
    class GameObject
    {
        public Model model { get; set; }
        public Vector3 position { get; set; }

        public GameObject()
        {
            model = null;
            position = Vector3.Zero;
            //bounding_sphere = new BoundingSphere();
        }
        /*
        protected BoundingSphere Calculate_Bounding_Sphere()
        {
            BoundingSphere merged_sphere = new BoundingSphere();
            BoundingSphere[] bounding_spheres;
            int index = 0;
            int mesh_count = model.Meshes.Count;

            bounding_spheres = new BoundingSphere[mesh_count];
            foreach (ModelMesh mesh in model.Meshes)
            {
                bounding_spheres[index++] = mesh.BoundingSphere;
            }

            merged_sphere = bounding_spheres[0];
            if ((model.Meshes.Count) > 1)
            {
                index = 1;
                do
                {
                    merged_sphere = BoundingSphere.CreateMerged(merged_sphere,
                        bounding_spheres[index]);
                    index++;
                } while (index < model.Meshes.Count);
            }

            merged_sphere.Center.Y = 0;
            merged_sphere.Radius *= 0.2f;
            return merged_sphere;
        }

        internal void DrawBoundingSphere(Matrix view, Matrix projection,
            GameObject bounding_sphere_model)
        {
            Matrix scale_matrix = Matrix.CreateScale(bounding_sphere.Radius);
            Matrix translate_matrix =
                Matrix.CreateTranslation(bounding_sphere.Center);
            Matrix world_matrix = scale_matrix * translate_matrix;

            foreach (ModelMesh mesh in bounding_sphere_model.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world_matrix;
                    effect.View = view;
                    effect.Projection = projection;
                }
                mesh.Draw();
            }
        }
        */
    }
    //End GameObject Class

    //Zombie Class
    class Zombie : GameObject
    {
        public bool Killed { get; set; }
        public int health { get; set; }
        private Vector3 directionVector;
        private Vector3 last_pos;


        public Zombie()
            : base()
        {
            Killed = false;
            health = GlobalState.MAX_ZOMBIE_HEALTH;
            directionVector = new Vector3(0, 0, 0);
            last_pos = new Vector3(0, 0, 0);
        }

        public void LoadContent(ContentManager content, string model_name)
        {
            model = content.Load<Model>(model_name);
            position = Vector3.Down;
        }

        public Vector3 GetTranslation(Vector3 playerPosition)
        {
            Vector3 futurePosition;

            futurePosition = position + Vector3.Normalize((playerPosition - position))*0.2f;

            futurePosition.Y = position.Y;
            
            return futurePosition;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            
            
            Matrix translate_matrix = Matrix.CreateTranslation(position);
            


            //Matrix world_matrix = translate_matrix;

            Matrix world_matrix = Matrix.CreateWorld(position, directionVector, new Vector3(0, 1.0f, 0));

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateScale(0.5f, 0.5f, 0.5f) * world_matrix * Matrix.CreateTranslation(0.0f, 0.0f, 0.0f);
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

        private int Calc_Distance(Vector3 object_pos, Vector3 future_position)
        {
            //x2 - x1 s +
            float num = (object_pos.X - position.X);
            num *= num;
            float num2 = (object_pos.Z - position.Z);
            num2 *= num2;

            return (int)Math.Round(Math.Sqrt((double)(num + num2)));
        }

        //need to add collision detection for trees and bullets
        public void Update(Vector3 player_pos)
        {
           
                Vector3 future_pos = GetTranslation(player_pos);
                if (Calc_Distance(player_pos, future_pos) > GlobalState.COLLISION_DISTANCE)
                {
                    position = future_pos;

                    directionVector = - position + last_pos;

                    last_pos = position;
                }
            
        }
    }
    //End Zombie Class

    //Tree Class
    class Tree : GameObject
    {
        public Tree()
            : base()
        {
        }

        public void LoadContent(ContentManager content, string model_name)
        {
            model = content.Load<Model>(model_name);
            position = Vector3.Down;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix translate_matrix = Matrix.CreateTranslation(position);
            Matrix world_matrix = translate_matrix;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World =
                        transforms[mesh.ParentBone.Index] * Matrix.CreateScale(3.0f, 3.0f, 3.0f) * world_matrix * Matrix.CreateTranslation(0.0f, -5.0f, 0.0f);
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

    }
    //End Tree Class

    //Player Class
    class Player : GameObject
    {

        public float forward_direction { get; set; }
        public int max_range { get; set; }
        public int health { get; set; }
        public int health_timer { get; set; }
        //may need variables to show which guns the player has and which is currently selected

        public Player()
            : base()
        {
            forward_direction = 0.0f;
            max_range = GlobalState.MAX_RANGE;
            health = GlobalState.MAX_PLAYER_HEALTH;
            position = new Vector3(0, 0, 0);
            health_timer = 0;
        }

        public void LoadContent(ContentManager content, string model_name)
        {
            model = content.Load<Model>(model_name);
        }

        internal void Reset()
        {
            position = Vector3.Zero;
            forward_direction = 0.0f;
            health = GlobalState.MAX_PLAYER_HEALTH;
        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            Matrix world_matrix = Matrix.Identity;
            Matrix rotationY_matrix = Matrix.CreateRotationY(forward_direction);
            Matrix translate_matrix = Matrix.CreateTranslation(position);

            world_matrix = rotationY_matrix * translate_matrix;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world_matrix *= transforms[mesh.ParentBone.Index];
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

        public void Update(GamePadState gamepad_state, KeyboardState keyboard_state, Tree[] trees, Zombie[] zombies, double elapsedTime)
        {
            Vector3 future_position = position;
            health_timer++;
            float turn_amount = 0;

            if (keyboard_state.IsKeyDown(Keys.A))
                turn_amount = 1;
            else if (keyboard_state.IsKeyDown(Keys.D))
                turn_amount = -1;
            else if (gamepad_state.ThumbSticks.Left.X != 0)
                turn_amount = gamepad_state.ThumbSticks.Left.X;

            //calculate turn amount and direction
            forward_direction += turn_amount * GlobalState.PLAYER_TURN_SPEED * (float) elapsedTime;
            Matrix orientation_matrix = Matrix.CreateRotationY(forward_direction);

            Vector3 movement = Vector3.Zero;
            if (keyboard_state.IsKeyDown(Keys.W))
                movement.Z = 1;
            else if (keyboard_state.IsKeyDown(Keys.S))
                movement.Z = -1;
            else if (gamepad_state.ThumbSticks.Left.Y != 0)
                movement.Z = gamepad_state.ThumbSticks.Left.Y;

            //if (movement.Z > 0)
            //{
                Vector3 speed = Vector3.Transform(movement, orientation_matrix);
                speed *= GlobalState.PLAYER_VELOCITY;
                future_position = position + speed;

                if (Validate_Movement(future_position, trees, zombies))
                {
                    position = future_position;
                }
                else position -= (3*speed); 
        }

        private bool Validate_Movement(Vector3 future_position, Tree[] trees, Zombie[] zombies)
        {
            

            //Don't allow player to go off map
            if ((Math.Abs(future_position.X) > max_range) ||
                (Math.Abs(future_position.Z) > max_range))
                return false;

            //Don't allow player to go through trees
            if (Check_For_Tree_Collision(future_position, trees))
                return false;
            if (Check_For_Zombie_Collision(future_position, zombies))
            {
                if (health_timer > 15)
                {
                    health -= 10;
                    health_timer = 0;
                }
                return true;
            }
            return true;
        }

        private int Calc_Distance(Vector3 object_pos, Vector3 future_position)
        {
            //x2 - x1 s +
            float num = (object_pos.X - position.X);
            num *= num;
            float num2 = (object_pos.Z - position.Z);
            num2 *= num2;
            
            return (int)Math.Round(Math.Sqrt((double)(num + num2)));
        }

        private bool Check_For_Tree_Collision(Vector3 future_position, Tree[] trees)
        {
            for (int cur_tree = 0; cur_tree < trees.Length; cur_tree++)
            {
                //calc distance
                if (Calc_Distance(trees[cur_tree].position, future_position) < GlobalState.COLLISION_DISTANCE)
                    return true;
            }
            return false;
        }

        private bool Check_For_Zombie_Collision(Vector3 future_position, Zombie[] zombies)
        {
            for (int cur_zomb = 0; cur_zomb < zombies.Length; cur_zomb++)
            {
                if (zombies[cur_zomb].Killed)
                    continue;
                if (Calc_Distance (zombies[cur_zomb].position, future_position) < GlobalState.COLLISION_DISTANCE +1)
                    return true;
            }
            return false;
        }

    }

    class Camera
    {
        public Vector3 avatar_head_offset { get; set; }
        public Vector3 target_offset { get; set; }
        public Matrix view_matrix { get; set; }
        public Matrix projection_matrix { get; set; }

        public Camera()
        {
            avatar_head_offset = new Vector3(0, 7, -15);
            target_offset = new Vector3(0, 5, 0);
            view_matrix = Matrix.Identity;
            projection_matrix = Matrix.Identity;
        }

        public void Update(float avatar_yaw, Vector3 position, float aspect_ratio)
        {
            Matrix rotation_matrix = Matrix.CreateRotationY(avatar_yaw);

            Vector3 transformed_head_offset = Vector3.Transform(avatar_head_offset, rotation_matrix);
            Vector3 transformed_reference = Vector3.Transform(target_offset, rotation_matrix);

            Vector3 camera_position = position + transformed_head_offset;
            Vector3 camera_target = position + transformed_reference;

            //Calculates camera's view and projection matrices based on current values
            view_matrix = Matrix.CreateLookAt(camera_position, camera_target, Vector3.Up);
            projection_matrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(GlobalState.VIEW_ANGLE), aspect_ratio,
                GlobalState.NEAR_CLIP, GlobalState.FAR_CLIP);
            
        }
    }

    //GUN CLASS
    class Guns :GameObject
    {
       
        public bool Obtained { get; set; }
        public SoundEffect gunSoundEffect { get; set; }
        private int shooting;
        private Texture2D flash_texture;
        private Model flash_model;
        
        /*
        List<Vector4> billboardList = new List<Vector4>();
        VertexPositionTexture[] billboardVertices;

        private void AddBillboards()
        {
            billboardList.Add(new Vector4(0, 0, 0, 1));
        }
        
        private void CreateBBVertices(Vector3 avatar_pos, float avatar_yaw)
        {
            billboardVertices = new VertexPositionTexture[billboardList.Count * 6];

            int i = 0;
            foreach (Vector4 currentV4 in billboardList)
            {
                Vector3 center = new Vector3(currentV4.X, currentV4.Y, currentV4.Z);
                float scaling = currentV4.W;

                Matrix bbMatrix = Matrix.CreateBillboard(center, avatar_pos, new Vector3(0, 1, 0), new Vector3(Math.Cos(avatar_yaw), 0, Math.Sin(avatar_yaw)));

                Vector3 posDL = new Vector3(-0.5f, -0.5f, 0);
                Vector3 billboardedPosDL = Vector3.Transform(posDL * scaling, bbMatrix);
                billboardVertices[i++] = new VertexPositionTexture(billboardedPosDL, new Vector2(1, 1));

                VertexBuffer vb = new VertexBuffer(graphicsDevice, new VertexDeclaration(
            }



        }
        */
        public Guns()
            : base()
        {
            Obtained = false;
            shooting = 0;
            //AddBillboards();
            
                
        }

        public void LoadContent(ContentManager content, string model_name, string flash_model_name, string texture_name)
        {
            model = content.Load<Model>(model_name);
            flash_texture = content.Load<Texture2D>(texture_name);
            flash_model = content.Load<Model>(flash_model_name);
            //position = Vector3.Down;
        }

        public void Draw(Vector3 avatar_pos, float avatar_yaw, Matrix view, Matrix proj)
        {
            //CreateBBVertices(avatar_pos,avatar_yaw);

            if (shooting>0)
            {
                Random rand = new Random();

                foreach (ModelMesh mesh in flash_model.Meshes)
                {
                    foreach (BasicEffect be in mesh.Effects)
                    {
                        be.LightingEnabled = false;
                        be.PreferPerPixelLighting = false;
                        be.Projection = proj;
                        be.View = view;
                        be.World = Matrix.Identity * Matrix.CreateScale(0.25f) * Matrix.CreateRotationZ((float)(rand.NextDouble() * Math.PI)) * Matrix.CreateTranslation(0.0f, 4.0f, -4.0f) * Matrix.CreateRotationY(MathHelper.ToRadians(180) + avatar_yaw) * Matrix.CreateTranslation(avatar_pos.X, avatar_pos.Y + 0.5f, avatar_pos.Z);
                        be.Texture = flash_texture;
                        be.TextureEnabled = true;
                    }
                    mesh.Draw();
                }
            }


            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.Projection = proj;
                    be.View = view;
                    be.World = Matrix.Identity * Matrix.CreateScale(0.18f) * Matrix.CreateTranslation(0, 0, 1.0f) * Matrix.CreateRotationY(MathHelper.ToRadians(180)+avatar_yaw) *Matrix.CreateTranslation(avatar_pos.X, avatar_pos.Y+0.5f, avatar_pos.Z);
                
                }
                
                mesh.Draw();
            }

            if (shooting>0)
            {
                shooting--;
            }

            
           
        }

        public void shoot()
        {
            shooting= 2;
        }

        
              
    }

}

    public static class RayRenderer
    {
        static VertexPositionColor[] verts = new VertexPositionColor[2];

        static VertexPositionColor[] arrowVerts = {
            new VertexPositionColor(Vector3.Zero, Color.White),
            new VertexPositionColor(new Vector3(.5f, 0f, -.5f), Color.White),
            new VertexPositionColor(new Vector3(-.5f, 0f, -.5f), Color.White),
            new VertexPositionColor(new Vector3(0f, .5f, -.5f), Color.White),
            new VertexPositionColor(new Vector3(0f, -.5f, -.5f), Color.White),
        };

        static short[] arrowIndexs = {
            0, 1,
            0, 2,
            0, 3,
            0, 4,
        };

        //  static VertexDeclaration vertDecl;
        static BasicEffect effect;

        /// <summary>
        /// Renders a Ray for debugging purposes.
        /// </summary>
        /// <param name="ray">The ray to render.</param>
        /// <param name="length">The distance along the ray to render.</param>
        /// <param name="graphicsDevice">The graphics device to use when rendering.</param>
        /// <param name="view">The current view matrix.</param>
        /// <param name="projection">The current projection matrix.</param>
        /// <param name="color">The color to use drawing the ray.</param>
        public static void Render(Ray ray, float length, GraphicsDevice graphicsDevice, Matrix view, Matrix projection, Color color)
        {
            if (effect == null)
            {
                effect = new BasicEffect(graphicsDevice);
                effect.VertexColorEnabled = false;
                effect.LightingEnabled = false;
            }

            verts[0] = new VertexPositionColor(ray.Position, Color.White);
            verts[1] = new VertexPositionColor(ray.Position + (ray.Direction * length), Color.White);

            effect.DiffuseColor = color.ToVector3();
            effect.Alpha = (float)color.A / 255f;

            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = projection;

            //note you may wish to comment these next 2 lines out and set the RasterizerState elswehere in code 
            //rather than here for every ray draw call. 
            RasterizerState rs = graphicsDevice.RasterizerState;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts, 0, 1);

                effect.World = Matrix.Invert(Matrix.CreateLookAt(
                    verts[1].Position,
                    verts[0].Position,
                    (ray.Direction != Vector3.Up) ? Vector3.Up : Vector3.Left));
                
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, arrowVerts, 0, 5, arrowIndexs, 0, 4);
            }

            //note you may wish to comment the next line out and set the RasterizerState elswehere in code 
            //rather than here for every ray draw call. 
            graphicsDevice.RasterizerState = rs;
        }
    }


