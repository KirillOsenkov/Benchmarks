namespace Tests
{
    public class Djb2
    {
        public static int GetHashCode(string text)
        {
            int hashCode = 5381;

            foreach (char ch in text)
            {
                unchecked
                {
                    hashCode = hashCode * 33 ^ ch;
                }
            }

            return hashCode;
        }
    }
}