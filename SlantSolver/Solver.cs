using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlantSolver.Types;

namespace SlantSolver
{
    public class Solver
    {
        private Puzzle puzzle;
        public Solver(Puzzle puzzle) => this.puzzle = new Puzzle(puzzle);

        /// Determine if ths current puzzle is solved or not
        public bool IsSolved()
        {
            // no empty cells
            foreach (byte cell in puzzle.grid)
                if (cell == Puzzle.EMPTY_CELL)
                    return false;

            return SatisfiesRules();
        }

        /// returns true if all rules are satisfied
        private bool SatisfiesRules()
        {
            for (uint y = 0; y < puzzle.VertexHeight(); y++)
            {
                for (uint x = 0; x < puzzle.VertexWidth(); x++)
                {
                    byte circle = puzzle.vertices[puzzle.VertexIndexOf(new Vec2(x, y))];
                    if (circle != Puzzle.NONE)
                        if (puzzle.IncomingLines(new Vec2(x, y)) != circle)
                            return false;
                        else
                            continue;
                }
            }

            return puzzle.ContainsCycle() == false;
        }

        // -------------------- //

        public Puzzle Solve() {
            ApplyEdgePattern();

            return puzzle;
        }

        /// This function checks the edges for 2s or 0s, or 1s in the corner and fills them in
        private bool ApplyEdgePattern() {
            for (uint x = 0; x < puzzle.VertexWidth(); x++) {
                var curr = new Vec2(x, 0);
                var circle = puzzle.vertices[puzzle.VertexIndexOf(curr)];
                if (circle == 0 || circle == 2)
                {
                    if (puzzle.VertexDownLeftExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr.Left())] = circle == 0 ? Puzzle.TOPLEFT : Puzzle.TOPRIGHT; // TODO: add a puzzle.SetGrid(index, value); function
                    
                    if (puzzle.VertexDownRightExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr)] = circle == 0 ? Puzzle.TOPRIGHT : Puzzle.TOPLEFT;
                }

                curr = new Vec2(x, puzzle.VertexHeight() - 1);
                circle = puzzle.vertices[puzzle.VertexIndexOf(curr)];
                if (circle == 0 || circle == 2)
                {
                    if (puzzle.VertexUpLeftExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr.Up().Left())] = circle == 0 ? Puzzle.TOPRIGHT : Puzzle.TOPLEFT;

                    if (puzzle.VertexUpRightExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr.Up())] = circle == 0 ? Puzzle.TOPLEFT : Puzzle.TOPRIGHT;
                }
            }

            for (uint y = 1; y < puzzle.VertexHeight() - 1; y++)
            {
                var curr = new Vec2(0, y);
                var circle = puzzle.vertices[puzzle.VertexIndexOf(curr)];
                if (circle == 0 || circle == 2)
                {
                    if (puzzle.VertexUpRightExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr.Up())] = circle == 0 ? Puzzle.TOPLEFT : Puzzle.TOPRIGHT;

                    if (puzzle.VertexDownRightExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr)] = circle == 0 ? Puzzle.TOPRIGHT : Puzzle.TOPLEFT;
                }

                curr = new Vec2(puzzle.VertexWidth() - 1, y);
                circle = puzzle.vertices[puzzle.VertexIndexOf(curr)];
                if (circle == 0 || circle == 2)
                {
                    if (puzzle.VertexUpLeftExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr.Left().Up())] = circle == 0 ? Puzzle.TOPRIGHT : Puzzle.TOPLEFT;

                    if (puzzle.VertexDownLeftExists(curr))
                        puzzle.grid[puzzle.GridIndexOf(curr.Left())] = circle == 0 ? Puzzle.TOPLEFT : Puzzle.TOPRIGHT;
                }
            }

            // TODO: replace all the indices with constants in the puzzle class or functions
            var topLeft = puzzle.vertices[0];
            var topRight = puzzle.vertices[puzzle.VertexWidth() - 1];
            var botLeft = puzzle.vertices[puzzle.VertexWidth() * (puzzle.VertexHeight() - 1)];
            var botRight = puzzle.vertices[puzzle.VertexWidth() * puzzle.VertexHeight() - 1];
            if (topLeft == 1) 
                puzzle.grid[0] = Puzzle.TOPLEFT;
            else if (topRight == 1) 
                puzzle.grid[puzzle.GridWidth() - 1] = Puzzle.TOPRIGHT;
            else if (botLeft == 1)
                puzzle.grid[puzzle.GridWidth() * (puzzle.GridHeight() - 1)] = Puzzle.TOPRIGHT;
            else if (botRight == 1)
                puzzle.grid[puzzle.GridWidth() * puzzle.GridHeight() - 1] = Puzzle.TOPLEFT;

            return false;
        }
    }
}
