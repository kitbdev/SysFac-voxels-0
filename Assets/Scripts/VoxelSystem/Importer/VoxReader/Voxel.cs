namespace VoxReader
{
    public readonly struct Voxel
    {
        /// <summary>
        /// The position of the voxel.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// The color of the voxel.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// The color index of the voxel.
        /// </summary>
        public int ColorIndex { get; }

        internal Voxel(Vector3 position, Color color,int colorIndex)
        {
            Position = position;
            Color = color;
            ColorIndex = colorIndex;
        }

        public override string ToString()
        {
            return $"Position: [{Position}], Color: [{Color}]";
        }
    }
}