// A hash combiner that is implemented with the Fowler/Noll/Vo algorithm (FNV-1a). This is a mutable struct for performance reasons.
// Taken from https://gist.github.com/StephenCleary/4f6568e5ab5bee7845943fdaef8426d2

namespace Tests
{
    public struct FnvHash32
    {
        public const uint Offset = 2166136261;
        private const uint Prime = 16777619;

        public static uint GetHashCodeFast(string text)
        {
            uint hash = Offset;

            unchecked
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    hash = (hash ^ ch) * Prime;
                }
            }

            return hash;
        }

        public static uint GetHashCode(string text)
        {
            uint hash = Offset;

            unchecked
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    byte b = (byte)ch;
                    hash ^= b;
                    hash *= Prime;

                    b = (byte)(ch >> 8);
                    hash ^= b;
                    hash *= Prime;
                }
            }

            return hash;
        }
    }

    public struct FnvHash64
    {
        public const ulong Offset = 14695981039346656037;
        private const ulong Prime = 1099511628211;

        public static ulong GetHashCodeFast(string text)
        {
            ulong hash = Offset;

            unchecked
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    hash = (hash ^ ch) * Prime;
                }
            }

            return hash;
        }

        public static ulong GetHashCode(string text)
        {
            ulong hash = Offset;

            unchecked
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];
                    byte b = (byte)ch;
                    hash ^= b;
                    hash *= Prime;

                    b = (byte)(ch >> 8);
                    hash ^= b;
                    hash *= Prime;
                }
            }

            return hash;
        }
    }

    /// <summary>
    /// A hash combiner that is implemented with the Fowler/Noll/Vo algorithm (FNV-1a). This is a mutable struct for performance reasons.
    /// Taken from https://gist.github.com/StephenCleary/4f6568e5ab5bee7845943fdaef8426d2
    /// </summary>
    public struct FnvHash64Old
    {
        /// <summary>
        /// The starting point of the FNV hash.
        /// </summary>
        public const ulong Offset = 14695981039346656037;

        /// <summary>
        /// The prime number used to compute the FNV hash.
        /// </summary>
        private const ulong Prime = 1099511628211;

        /// <summary>
        /// Gets the current result of the hash function.
        /// </summary>
        public ulong HashCode { get; private set; }

        public FnvHash64Old(ulong hash)
        {
            HashCode = hash;
        }

        /// <summary>
        /// Creates a new FNV hash initialized to <see cref="Offset"/>.
        /// </summary>
        public static FnvHash64Old Create()
        {
            var result = new FnvHash64Old();
            result.HashCode = Offset;
            return result;
        }

        public static ulong GetHashCode(string text)
        {
            var hash = Create();

            for (int i = 0; i < text.Length; i++)
            {
                hash.Combine(text[i]);
            }

            return hash.HashCode;
        }

        public static ulong Combine(ulong left, ulong right)
        {
            var fnv = new FnvHash64Old(left);
            fnv.Combine(right);
            return fnv.HashCode;
        }

        /// <summary>
        /// Adds the specified byte to the hash.
        /// </summary>
        /// <param name="data">The byte to hash.</param>
        public void Combine(byte data)
        {
            unchecked
            {
                HashCode ^= data;
                HashCode *= Prime;
            }
        }

        public void Combine(char data)
        {
            unchecked
            {
                var sh = (ushort)data;
                Combine(unchecked((byte)sh));
                Combine(unchecked((byte)(sh >> 8)));
            }
        }

        /// <summary>
        /// Adds the specified integer to this hash, in little-endian order.
        /// </summary>
        /// <param name="data">The integer to hash.</param>
        public void Combine(int data)
        {
            Combine(unchecked((byte)data));
            Combine(unchecked((byte)(data >> 8)));
            Combine(unchecked((byte)(data >> 16)));
            Combine(unchecked((byte)(data >> 24)));
        }

        public void Combine(ulong data)
        {
            Combine(unchecked((byte)data));
            Combine(unchecked((byte)(data >> 8)));
            Combine(unchecked((byte)(data >> 16)));
            Combine(unchecked((byte)(data >> 24)));
            Combine(unchecked((byte)(data >> 32)));
            Combine(unchecked((byte)(data >> 40)));
            Combine(unchecked((byte)(data >> 48)));
            Combine(unchecked((byte)(data >> 56)));
        }
    }
}