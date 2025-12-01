namespace AoC2024
{
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Vector2Int other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2Int other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }

        public Vector2Int Normalize()
        {
            // return new Vector2Int((int) (x / Magnitude()), (int) (y / Magnitude()));
            return new Vector2Int(x == 0 ? 0 : x / x, y == 0 ? 0 : y / y);
        }

        public double Magnitude()
        {
            return Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2));
        }

        public int Max() => Math.Max(x, y);
        public int AbsMax() => Math.Max(Math.Abs(x), Math.Abs(y));
        public static Vector2Int operator +(Vector2Int first, Vector2Int second) => new Vector2Int(first.x + second.x, first.y + second.y);
        public static Vector2Int operator *(int first, Vector2Int second) => new Vector2Int(first * second.x, first * second.y);
        public static Vector2Int operator /(Vector2Int first, int second) => new Vector2Int(first.x / second, first.y / second);
        public static Vector2Int operator -(Vector2Int first, Vector2Int second) => new Vector2Int(first.x - second.x, first.y - second.y);
        public static bool operator ==(Vector2Int first, Vector2Int second) => first.x == second.x && first.y == second.y;
        public static bool operator !=(Vector2Int first, Vector2Int second) => !(first == second);
        public override string ToString()
        {
            return $"({x},{y})";
        }
    }
}