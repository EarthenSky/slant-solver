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
            Solver solver = new Solver((5, 5), "a1102a1f1a2b1c3b2a2a0a1110a");
            solver.PrintPuzzle();

            solver.SetGrid(new byte [] { 
                1,1,1,2,1,
                1,1,2,1,1,
                1,2,2,1,2,
                1,1,2,2,2,
                2,2,2,2,1,
            });
            Console.WriteLine("\nIsSolved: " + solver.IsSolved().ToString());
            solver.PrintPuzzle();
             
            // TODO: this should be false, due to loop detection
            solver.SetGrid(new byte[] {
                1,1,1,2,1,
                1,1,2,1,2,
                1,2,2,1,2,
                1,1,2,2,2,
                2,2,2,2,1,
            });
            Console.WriteLine("\nIsSolved: " + solver.IsSolved().ToString());
            solver.PrintPuzzle();

            // wait before exiting
            var _ = Console.ReadLine();
        }
    }
}
