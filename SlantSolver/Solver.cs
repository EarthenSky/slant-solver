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
        public Solver(Puzzle puzzle) => this.puzzle = puzzle;

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

        public Puzzle Solve() {
            throw new Exception("TODO: this");
        }
    }
}
