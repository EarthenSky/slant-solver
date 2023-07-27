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

        // TODO: do DFS, but cache nodes
        // determine if the current puzzle contains a loop of some kind
        private bool ContainsLoop() 
        {
            // contains true if this tile has been part of a tree / cycle that has been checked.
            bool[] gridCache = new bool[GridWidth() * GridHeight()];
            Utils.Populate(gridCache, false);

            for (uint y = 0; y < GridHeight(); y++) 
            {
                for (uint x = 0; x < GridWidth(); x++)
                {
                    bool hasBeenChecked = gridCache[x + y * GridWidth()];
                    if (!hasBeenChecked && this.HasCycleDFS(gridCache, (x, y)))
                        return true;
                }
            }

            return false;
        }

        private bool TileAboveExists((uint, uint) tile) => tile.Item2 != 0;
        private bool TileLeftExists((uint, uint) tile) => tile.Item1 != 0;
        private bool TileRightExists((uint, uint) tile) => tile.Item1 != GridWidth() - 1;
        private bool TileBelowExists((uint, uint) tile) => tile.Item2 != GridHeight() - 1;

        private uint GridIndexOf((uint, uint) tile) => tile.Item1 + tile.Item2 * GridWidth();
         
        /// this function determines the kind of tile given the start & end points of a diagonal
        private byte TileKind((uint, uint) first, (uint, uint) second) => grid[GridIndexOf(TilePosOf(first, second))];

        /// This function takes a diagonal & turns it into the 'position' of the Tile the diagonal resides in
        private (uint, uint) TilePosOf((uint, uint) first, (uint, uint) second)
        {
            if (first.Item1 > second.Item1 && first.Item2 > second.Item2)
            {
                return (second.Item1, second.Item2);
            }
            else if (first.Item1 < second.Item1 && first.Item2 < second.Item2)
            {
                return (first.Item1, first.Item2);
            }
            else if (first.Item1 > second.Item1 && first.Item2 < second.Item2)
            {
                return ((first.Item1 - 1), first.Item2);
            }
            else if (first.Item1 < second.Item1 && first.Item2 > second.Item2)
            {
                return ((second.Item1 - 1), second.Item2);
            }
            else
            {
                throw new Exception("first and second do not form a diagonal: " + first.ToString() + " & " + second.ToString());
            }
        }

        /// This function takes the the start & end points of a diagonal, and determines if any other 
        /// tiles are connected & unvisisted, adding them to the stack if so.
        /// 
        /// The return value represents whether a cycle has been detected or not
        private bool CheckAdjacent(
            List<((uint, uint), (uint, uint))> stack, 
            List<(uint, uint)> visited, 
            (uint, uint) curr, 
            (uint, uint) next
        ) {
            // NOTE: there is probably some way to generalize this code better, but the fact that it's
            // partially unrolled should count for a bit of performance!

            // add adjacent tiles if possible
            bool isNextBelow = next.Item2 > curr.Item2;
            byte currentTileKind = TileKind(curr, next);
            var tilePos = TilePosOf(curr, next);
            if (currentTileKind == TOPLEFT && isNextBelow)
            {
                var below = (next.Item1 - 1, next.Item2);
                var belowNext = (next.Item1 - 1, next.Item2 + 1);
                if (TileBelowExists(tilePos) && grid[GridIndexOf(below)] == TOPRIGHT)
                    if (visited.Contains(below))
                        return true;
                    else
                        stack.Add((next, belowNext));

                var right = (next.Item1, next.Item2 - 1);
                var rightNext = (next.Item1 + 1, next.Item2 - 1);
                if (TileRightExists(tilePos) && grid[GridIndexOf(right)] == TOPRIGHT)
                    if (visited.Contains(right))
                        return true;
                    else
                        stack.Add((next, rightNext));

                var belowRight = (next.Item1, next.Item2);
                var belowRightNext = (next.Item1 + 1, next.Item2 + 1);
                if (TileBelowExists(tilePos) && TileRightExists(tilePos) && grid[GridIndexOf(belowRight)] == TOPLEFT)
                    if (visited.Contains(belowRight))
                        return true;
                    else
                        stack.Add((next, belowRightNext));
            }
            else if (currentTileKind == TOPLEFT && !isNextBelow)
            {
                var above = (next.Item1, next.Item2 - 1);
                var aboveNext = (next.Item1 + 1, next.Item2 - 1);
                if (TileAboveExists(tilePos) && grid[GridIndexOf(above)] == TOPRIGHT)
                    if (visited.Contains(above))
                        return true;
                    else
                        stack.Add((next, aboveNext));

                var left = (next.Item1 - 1, next.Item2);
                var leftNext = (next.Item1 - 1, next.Item2 + 1);
                if (TileLeftExists(tilePos) && grid[GridIndexOf(left)] == TOPRIGHT)
                    if (visited.Contains(left))
                        return true;
                    else
                        stack.Add((next, leftNext));

                var aboveLeft = (next.Item1 - 1, next.Item2 - 1);
                var aboveLeftNext = (next.Item1 - 1, next.Item2 - 1);
                if (TileAboveExists(tilePos) && TileLeftExists(tilePos) && grid[GridIndexOf(aboveLeft)] == TOPLEFT)
                    if (visited.Contains(aboveLeft))
                        return true;
                    else
                        stack.Add((next, aboveLeftNext));
            }
            else if (currentTileKind == TOPRIGHT && isNextBelow)
            {
                var below = (next.Item1, next.Item2);
                var belowNext = (next.Item1 + 1, next.Item2 + 1);
                if (TileBelowExists(tilePos) && grid[GridIndexOf(below)] == TOPLEFT)
                    if (visited.Contains(below))
                        return true;
                    else
                        stack.Add((next, belowNext));

                var left = (next.Item1 - 1, next.Item2 - 1);
                var leftNext = (next.Item1 - 1, next.Item2 - 1);
                if (TileLeftExists(tilePos) && grid[GridIndexOf(left)] == TOPLEFT)
                    if (visited.Contains(left))
                        return true;
                    else
                        stack.Add((next, leftNext));

                var belowLeft = (next.Item1 - 1, next.Item2);
                var belowLeftNext = (next.Item1 - 1, next.Item2 + 1);
                if (TileBelowExists(tilePos) && TileLeftExists(tilePos) && grid[GridIndexOf(belowLeft)] == TOPRIGHT)
                    if (visited.Contains(belowLeft))
                        return true;
                    else
                        stack.Add((next, belowLeftNext));
            }
            else if (currentTileKind == TOPRIGHT && !isNextBelow)
            {
                var above = (next.Item1 - 1, next.Item2 - 1);
                var aboveNext = (next.Item1 - 1, next.Item2 - 1);
                if (TileAboveExists(tilePos) && grid[GridIndexOf(above)] == TOPLEFT)
                    if (visited.Contains(above))
                        return true;
                    else
                        stack.Add((next, aboveNext));

                var right = (next.Item1, next.Item2);
                var rightNext = (next.Item1 + 1, next.Item2 + 1);
                if (TileRightExists(tilePos) && grid[GridIndexOf(right)] == TOPLEFT)
                    if (visited.Contains(right))
                        return true;
                    else
                        stack.Add((next, rightNext));

                var aboveRight = (next.Item1, next.Item2 - 1);
                var aboveRightNext = (next.Item1 + 1, next.Item2 - 1);
                if (TileAboveExists(tilePos) && TileRightExists(tilePos) && grid[GridIndexOf(aboveRight)] == TOPRIGHT)
                    if (visited.Contains(aboveRight))
                        return true;
                    else
                        stack.Add((next, aboveRightNext));
            }

            return false;
        }

        private bool HasCycleDFS(bool[] gridCache, (uint, uint) start) 
        {
            bool[] touched = new bool[GridWidth() * GridHeight()];
            Utils.Populate(touched, false);

            // TODO: turn visited into a map
            List<(uint, uint)> visited = new List<(uint, uint)>();
            List<((uint, uint), (uint, uint))> stack = new List<((uint, uint), (uint, uint))>();

            // evaluate starting diagonal in both directions
            {
                // figure out the locations of the starting verticies
                (uint, uint) startNext;
                if (grid[GridIndexOf(start)] == TOPRIGHT) {
                    start = (start.Item1 + 1, start.Item2); // TODO: after turing start into a Vec2 class, add a method to move right one, like `(new Vec2 (10, 10)).Right()`
                    startNext = (start.Item1 - 1, start.Item2 + 1); // TODO: create a NextOf() function, that takes in a Vec2 base & a Vec2 tilePos?
                } else {
                    startNext = (start.Item1 + 1, start.Item2 + 1);
                }

                var tilePos = TilePosOf(start, startNext);
                visited.Add(tilePos);
                gridCache[GridIndexOf(tilePos)] = true;

                CheckAdjacent(stack, visited, start, startNext);
                CheckAdjacent(stack, visited, startNext, start);
            }

            while (stack.Count != 0) 
            {
                // TODO: find a c# stack ADT
                // NOTE: currLower defines the direction
                ((uint, uint) curr, (uint, uint) next) = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);

                var tilePos = TilePosOf(curr, next);
                visited.Add(tilePos);
                gridCache[GridIndexOf(tilePos)] = true;

                bool cycle = CheckAdjacent(stack, visited, curr, next);
                if (cycle) 
                    return true;
            }

            return false;
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
