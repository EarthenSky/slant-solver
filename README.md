# slant-solver
 a slant solver! this thing solves slant

# what is slant?
- a puzzle in the same vein as sudoku!
- https://en.wikipedia.org/wiki/Gokigen_Naname

# TODO: 
- do an http fetch of https://www.puzzle-slant.com, search for `<span id="puzzleID">` for the puzzle id, `var task = ` for the puzzle string format, and `hashedSolution: ` for the hashed solution to the puzzle. 


# TOLEARN:
- https://learn.microsoft.com/en-us/aspnet/web-forms/overview/deployment/web-deployment-in-the-enterprise/understanding-the-project-file
  - .csproj is an msbuild c# (cs) project file!
  - MSBuild is not specifically attached to Visual Studio, you can customize it!
  -  

      
# Blog post:
  -start by reading the file format -> I was unable to find a real description of the format, but I managed to figure it out based on looking at t afew examples
  - next, we need to come up with a way to detecgt if the puzzle is solved or not. This is required in oredr to start wiht some brute force approaches. We can even count how many mistakes we catch.
    - in order to detect if the puzzle is solved or not, we need to search for any cycles. The playing field has no cycles if it is a forest (that is a set of unconnected acylic graphs (trees))

# blog post:
- slant is a fun but somewhat obscure puzzle (at least when compared with sudoku)
- lines can be placed from top left to bottom right, or top right to bottom left.
- each number can have 4 incoming or passing lines.
- an "implied n" is a number that would satify n if there was a number there.

# Algorithm Slant Patterns:
- a zero must be on the edge and must have two items not touching it
- a 1 in the corner must have one incoming line
- a 4 must have all incoming lines

- if a 2 has two incoming lines, the other two lines are outgoing
- if a 1 has one incoming line or three passing lines, then the others tiles are solved
- if a 3 has one passing line or three incoming lines, then the others tiles are solved

- two touching 1s makes a dome pattern
- two orthogonally touching 3s makes a spike pattern
- two diagonally touching 1s makes a line dividing them (unless at least one of the 1s is touching the edge)

- fully generalized: two 1s separated by n 2s, n>=0, makes a dome pattern. a 2 with a single incoming line in the opposite direction of the other item also counts as a 1. 
- fully generalized: two 3s separated by n 2s, n>=0, makes a dome pattern. ditto, but with one passing line
- a 3 with two inbound lines is a 1, the opposite is true about a 1 with two passing lines (is a 3).

- if placing a single line causes a cycle somewhere, then the other line direction must be correct

# Cycle detection algorithm:
- cycle detection can be optimized. Certain lines cannot be part of a cycle no matter what, and can be ignored. For example, any line that touches the edge of the board cannot be part of a cycle, as well as a line that intersects a 3 (or an implied 3)