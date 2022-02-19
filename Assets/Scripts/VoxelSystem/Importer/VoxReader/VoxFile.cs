using System.Linq;
using VoxReader.Interfaces;

namespace VoxReader
{
    internal class VoxFile : IVoxFile
    {
        public int VersionNumber { get; }
        public IModel[] Models { get; }
        public IPalette Palette { get; }
        public IChunk[] Chunks { get; }

        internal VoxFile(int versionNumber, IModel[] models, IPalette palette, IChunk[] chunks)
        {
            VersionNumber = versionNumber;
            Models = models;
            Palette = palette;
            Chunks = chunks;
        }

        public T GetChunk<T>() where T: class, IChunk{
            return Chunks.FirstOrDefault(c => c is T) as T;
        }
    }
}