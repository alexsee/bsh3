// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Brightbits.BSH.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncCompletionAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor DiscardedTask = new(
        "BSH0001", "Observe asynchronous completion",
        "Observe completion of '{0}': await or return the task, or use GetAwaiter().GetResult() in a synchronous adapter",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NestedTask = new(
        "BSH0002", "Do not create nested tasks with StartNew",
        "Use Task.Run(async () => await ...) instead of Task.Factory.StartNew so completion and failures follow the inner task",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AsyncAction = new(
        "BSH0003", "Preserve asynchronous callback completion",
        "Use a task-returning delegate instead of Action for asynchronous callbacks so callers can observe completion and failures",
        "Correctness", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiscardedTask, NestedTask, AsyncAction);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeExpressionStatement, OperationKind.ExpressionStatement);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        context.RegisterOperationAction(AnalyzeDelegateCreation, OperationKind.DelegateCreation);
    }

    private static void AnalyzeDelegateCreation(OperationAnalysisContext context)
    {
        var creation = (IDelegateCreationOperation)context.Operation;
        // Events are a legitimate async-void boundary, including events typed as Action.
        if (creation.Parent is IEventAssignmentOperation ||
            creation.Type is not INamedTypeSymbol delegateType ||
            delegateType.Name != "Action" || delegateType.ContainingNamespace.ToDisplayString() != "System")
        {
            return;
        }

        var method = creation.Target switch
        {
            IAnonymousFunctionOperation function => function.Symbol,
            IMethodReferenceOperation reference => reference.Method,
            _ => null
        };
        if (method is { IsAsync: true, ReturnsVoid: true })
        {
            context.ReportDiagnostic(Diagnostic.Create(AsyncAction, creation.Syntax.GetLocation()));
        }
    }

    private static bool IsTask(ITypeSymbol type)
    {
        for (var current = type as INamedTypeSymbol; current != null; current = current.BaseType)
        {
            if (current.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
                (current.Name == "Task" || current.Name == "ValueTask"))
            {
                return true;
            }
        }

        return false;
    }

    private static void AnalyzeExpressionStatement(OperationAnalysisContext context)
    {
        var expression = ((IExpressionStatementOperation)context.Operation).Operation;
        // Assignment transfers ownership to a local/field (including an explicit
        // discard for intentionally detached work); it does not drop a result.
        if (!(expression is IAssignmentOperation) && IsTask(expression.Type))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiscardedTask, expression.Syntax.GetLocation(), expression.Syntax));
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var method = invocation.TargetMethod;
        if (method.Name == "StartNew" &&
            method.ContainingType.Name == "TaskFactory" &&
            method.ContainingNamespace.ToDisplayString() == "System.Threading.Tasks" &&
            invocation.Type is INamedTypeSymbol result &&
            result.TypeArguments.Length == 1 && IsTask(result.TypeArguments[0]))
        {
            context.ReportDiagnostic(Diagnostic.Create(NestedTask, invocation.Syntax.GetLocation()));
        }
    }
}
