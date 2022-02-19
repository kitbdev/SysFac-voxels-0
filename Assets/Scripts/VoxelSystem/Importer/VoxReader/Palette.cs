using System.Text;
using VoxReader.Interfaces;

namespace VoxReader {
    public class Palette : IPalette {
        public Color[] Colors { get; }
        // 8 colors per row
        public Color[] GetColorsOnRow(int row) {
            if (row < 0 || row > Colors.Length) {
                return null;
            }
            const int NUM_COLORS_PER_ROW = 8;
            // const int NUM_ROWS = 32;
            Color[] rowcolors = new Color[NUM_COLORS_PER_ROW];
            for (int i = 0; i < NUM_COLORS_PER_ROW; i++) {
                rowcolors[i] = Colors[row * NUM_COLORS_PER_ROW + i];
            }
            return rowcolors;
        }

        public Palette(Color[] colors) {
            Colors = colors;
        }

        public override string ToString() {
            var output = new StringBuilder();

            for (int i = 0; i < Colors.Length - 1; i++) {
                output.AppendLine(GetText(i));
            }
            output.Append(GetText(Colors.Length - 1));

            string GetText(int index) => $"{index}: [{Colors[index]}]";

            return output.ToString();
        }
    }
}