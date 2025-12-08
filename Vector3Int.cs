namespace AoC2025
{
	public struct Vector3Int
	{
		public int x;
		public int y;
		public int z;

		public Vector3Int(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public bool Equals(Vector3Int other)
		{
			return x == other.x && y == other.y && z == other.z;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector3Int other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (x * 397) ^ y + z;
			}
		}

		public Vector3Int Normalize()
		{
			// return new Vector3Int((int) (x / Magnitude()), (int) (y / Magnitude()));
			return new Vector3Int(x == 0 ? 0 : x / x, y == 0 ? 0 : y / y,  z == 0 ? 0 : z / z);
		}

		public double Magnitude()
		{
			return Math.Sqrt(Math.Pow(x,2) + Math.Pow(y,2) + Math.Pow(z,2));
		}

		public int Max() => Math.Max(Math.Max(x, y), z);
		public int AbsMax() => Math.Max(Math.Max(Math.Abs(x), Math.Abs(y)), Math.Abs(z));
		public static Vector3Int operator +(Vector3Int first, Vector3Int second) => new Vector3Int(first.x + second.x, first.y + second.y, first.z + second.z);
		public static Vector3Int operator *(int first, Vector3Int second) => new Vector3Int(first * second.x, first * second.y, first * second.z);
		public static Vector3Int operator /(Vector3Int first, int second) => new Vector3Int(first.x / second, first.y / second, first.z / second);
		public static Vector3Int operator -(Vector3Int first, Vector3Int second) => new Vector3Int(first.x - second.x, first.y - second.y, first.z - second.z);
		public static bool operator ==(Vector3Int first, Vector3Int second) => first.x == second.x && first.y == second.y;
		public static bool operator !=(Vector3Int first, Vector3Int second) => !(first == second);
		public override string ToString()
		{
			return $"({x},{y},{z})";
		}
	}
}