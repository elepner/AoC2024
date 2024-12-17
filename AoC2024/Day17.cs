using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOf.Types;
using Xunit;
using Xunit.Abstractions;


namespace AoC2024;
using DeviceProgram = int[];
public class Day17(ITestOutputHelper toh)
{
    public const string Sample = """
                                 Register A: 729
                                 Register B: 0
                                 Register C: 0

                                 Program: 0,1,5,4,3,0
                                 """;

    public const string TaskInput = """
                                    Register A: 30899381
                                    Register B: 0
                                    Register C: 0

                                    Program: 2,4,1,1,7,5,4,0,0,3,1,6,5,5,3,0
                                    """;

    [Fact]
    public void ShouldSolveSample()
    {
        var input = ParseInput(Sample);
        var result = Evaluate(input.state, input.program, out var finalSate);

        Assert.Equal("4,6,3,5,6,3,5,2,1,0".Split(",").Select(int.Parse), result);
    }


    [InlineData(0, 0, 9, "2,6", null)]
    [InlineData(10, 0, 0, "5,0,5,1,5,4", "0,1,2")]
    [InlineData(2024, 0, 0, "0,1,5,4,3,0", "4,2,5,6,7,7,7,7,3,1,0")]
    [InlineData(0, 29, 0, "1,7", null)]
    [InlineData(0, 2024, 43690, "4,0", null)]
    [Theory]
    public void MoreSamples(int a, int b, int c, string program, string? output)
    {
        var prog = program.Split(",").Select(int.Parse).ToArray();
        var state = new MachineState(a, b, c, 0);
        var result = Evaluate(state, prog, out var finalSate);
        var res = string.Join(",", result);

        if (a == 2024)
        {
            Assert.Equal(0, finalSate.A);
        }

        if (c == 9)
        {
            Assert.Equal(1, finalSate.B);
        }

        if (b == 29)
        {
            Assert.Equal(26, finalSate.B);
        }

        if (c == 43690)
        {
            Assert.Equal(44354, finalSate.B);
        }

        if (output != null)
        {
            Assert.Equal(output, res);
        }
        
    }

    [Fact]
    public void ShouldSolvePt1()
    {
        var input = ParseInput(TaskInput);
        var result = Evaluate(input.state, input.program, out var finalSate);
        var res = string.Join(",", result);

        Assert.False(res == "4,7,3,7,6,7,4,7,0");
    }

    [Fact]
    public void ShouldSolveSamplePt2()
    {
        var result = SolvePt2(ParseInput("""
                                         Register A: 2024
                                         Register B: 0
                                         Register C: 0
                                         
                                         Program: 0,3,5,4,3,0
                                         """));

        Assert.Equal(117440, result);
    }

    [Fact]
    public void ShouldSolvePt2()
    {
        var result = SolvePt2(ParseInput("""
                                         Register A: 30899381
                                         Register B: 0
                                         Register C: 0
                                         
                                         Program: 2,4,1,1,7,5,4,0,0,3,1,6,5,5,3,0
                                         """));

        toh.WriteLine(result.ToString());
    }

    private int SolvePt2((DeviceProgram program, MachineState state) input)
    {
        int res = 0;
        var expected = string.Join(",", input.program);


        for (int i = 0;; i++)
        {



            if (i % 1000 == 0)
            {
                System.Diagnostics.Debug.WriteLine($"Processed {i}");
            }

            if (Eval(input.program, input.state with { A = i }, expected))
            {
                return i;
            }


        }

    }

    private static bool Eval(DeviceProgram program, MachineState state, string expected)
    {
        try
        {
            var result = Evaluate(state, program, out var finalSate, (added) =>
            {

                if (added.Count > program.Length)
                {
                    throw new TerminateEvaluationException(false);
                }

                if (added.Count == program.Length && added.SequenceEqual(program))
                {
                    throw new TerminateEvaluationException(true);
                }

                var current = string.Join(",", added);
                if (!expected.StartsWith(current))
                {
                    throw new TerminateEvaluationException(false);
                }
            });

        }
        catch (TerminateEvaluationException ex)
        {
            if (ex.Success)
            {
                return true;
            }

        }

        return false;
    }

    private static List<int> Evaluate(MachineState state, DeviceProgram program, out MachineState finalState, Action<List<int>>? valueAdded = null)
    {
        List<int> output = new List<int>();
        while (program.Length > state.Pointer)
        {
            state = PerformOp(program[state.Pointer], program[state.Pointer + 1], state, (val) =>
            {
                output.Add(val);
                valueAdded?.Invoke(output);
            });
        }

        finalState = state;
        return output;
    }

    private (DeviceProgram program, MachineState state) ParseInput(string input)
    {
        var lines = input.Trim().Split(Environment.NewLine).Select(x => x.Trim()).ToArray();

        var registers = lines.Take(3).Select(x => x.Split(": ")[1]).Select(x => int.Parse(x)).ToArray();

        var program = lines.Last().Split(": ")[1].Split(",").Select(x => int.Parse(x)).ToArray();
        return (program, new MachineState(registers[0], registers[1], registers[1], 0));
    }

    private static int EvaluateComboOperand(int operand, MachineState machineState)
    {
        Assert.True(operand < 8);
        Assert.True(operand >= 0);
        return operand switch
        {
            <= 3 => operand,
            4 => machineState.A,
            5 => machineState.B,
            6 => machineState.C,
            7 => throw new Exception($"{operand} is reserved"),
            _ => throw new Exception($"{operand} is invalid"),
        };
    }

    private static int EvaluateLiteral(int operand, MachineState machineState)
    {
        return operand;
    }

    private static MachineState PerformOp(int opId, int operand, MachineState machineState, Action<int> output)
    {

        var combo = new Lazy<int>(() => EvaluateComboOperand(operand, machineState));
        var literal = new Lazy<int>(() => EvaluateLiteral(operand, machineState));

        
        int CalcAdv()
        {
            return (int)((double)machineState.A / (1L << combo.Value));
        }

        var newState = opId switch
        {
            0 => machineState with
            {
                A = CalcAdv()
            },
            1 => machineState with
            {
                B = machineState.B ^ literal.Value
            },
            2 => machineState with { B = combo.Value % 8 },
            3 => machineState.A == 0 ? machineState with{Pointer = machineState.Pointer + 2}: machineState with { Pointer = literal.Value },
            4 => machineState with { B = machineState.C ^ machineState.B },
            5 => Output(() =>
            {
                output(combo.Value % 8);
                return machineState;
            }),
            6 => machineState with
            {
                B = CalcAdv()
            },
            7 => machineState with
            {
                C = CalcAdv()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(opId), opId, null)
        };

        if (opId != 3)
        {
            newState = newState with { Pointer = machineState.Pointer + 2 };
        }

        return newState;
    }

    private static MachineState Output(Func<MachineState> stateFn)
    {
        return stateFn();
    }

}

class TerminateEvaluationException(bool success) : Exception
{
    public bool Success { get; } = success;
};

record struct MachineState(int A, int B, int C, int Pointer);
