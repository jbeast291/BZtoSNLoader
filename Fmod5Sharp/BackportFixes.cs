namespace Fmod5Sharp;

internal static class BackportFixes
{
    extension(byte[] byteArray)
    {
        internal byte[] Reverse()
        {
            System.MemoryExtensions.Reverse(byteArray);
            return byteArray;
        }

        internal byte[] ToArray() => byteArray;
    }
}