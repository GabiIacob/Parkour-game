using OpenTK.Mathematics;

namespace Jump
{
    public class Transform
    {
        

        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero; 
        public Vector3 Scale = Vector3.One;

        
        public void Translate(Vector3 delta)
        {
            Position += delta;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

       
        public void Rotate(Vector3 delta)
        {
            Rotation += delta;
        }

        public void SetRotation(Vector3 rotation)
        {
            Rotation = rotation;
        }

        public void ScaleBy(Vector3 factor)
        {
            Scale *= factor;
        }

        public void SetScale(Vector3 scale)
        {
            Scale = scale;
        }

       
        public Matrix4 GetModelMatrix()
        {
            Matrix4 scaleMatrix = Matrix4.CreateScale(Scale);

            Matrix4 rotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X));
            Matrix4 rotY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y));
            Matrix4 rotZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));

            Matrix4 rotationMatrix = rotX * rotY * rotZ;

            Matrix4 translationMatrix = Matrix4.CreateTranslation(Position);

            return scaleMatrix * rotationMatrix * translationMatrix;
        }
    }
}
