using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlantSolver
{
    public class Solver
    {
        const byte EMPTY = 255;
        const byte ZERO = 0;
        const byte ONE = 1;
        const byte TWO = 2;
        const byte THREE = 3;

        private (uint, uint) size; // (x, y)
        private string puzzleString;

        private List<bool> solution;
        // no byte may store a value larger than 3
        private byte[] puzzle;

        public Solver((uint, uint) size, string puzzle) {
            this.size = size;
            this.puzzleString = puzzle;
            this.ParsePuzzle(size, puzzle);
        }

        // The seemingly widely accepted format for storing slant puzzles is to store a matrix of each crosshair.
        // From left to right, each crosshair number is separated by the number of spaces indicated by the value of the letter in base 27 (probably)
        // For example, no letter indicates that numbers are consecutive, while a represents 1 space, and b represents 2 spaces, etc.
        private void ParsePuzzle((uint, uint) size, string puzzleString) {
            this.puzzle = new byte[(size.Item1+1) * (size.Item2+1)];
            Utils.Populate(this.puzzle, EMPTY);

            if (size.Item1 > 25 || size.Item2 > 25) {
                throw new Exception(
                    "puzzles of size larger than 25 are not supported becuase " +
                    "idk how the format is specified exactly when the number of " +
                    "spaces between two numbers exceeds 27 (all letters)"
                );
            } else {
                uint i = 0;
                foreach (char ch in puzzleString) {
                    if (Char.IsDigit(ch)) {
                        uint value = (uint)ch - (uint)'0';
                        this.puzzle[i] = (byte)value;
                        i += 1;
                    } else {
                        // case: ch must be a letter
                        uint spaces = (uint)ch - (uint)'a' + 1;
                        i += spaces;
                    }
                }
            }
        }

        public void PrintPuzzle() {
            Console.WriteLine("The current puzzle: ");
            Console.WriteLine(this.puzzleString);
            for (int y = 0; y < size.Item2 + 1; y++) {
                for (int x = 0; x < size.Item1 + 1; x++) {
                    var curr = this.puzzle[y * (size.Item2 + 1) + x];
                    if (curr != EMPTY) { 
                        Console.Write(curr.ToString());
                        if (x != size.Item1) Console.Write("---");
                    } else { 
                        Console.Write(' ');
                        if (x != size.Item1) Console.Write("---");
                    }
                }

                Console.WriteLine("");

                if (y != size.Item2) {
                    for (int x = 0; x < size.Item1 + 1; x++)
                    {
                        Console.Write('|');
                        if (x != size.Item1) Console.Write("   ");
                    }

                    Console.WriteLine("");
                }
            }
        }

        public void SolvePuzzle() { 
        
        }
    }
}
