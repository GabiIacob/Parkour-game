using OpenTK.Mathematics;

namespace Jump
{
   
    public class MovingPlatform
    {
        public int BlockIndex { get; set; }

        public Vector3 StartPosition { get; set; }

        public float MoveRange { get; set; }

        public float Speed { get; set; }

        public float CurrentOffset { get; private set; }

        public int Direction { get; private set; }

        public int PhysicalBlockIndex { get; set; }

        public MovingPlatform(int blockIndex, Vector3 startPosition, float moveRange, float speed, int physicalBlockIndex)
        {
            BlockIndex = blockIndex;
            StartPosition = startPosition;
            MoveRange = moveRange;
            Speed = speed;
            PhysicalBlockIndex = physicalBlockIndex;
            CurrentOffset = 0f;
            Direction = 1; 
        }

        public void Update(float deltaTime)
        {
            CurrentOffset += Direction * Speed * deltaTime;

            if (CurrentOffset >= MoveRange)
            {
                CurrentOffset = MoveRange;
                Direction = -1;
            }
            else if (CurrentOffset <= -MoveRange)
            {
                CurrentOffset = -MoveRange;
                Direction = 1; 
            }
        }

       
        public Vector3 GetCurrentPosition()
        {
            return StartPosition + new Vector3(CurrentOffset, 0, 0);
        }

       
        public void Reset()
        {
            CurrentOffset = 0f;
            Direction = 1;
        }
    }
}