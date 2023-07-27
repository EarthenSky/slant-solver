using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlantSolver.Types
{
    public struct Vec2
    {
        public uint x, y;
        public Vec2(uint x, uint y) => (this.x, this.y) = (x, y);
        public Vec2(Vec2 vec) => (this.x, this.y) = (vec.x, vec.y);

        public Vec2 Left() => new Vec2(x - 1, y);
        public Vec2 Right() => new Vec2(x + 1, y);
        public Vec2 Up() => new Vec2(x, y - 1);
        public Vec2 Down() => new Vec2(x, y + 1);

    }
}
