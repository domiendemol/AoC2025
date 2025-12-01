namespace AoC2024
{
    public struct Vector2Long
    {
        public long x;
        public long y;

        public Vector2Long(long x, long y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Vector2Long other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2Long other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)((x * 397) ^ y);
            }
        }

        public Vector2Long Normalize()
        {
            return new Vector2Long(x == 0 ? 0 : x / x, y == 0 ? 0 : y / y);
        }

        public double Magnitude()
        {
            return Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2));
        }
        
        public static Vector2Long operator +(Vector2Long first, Vector2Long second) => new Vector2Long(first.x + second.x, first.y + second.y);
        public static Vector2Long operator *(long first, Vector2Long second) => new Vector2Long(first * second.x, first * second.y);
        public static Vector2Long operator -(Vector2Long first, Vector2Long second) => new Vector2Long(first.x - second.x, first.y - second.y);
        public static bool operator ==(Vector2Long first, Vector2Long second) => first.x == second.x && first.y == second.y;
        public static bool operator !=(Vector2Long first, Vector2Long second) => !(first == second);
        public override string ToString()
        {
            return $"({x},{y})";
        }
    }
}