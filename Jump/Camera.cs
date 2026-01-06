using OpenTK.Mathematics;
using System;

namespace Jump
{
    public class Camera
    {
        public Vector3 Position;

        public Vector3 Front = -Vector3.UnitZ;
        public Vector3 Up = Vector3.UnitY;
        public Vector3 Right = Vector3.UnitX;

        // Unghiurile de rotație
        public float Pitch;          // sus / jos
        public float Yaw = -90.0f;    // stanga / dreapta

        // Constructor
        public Camera(Vector3 position)
        {
            Position = position;
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(
                Position,              
                Position + Front,      
                Up                     
            );
        }

        public void UpdateVectors()
        {
            Pitch = Math.Clamp(Pitch, -89f, 89f);

            float pitchRad = MathHelper.DegreesToRadians(Pitch);
            float yawRad = MathHelper.DegreesToRadians(Yaw);

            Vector3 newFront;
            newFront.X = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
            newFront.Y = MathF.Sin(pitchRad);
            newFront.Z = MathF.Cos(pitchRad) * MathF.Sin(yawRad);

            Front = Vector3.Normalize(newFront);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
