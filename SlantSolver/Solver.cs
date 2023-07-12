using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlantSolver
{
    public class Solver
    {
        const byte NONE = 255;
        const byte ZERO = 0;
        const byte ONE = 1;
        const byte TWO = 2;
        const byte THREE = 3;
        const byte FOUR = 4;

        public const byte EMPTY = 0;
        public const byte TOPLEFT = 1;
        public const byte TOPRIGHT = 2;

        // TODO: convert into a Size(.width=uint, .height=uint) class
        private (uint, uint) size; // (x, y)
        private string puzzleString;
        
        // TODO: use enums
        private byte[] grid;
        private byte[] vertices; // size + (1,1)

        public Solver((uint, uint) size, string puzzle) 
        {
            this.size = size;
            this.puzzleString = puzzle;

            this.grid = new byte[size.Item1 * size.Item2];
            Utils.Populate(this.grid, EMPTY);

            this.vertices = new byte[(size.Item1 + 1) * (size.Item2 + 1)];
            Utils.Populate(this.vertices, NONE);

            this.ParsePuzzle(size, puzzle);
        }

        public void PrintPuzzle() {
            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine("The current puzzle: ");
            Console.WriteLine(this.puzzleString);
            for (int y = 0; y < size.Item2 + 1; y++) {
                for (int x = 0; x < size.Item1 + 1; x++) {
                    var curr = this.vertices[y * (size.Item2 + 1) + x];
                    if (curr != NONE) { 
                        Console.Write(curr.ToString());
                        if (x != size.Item1) Console.Write("---");
                    } else {
                        // TODO: display diagonals
                        Console.Write(' ');
                        if (x != size.Item1) Console.Write("---");
                    }
                }

                Console.WriteLine("");

                if (y != size.Item2) {
                    for (int x = 0; x < size.Item1 + 1; x++)
                    {
                        Console.Write('|');
                        if (x != size.Item1)
                        {                   
                            byte value = grid[y * GridWidth() + x];
                            string token = value == EMPTY ? " " : (value == TOPLEFT ? "╲" : "╱");
                            Console.Write(" " + token + " ");
                        }
                    }

                    Console.WriteLine("");
                }
            }
        }

        public void SolvePuzzle() 
        { 
        
        }

        // used for testing
        public void SetGrid(byte[] grid) 
        {
            if (grid.Length != size.Item1 * size.Item2) 
                throw new Exception("invalid grid size");

            this.grid = grid;
        }

        // Determine if ths current puzzle is solved or not
        public bool IsSolved()
        {
            // no empty cells
            foreach (byte cell in grid)
                if (cell == EMPTY) 
                    return false;

            return SatisfiesRules();
        }

        // ------------------------ //

        private uint GridHeight()
        {
            return size.Item2;
        }
        private uint GridWidth()
        {
            return size.Item1;
        }

        private uint VertexHeight() 
        {
            return size.Item2 + 1;
        }
        private uint VertexWidth()
        {
            return size.Item1 + 1;
        }

        // The seemingly widely accepted format for storing slant puzzles is to store a matrix of each crosshair.
        // From left to right, each crosshair number is separated by the number of spaces indicated by the value of the letter in base 27 (probably)
        // For example, no letter indicates that numbers are consecutive, while a represents 1 space, and b represents 2 spaces, etc.
        private void ParsePuzzle((uint, uint) size, string puzzleString)
        {
            if (size.Item1 > 25 || size.Item2 > 25)
            {
                throw new Exception(
                    "puzzles of size larger than 25 are not supported becuase " +
                    "idk how the format is specified exactly when the number of " +
                    "spaces between two numbers exceeds 27 (all letters)"
                );
            }
            else
            {
                uint i = 0;
                foreach (char ch in puzzleString)
                {
                    if (Char.IsDigit(ch))
                    {
                        uint value = (uint)ch - (uint)'0';
                        this.vertices[i] = (byte)value;
                        i += 1;
                    }
                    else
                    {
                        // case: ch must be a letter
                        uint spaces = (uint)ch - (uint)'a' + 1;
                        i += spaces;
                    }
                }
            }
        }

        // x, y are vertex coordinates 
        private uint IncomingLines(uint x, uint y) {
            uint numIncoming = 0;

            // case: top left square exists    
            if (y > 0 && x > 0)
                numIncoming += grid[(y-1) * GridWidth() + (x-1)] == TOPLEFT ? 1u : 0u;

            // case: top right square exists
            if (y > 0 && x < VertexWidth() - 1)
                numIncoming += grid[(y-1) * GridWidth() + x] == TOPRIGHT ? 1u : 0u;
             
            // case: bottom left square exists    
            if (y < VertexHeight() - 1 && x > 0)
                numIncoming += grid[y * GridWidth() + (x - 1)] == TOPRIGHT ? 1u : 0u;

            // case: bottom right square exists
            if (y < VertexHeight() - 1 && x < VertexWidth() - 1)
                numIncoming += grid[y * GridWidth() + x] == TOPLEFT ? 1u : 0u;

            return numIncoming;
        }

        // determine if the current puzzle contains a loop of some kind
        private bool ContainsLoop() {
            // TODO: this
            return true;
        }
        
        // returns true if all rules are satisfied
        private bool SatisfiesRules() {
            for (uint y = 0; y < VertexHeight(); y++)
            {
                for (uint x = 0; x < VertexWidth(); x++)
                {
                    byte circle = this.vertices[y * VertexWidth() + x];
                    if (circle != NONE)
                    {
                        if (IncomingLines(x, y) != circle)
                            return false;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return ContainsLoop() == false;
        }
    }
}
