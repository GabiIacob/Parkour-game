using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace Jump
{
    public class Block
    {
        public Vector3 Position;
        public Vector3 Size;
        public Block(Vector3 position, Vector3 size)
        {
            Position = position;
            Size = size;
        }
        public bool CheckCollision(Vector3 point)
        {
            return (point.X >= Position.X && point.X <= Position.X + Size.X) &&
                   (point.Y >= Position.Y && point.Y <= Position.Y + Size.Y) &&
                   (point.Z >= Position.Z && point.Z <= Position.Z + Size.Z);
        }
    }
}
