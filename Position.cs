namespace AdventOfCodeHelpers
{
    public enum EDirection : int
    {
        Unknown,
        North = 0x01,
        East = 0x02,
        South = 0x04,
        West = 0x08,

        NorthEast = 0x03,
        SouthEast = 0x06,
        SouthWest = 0x0C,
        NorthWest = 0x09
    };

    public class PositionTracker : Position
    {
        public List<Position> History { get; set; } = new List<Position>();

        public PositionTracker()
        {
        }

        public PositionTracker(int x, int y)
            : base(x, y)
        {
        }

        public PositionTracker(int x, int y, EDirection direction)
            : base(x, y, direction) 
        {
        }

        public override bool Move()
        {
            Position newPos = new Position(X, Y, Direction);
            newPos.Sum = Sum;
            newPos.Step = Step;
            newPos.LifeTime = History.Count;
            History.Add(newPos);
            return base.Move();
        }

        public PositionTracker Split(EDirection newDir)
        {
            PositionTracker pt = new PositionTracker(X, Y, newDir);
            pt.Sum = Sum + 1;
            pt.Step = 0;
            pt.History = new List<Position>(History); // Copy the history
            return pt;
        }

        public PositionTracker Split(Position newPos)
        {
            PositionTracker pt = new PositionTracker(newPos.X, newPos.Y, newPos.Direction);
            pt.Sum = Sum + 1;
            pt.Step = 0;
            pt.History = new List<Position>(History); // Copy the history
            pt.LifeTime = History.Count;
            pt.History.Add(new PositionTracker(this.X, this.Y, this.Direction));
            return pt;
        }

        public bool HasVisited(int x, int y)
        {
            return (History.Where(a => a.X == x && a.Y == y).Any());
        }

        public bool HasVisited(Position pos)
        {
            return (History.Where(a => a.X == pos.X && a.Y == pos.Y).Any());
        }

        public int DistanceSum 
        {
            get
            {
                return X + Y + (History.Count * 9 - Sum);
            }
        }

        public int WeightPerDirectionSwitch
        {
            get
            {
                if (History.Count > 0)
                {
                    int switches = 0;
                    EDirection dir = History[0].Direction;
                    for (int n = 1; n < History.Count; n++)
                    {
                        if (dir != History[n].Direction)
                        {
                            dir = History[n].Direction;
                            switches++;
                        }
                    }
                    return History.Count + switches * 1000;
                }
                return 0;
            }

        }

        public int GetMinX
        {
            get
            {
                return History.Min(a => a.X);
            }
        }

        public int GetMinY
        {
            get
            {
                return History.Min(a => a.Y);
            }
        }

        public int GetMaxX
        {
            get
            {
                return History.Max(a => a.X);
            }
        }

        public int GetMaxY
        {
            get
            {
                return History.Max(a => a.Y);
            }
        }
    }

    /// <summary>
    /// Tracker to be used in shortest path algo to track the length
    /// </summary>
    public class LengthTracker : Position
    {
        public int Length { get; set; }

        public LengthTracker()
        {
        }

        public LengthTracker(int x, int y)
            : base(x, y)
        {
            Length = 0;
        }

        public LengthTracker(int x, int y, EDirection direction)
            : base(x, y, direction)
        {
            Length = 0;
        }

        public override bool Move()
        {
            Length++;
            return base.Move();
        }

        public LengthTracker Split(EDirection newDir)
        {
            LengthTracker pt = new LengthTracker(X, Y, newDir);
            pt.Length = Length + 1;
            return pt;
        }

        public LengthTracker Split(Position newPos)
        {
            LengthTracker pt = new LengthTracker(newPos.X, newPos.Y, newPos.Direction);
            pt.Length = Length + 1;
            return pt;
        }
    }

    public class Position : IComparable<Position>
    {
        public Position()
        {
        }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
            Direction = EDirection.Unknown;
        }

        public Position(Position pos)
        {
            X = pos.X;
            Y = pos.Y;
            Direction = pos.Direction;
        }

        public Position(int x, int y, EDirection direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }

        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public EDirection Direction { get; set; } = EDirection.Unknown;
        public int Step { get; set; } = 0; // Step increases every time we move in the same direction
        public int Sum { get; set; } = 0;
        public int LifeTime { get; set; } = 0; // Lifetime of this position

        private static int s_minX = int.MinValue;
        private static int s_minY = int.MinValue;
        private static int s_maxX = int.MaxValue;
        private static int s_maxY = int.MaxValue;

        public static void SetMaxes(int maxX, int maxY)
        {
            s_maxX = maxX;
            s_maxY = maxY;
        }

        public static void SetMaxes(int minX, int maxX, int minY, int maxY)
        {
            s_minX = minX;
            s_minY = minY;
            s_maxX = maxX;
            s_maxY = maxY;
        }


        public EDirection GetDirection(Position previousPos)
        {
            int dx = X - previousPos.X;
            int dy = Y - previousPos.Y;
            if (dx > 0 && dy == 0) return EDirection.East;
            if (dx < 0 && dy == 0) return EDirection.West;
            if (dx == 0 && dy > 0) return EDirection.South;
            if (dx == 0 && dy < 0) return EDirection.North;
            return EDirection.North;
        }

        public Position PeekMovePosition()
        {
            return PeekMovePosition(this.Direction);
        }

        public Position PeekMovePosition(EDirection direction)
        {
            switch(direction) 
            { 
                case EDirection.North: return new Position(X, Math.Clamp(Y - 1, s_minY, s_maxY - 1), direction);
                case EDirection.East: return new Position(Math.Clamp(X + 1, s_minX, s_maxX - 1), Y, direction);
                case EDirection.South: return new Position(X, Math.Clamp(Y + 1, s_minY, s_maxY - 1), direction);
                case EDirection.West: return new Position(Math.Clamp(X - 1, s_minX, s_maxX - 1), Y, direction);
                default: return new Position(X, Y, EDirection.Unknown);
            }
        }

        public Position PeekRight()
        {
            EDirection direction = GetPerpendicularRight(this.Direction);
            return PeekMovePosition(direction);
        }

        public Position PeekLeft()
        {
            EDirection direction = GetPerpendicularLeft(this.Direction);
            return PeekMovePosition(direction);
        }

        public EDirection GetPerpendicularRight(EDirection direction)
        {
            EDirection dirReturn = EDirection.Unknown;
            switch (direction)
            {
                case EDirection.North: dirReturn = EDirection.East; break;
                case EDirection.East: dirReturn = EDirection.South; break;
                case EDirection.South: dirReturn = EDirection.West; break;
                case EDirection.West: dirReturn = EDirection.North; break;
                default:
                    break;
            }
            return dirReturn;
        }

        /// <summary>
        /// Convert a direction character to a direction enum value
        /// </summary>
        /// <param name="dirChar"></param>
        /// <returns></returns>
        static public EDirection CharToDirection(char dirChar)
        {
            switch (dirChar)
            {
                case '^': return EDirection.North;
                case '<': return EDirection.West;
                case '>': return EDirection.East;
                case 'v': return EDirection.South;
                default:
                    break;
            }
            return EDirection.Unknown;
        }

        public EDirection GetPerpendicularLeft(EDirection direction)
        {
            EDirection dirReturn = EDirection.Unknown;
            switch (direction)
            {
                case EDirection.North: dirReturn = EDirection.West; break;
                case EDirection.West: dirReturn = EDirection.South; break;
                case EDirection.South: dirReturn = EDirection.East; break;
                case EDirection.East: dirReturn = EDirection.North; break;
                default:
                    break;
            }
            return dirReturn;
        }

        public void RotateRight()
        {
            Direction = GetPerpendicularRight(Direction);
            Step = 0;
        }

        public void RotateLeft()
        {
            Direction = GetPerpendicularLeft(Direction);
            Step = 0;
        }

        public EDirection GetOppositeDirection()
        {
            switch (Direction)
            {
                case EDirection.North: return EDirection.South; 
                case EDirection.East: return EDirection.West;
                case EDirection.South: return EDirection.North; 
                case EDirection.West: return EDirection.East; 
                default:
                    break;
            }
            return EDirection.Unknown;
        }

        public virtual bool Move()
        {
            return Move(this.Direction);
        }

        /// <summary>
        /// Move 1 step to a new direction
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public virtual bool Move(EDirection dir)
        {
            switch (dir)
            {
                case EDirection.North:
                    if (Y <= s_minY)
                        Direction = EDirection.Unknown;
                    else
                    {
                        Y--;
                        Step++;
                    }
                    break;
                case EDirection.East:
                    if (X >= s_maxX - 1)
                        Direction = EDirection.Unknown;
                    else
                    {
                        X++;
                        Step++;
                    }
                    break;
                case EDirection.South:
                    if (Y >= s_maxY - 1)
                        Direction = EDirection.Unknown;
                    else
                    {
                        Y++;
                        Step++;
                    }
                    break;
                case EDirection.West:
                    if (X <= s_minX)
                        Direction = EDirection.Unknown;
                    else
                    { 
                        X--;
                        Step++;
                    }
                    break;
                default:
                    break;
            }
            return (Direction != EDirection.Unknown);
        }

        public override string ToString()
        {
            if (Sum > 0)
                return $"[{X}, {Y} - {Direction} : {Sum}]";
            else if (Direction != EDirection.Unknown)
                return $"[{X}, {Y} - {Direction}]";
            return $"[{X}, {Y}]";
        }

        public static bool operator ==(Position lhs, Position rhs)
        {
            return (lhs.X == rhs.X && lhs.Y == rhs.Y);
        }
        public static bool operator !=(Position lhs, Position rhs) => !(lhs == rhs);

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override bool Equals(object? obj)
        {
            return obj is Position position && X == position.X && Y == position.Y;
        }

        public int CompareTo(Position? other)
        {
            if (X < other?.X) return -1;
            else if (X == other?.X && Y < other?.Y) return -1;
            else if (X == other?.X && Y == other?.Y && Sum < other?.Sum) return -1;
            else if (X == other?.X && Y == other?.Y && Sum == other?.Sum) return 0;
            return 1;
        }
    }
}
