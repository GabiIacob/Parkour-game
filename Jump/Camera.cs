using OpenTK.Mathematics;
using System;

namespace Jump
{
    public class Camera
    {
        // Poziția camerei în lume
        public Vector3 Position;

        // Direcțiile camerei
        public Vector3 Front = -Vector3.UnitZ;
        public Vector3 Up = Vector3.UnitY;
        public Vector3 Right = Vector3.UnitX;

        // Unghiurile de rotație
        public float Pitch;          // sus / jos
        public float Yaw = -90.0f;    // stânga / dreapta

        // Constructor
        public Camera(Vector3 position)
        {
            Position = position;
        }

        // Matricea de view (unde se uită camera)
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(
                Position,              // poziția camerei
                Position + Front,      // direcția în care se uită
                Up                     // direcția "sus"
            );
        }

        // Recalculează direcțiile camerei după ce Pitch / Yaw se schimbă
        public void UpdateVectors()
        {
            // Limităm Pitch ca să evităm răsturnarea camerei
            Pitch = Math.Clamp(Pitch, -89f, 89f);

            // Convertim unghiurile din grade în radiani
            float pitchRad = MathHelper.DegreesToRadians(Pitch);
            float yawRad = MathHelper.DegreesToRadians(Yaw);

            // Calculăm direcția în față (Front)
            Vector3 newFront;
            newFront.X = MathF.Cos(pitchRad) * MathF.Cos(yawRad);
            newFront.Y = MathF.Sin(pitchRad);
            newFront.Z = MathF.Cos(pitchRad) * MathF.Sin(yawRad);

            // Normalizăm vectorul
            Front = Vector3.Normalize(newFront);

            // Calculăm direcția dreapta și sus
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}
