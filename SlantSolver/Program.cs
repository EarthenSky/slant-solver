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

            // wait before exiting
            var _ = Console.ReadLine();
        }
    }
}
