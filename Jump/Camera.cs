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
        public float Pitch;
        public float Yaw = -90.0f;

        public Camera(Vector3 position) { Position = position; }
        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Front, Up);

        public void UpdateVectors()
        {
            Pitch = Math.Clamp(Pitch, -89f, 89f);
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(Pitch)) * MathF.Cos(MathHelper.DegreesToRadians(Yaw));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(Pitch));
            front.Z = MathF.Cos(MathHelper.DegreesToRadians(Pitch)) * MathF.Sin(MathHelper.DegreesToRadians(Yaw));
            Front = Vector3.Normalize(front);
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}