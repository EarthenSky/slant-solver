using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlantSolver
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Program.TestCycleDetection();
            Program.TestSolver();

            // wait before exiting
            var _ = Console.ReadLine();
        }

        static void TestCycleDetection() 
        {
            Console.WriteLine("TestCycleDetection:\n");

            Puzzle puzzle = new Puzzle((5, 5), "a1102a1f1a2b1c3b2a2a0a1110a");
            puzzle.Display();

            puzzle.SetGrid(new byte[] {
                1,1,1,2,1,
                1,1,2,1,1,
                1,2,2,1,2,
                1,1,2,2,2,
                2,2,2,2,1,
            });
            Console.WriteLine("\nIsSolved: " + new Solver(puzzle).IsSolved().ToString());
            puzzle.Display();

            puzzle.SetGrid(new byte[] {
                1,1,1,2,1,
                1,1,2,1,2,
                1,2,2,1,2,
                1,1,2,2,2,
                2,2,2,2,1,
            });
            Console.WriteLine("\nIsSolved: " + new Solver(puzzle).IsSolved().ToString());
            puzzle.Display();

            Console.WriteLine("\n\n");
        }

        static void TestSolver()
        {
            Console.WriteLine("TestSolver:\n");

            Puzzle puzzle = new Puzzle((5, 5), "a1102a1f1a2b1c3b2a2a0a1110a");
            puzzle.Display();

            var solver = new Solver(puzzle);
            Puzzle solvedPuzzle = solver.Solve();
            solvedPuzzle.Display();

            Console.WriteLine("\n\n");
        }
    }
}
