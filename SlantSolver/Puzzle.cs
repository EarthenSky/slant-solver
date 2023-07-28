using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlantSolver.Types;

namespace SlantSolver
{
    public class Puzzle
    {
        public const byte NONE = 255;
        public const byte ZERO = 0;
        public const byte ONE = 1;
        public const byte TWO = 2;
        public const byte THREE = 3;
        public const byte FOUR = 4;

        public const byte EMPTY_CELL = 0;
        public const byte TOPLEFT = 1;
        public const byte TOPRIGHT = 2;

        // TODO: convert into a Size(.width=uint, .height=uint) class
        private (uint, uint) size; // (x, y)
        private string puzzleString;

        // TODO: use enums
        public byte[] grid;
        public byte[] vertices; // size + (1,1)

        public Puzzle((uint, uint) size, string puzzle)
        {
            this.size = size;
            this.puzzleString = puzzle;

            this.grid = new byte[GridWidth() * GridHeight()];
            Utils.Populate(this.grid, EMPTY_CELL);

            this.vertices = new byte[VertexWidth() * VertexHeight()];
            Utils.Populate(this.vertices, NONE);

            this.ParsePuzzle(size, puzzle);
        }

        /// please ensure that grid and vertices are copies
        public Puzzle((uint, uint) size, byte[] grid, byte[] vertices)
        {
            this.size = size;
            this.puzzleString = "N/A";

            // TODO: ensure that grid & vertices are well formed
            this.grid = grid;
            this.vertices = vertices;
        }
        public Puzzle(Puzzle puzzle)
        {
            this.size = puzzle.size;
            this.puzzleString = puzzle.puzzleString;
            this.grid = puzzle.grid.Clone() as byte[];
            this.vertices = puzzle.vertices.Clone() as byte[];
        }

        public void Display()
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.WriteLine("The current puzzle: ");
            Console.WriteLine(this.puzzleString);
            for (uint y = 0; y < size.Item2 + 1; y++)
            {
                for (uint x = 0; x < size.Item1 + 1; x++)
                {
                    byte curr = vertices[x + y * VertexWidth()];
                    if (curr != NONE)
                    {
                        Console.Write(curr.ToString());
                        if (x != size.Item1) Console.Write("---");
                    }
                    else
                    {
                        Console.Write(' ');
                        if (x != size.Item1) Console.Write("---");
                    }
                }

                Console.WriteLine("");

                if (y != size.Item2)
                {
                    for (uint x = 0; x < size.Item1 + 1; x++)
                    {
                        Console.Write('|');
                        if (x != size.Item1)
                        {
                            byte value = grid[GridIndexOf(new Vec2(x, y))];
                            string token = value == EMPTY_CELL ? " " : (value == TOPLEFT ? "╲" : "╱");
                            Console.Write(" " + token + " ");
                        }
                    }

                    Console.WriteLine("");
                }
            }
        }

        /// For testing
        public void SetGrid(byte[] grid)
        {
            if (grid.Length != size.Item1 * size.Item2)
                throw new Exception("invalid grid size");

            this.grid = grid;
        }

        /// Determine if the current puzzle contains a loop of some kind
        public bool ContainsCycle()
        {
            // contains true if this tile has been part of a tree / cycle that has been checked.
            bool[] gridCache = new bool[GridWidth() * GridHeight()];
            Utils.Populate(gridCache, false);

            for (uint y = 0; y < GridHeight(); y++)
            {
                for (uint x = 0; x < GridWidth(); x++)
                {
                    bool hasBeenChecked = gridCache[x + y * GridWidth()];
                    if (!hasBeenChecked && HasCycleDFS(gridCache, new Vec2(x, y)))
                        return true;
                }
            }

            return false;
        }

        private bool HasCycleDFS(bool[] gridCache, Vec2 start)
        {
            bool[] touched = new bool[GridWidth() * GridHeight()];
            Utils.Populate(touched, false);

            // TODO: turn visited into a map
            Stack<(Vec2, Vec2)> stack = new Stack<(Vec2, Vec2)>();
            List<Vec2> visited = new List<Vec2>();

            // evaluate starting diagonal in both directions
            {
                // figure out the locations of the starting verticies
                Vec2 startNext;
                if (grid[GridIndexOf(start)] == TOPRIGHT)
                {
                    start = start.Right();
                    startNext = start.Left().Down();
                }
                else
                {
                    startNext = start.Right().Down();
                }

                var tilePos = TilePosOf(start, startNext);
                visited.Add(tilePos);
                gridCache[GridIndexOf(tilePos)] = true;

                CheckAdjacent(stack, visited, start, startNext);
                CheckAdjacent(stack, visited, startNext, start);
            }

            while (stack.Count != 0)
            {
                // curr,next is a diagonal line segment starting from curr and ending at next
                (Vec2 curr, Vec2 next) = stack.Pop();

                var tilePos = TilePosOf(curr, next);
                visited.Add(tilePos);
                gridCache[GridIndexOf(tilePos)] = true;

                bool cycle = CheckAdjacent(stack, visited, curr, next);
                if (cycle)
                    return true;
            }

            return false;
        }

        // ------------------------ //
        // helpers:

        public uint GridHeight() => size.Item2;
        public uint GridWidth() => size.Item1;
        public uint VertexHeight() => size.Item2 + 1;
        public uint VertexWidth() => size.Item1 + 1;

        public uint GridIndexOf(Vec2 tile) => tile.x + tile.y * GridWidth();
        public uint VertexIndexOf(Vec2 tile) => tile.x + tile.y * VertexWidth();

        public bool TileAboveExists(Vec2 tile) => tile.y != 0;
        public bool TileLeftExists(Vec2 tile) => tile.x != 0;
        public bool TileRightExists(Vec2 tile) => tile.x != GridWidth() - 1;
        public bool TileBelowExists(Vec2 tile) => tile.y != GridHeight() - 1;

        public bool VertexUpLeftExists(Vec2 tile) => tile.y != 0 && tile.x != 0;
        public bool VertexUpRightExists(Vec2 tile) => tile.y != 0 && (tile.x != VertexWidth() - 1);
        public bool VertexDownLeftExists(Vec2 tile) => (tile.y != VertexHeight() - 1) && tile.x != 0;
        public bool VertexDownRightExists(Vec2 tile) => (tile.y != VertexHeight() - 1) && (tile.x != VertexWidth() - 1);

        /// this function determines the kind of tile given the start & end points of a diagonal
        public byte TileKind(Vec2 first, Vec2 second) => grid[GridIndexOf(TilePosOf(first, second))];

        /// This function takes a diagonal & turns it into the 'position' of the Tile the diagonal resides in
        public Vec2 TilePosOf(Vec2 first, Vec2 second)
        {
            if (first.x > second.x && first.y > second.y)
                return second;
            else if (first.x < second.x && first.y < second.y)
                return first;
            else if (first.x > second.x && first.y < second.y)
                return first.Left();
            else if (first.x < second.x && first.y > second.y)
                return second.Left();
            else
                throw new Exception("first and second do not form a diagonal: " + first.ToString() + " & " + second.ToString());
        }

        public uint IncomingLines(Vec2 pos)
        {
            // pos is vertex coordinates 
            var (x, y) = (pos.x, pos.y);
            uint numIncoming = 0;

            // case: top left square exists    
            if (y > 0 && x > 0)
                numIncoming += grid[(y - 1) * GridWidth() + (x - 1)] == TOPLEFT ? 1u : 0u;

            // case: top right square exists
            if (y > 0 && x < VertexWidth() - 1)
                numIncoming += grid[(y - 1) * GridWidth() + x] == TOPRIGHT ? 1u : 0u;

            // case: bottom left square exists    
            if (y < VertexHeight() - 1 && x > 0)
                numIncoming += grid[y * GridWidth() + (x - 1)] == TOPRIGHT ? 1u : 0u;

            // case: bottom right square exists
            if (y < VertexHeight() - 1 && x < VertexWidth() - 1)
                numIncoming += grid[y * GridWidth() + x] == TOPLEFT ? 1u : 0u;

            return numIncoming;
        }


        /// This function takes the the start & end points of a diagonal, and determines if any other 
        /// tiles are connected & unvisisted, adding them to the stack if so.
        /// 
        /// The return value represents whether a cycle has been detected or not
        public bool CheckAdjacent(
            Stack<(Vec2, Vec2)> stack,
            List<Vec2> visited,
            Vec2 curr,
            Vec2 next
        )
        {
            // NOTE: there is probably some way to generalize this code better, but the fact that it's
            // partially unrolled should count for a bit of performance!

            // add adjacent tiles if possible
            bool isNextBelow = next.y > curr.y;
            byte currentTileKind = TileKind(curr, next);
            var tilePos = TilePosOf(curr, next);
            if (currentTileKind == TOPLEFT && isNextBelow)
            {
                var below = next.Left();
                var belowNext = next.Left().Down();
                if (TileBelowExists(tilePos) && grid[GridIndexOf(below)] == TOPRIGHT)
                    if (visited.Contains(below))
                        return true;
                    else
                        stack.Push((next, belowNext));

                var right = next.Up();
                var rightNext = next.Up().Right();
                if (TileRightExists(tilePos) && grid[GridIndexOf(right)] == TOPRIGHT)
                    if (visited.Contains(right))
                        return true;
                    else
                        stack.Push((next, rightNext));

                var belowRight = next;
                var belowRightNext = next.Right().Down();
                if (TileBelowExists(tilePos) && TileRightExists(tilePos) && grid[GridIndexOf(belowRight)] == TOPLEFT)
                    if (visited.Contains(belowRight))
                        return true;
                    else
                        stack.Push((next, belowRightNext));
            }
            else if (currentTileKind == TOPLEFT && !isNextBelow)
            {
                var above = next.Up();
                var aboveNext = next.Up().Right();
                if (TileAboveExists(tilePos) && grid[GridIndexOf(above)] == TOPRIGHT)
                    if (visited.Contains(above))
                        return true;
                    else
                        stack.Push((next, aboveNext));

                var left = next.Left();
                var leftNext = next.Left().Down();
                if (TileLeftExists(tilePos) && grid[GridIndexOf(left)] == TOPRIGHT)
                    if (visited.Contains(left))
                        return true;
                    else
                        stack.Push((next, leftNext));

                var aboveLeft = next.Left().Up();
                var aboveLeftNext = next.Left().Up();
                if (TileAboveExists(tilePos) && TileLeftExists(tilePos) && grid[GridIndexOf(aboveLeft)] == TOPLEFT)
                    if (visited.Contains(aboveLeft))
                        return true;
                    else
                        stack.Push((next, aboveLeftNext));
            }
            else if (currentTileKind == TOPRIGHT && isNextBelow)
            {
                var below = next;
                var belowNext = next.Down().Right();
                if (TileBelowExists(tilePos) && grid[GridIndexOf(below)] == TOPLEFT)
                    if (visited.Contains(below))
                        return true;
                    else
                        stack.Push((next, belowNext));

                var left = next.Left().Up();
                var leftNext = next.Left().Up();
                if (TileLeftExists(tilePos) && grid[GridIndexOf(left)] == TOPLEFT)
                    if (visited.Contains(left))
                        return true;
                    else
                        stack.Push((next, leftNext));

                var belowLeft = next.Left();
                var belowLeftNext = next.Left().Down();
                if (TileBelowExists(tilePos) && TileLeftExists(tilePos) && grid[GridIndexOf(belowLeft)] == TOPRIGHT)
                    if (visited.Contains(belowLeft))
                        return true;
                    else
                        stack.Push((next, belowLeftNext));
            }
            else if (currentTileKind == TOPRIGHT && !isNextBelow)
            {
                var above = next.Left().Up();
                var aboveNext = next.Left().Up();
                if (TileAboveExists(tilePos) && grid[GridIndexOf(above)] == TOPLEFT)
                    if (visited.Contains(above))
                        return true;
                    else
                        stack.Push((next, aboveNext));

                var right = next;
                var rightNext = next.Right().Down();
                if (TileRightExists(tilePos) && grid[GridIndexOf(right)] == TOPLEFT)
                    if (visited.Contains(right))
                        return true;
                    else
                        stack.Push((next, rightNext));

                var aboveRight = next.Up();
                var aboveRightNext = next.Up().Right();
                if (TileAboveExists(tilePos) && TileRightExists(tilePos) && grid[GridIndexOf(aboveRight)] == TOPRIGHT)
                    if (visited.Contains(aboveRight))
                        return true;
                    else
                        stack.Push((next, aboveRightNext));
            }

            return false;
        }

        // ------------------------ //

        /// The seemingly widely accepted format for storing slant puzzles is to store a matrix of each crosshair.
        /// From left to right, each crosshair number is separated by the number of spaces indicated by the value of the letter in base 27 (probably)
        /// For example, no letter indicates that numbers are consecutive, while a represents 1 space, and b represents 2 spaces, etc.
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
                    if (char.IsDigit(ch))
                    {
                        uint value = (uint)ch - (uint)'0';
                        vertices[i] = (byte)value;
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

    }
}
