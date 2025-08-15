# Linear Programming Solvers.

This class library contains three LPP solvers:
 
 + **Primary Simplex Method;**
 + **The Big M method;**
 + **Gomory Cutting-Plane method.**

It also contains essential objects for defining a problem:

+ **Linear Programming Problem;**
+ **Constraint.**

## Usage example :computer::
```
var problem = new LinearProgrammingProblem
{
    IsMaximization = true,
    ObjectiveFunctionCoefficients = new List<string> { "1", "1" },
    Constraints = new List<Constraint>
    {
        new Constraint
        {
            Coefficients = new List<string> { "6", "5" },
            RightHandSide = "20",
            Type = ConstraintType.LessThanOrEqual,
        },
        new Constraint
        {
            Coefficients = new List<string> { "2", "3" },
            RightHandSide = "10",
            Type = ConstraintType.LessThanOrEqual,
        },
    }
};

var solver = new SimplexSolver(problem);
solver.Solve();
var gomory = new GomorySolver(_solver.Table, problem);
gomory.Solve();
```