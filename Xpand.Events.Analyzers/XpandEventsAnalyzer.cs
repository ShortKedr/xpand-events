#nullable enable

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xpand.Events.Analyzers {
    /// <summary>Reports common signal ownership and lifecycle mistakes.</summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class XpandEventsAnalyzer : DiagnosticAnalyzer {
        /// <summary>Diagnostic ID for a discarded subscription token.</summary>
        internal const string DiscardedSubscriptionId = "XPAND001";

        /// <summary>Diagnostic ID for an asynchronous handler passed to a synchronous signal.</summary>
        internal const string AsyncVoidHandlerId = "XPAND002";

        /// <summary>Diagnostic ID for a static signal publisher with process-lifetime retention risk.</summary>
        internal const string StaticPublisherId = "XPAND003";

        /// <summary>Diagnostic ID for a Unity subscription without explicit lifecycle ownership.</summary>
        internal const string UnityLifecycleId = "XPAND004";

        private static readonly DiagnosticDescriptor DiscardedSubscription = new DiagnosticDescriptor(
            DiscardedSubscriptionId,
            "Subscription token is discarded",
            "Store or scope the IDisposable returned by Subscribe",
            "Lifetime",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Discarding a strong subscription token makes deterministic unsubscription difficult.");

        private static readonly DiagnosticDescriptor AsyncVoidHandler = new DiagnosticDescriptor(
            AsyncVoidHandlerId,
            "Async handler passed to synchronous signal",
            "Use AsyncSignal<T> instead of an async handler on synchronous Signal",
            "Correctness",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Synchronous signals do not await async void handlers or observe their failures.");

        private static readonly DiagnosticDescriptor StaticPublisher = new DiagnosticDescriptor(
            StaticPublisherId,
            "Static signal may retain subscribers for process lifetime",
            "Static signal field '{0}' requires an explicit process-lifetime ownership policy",
            "Lifetime",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Strong subscriptions on static publishers can retain targets for the process lifetime.");

        private static readonly DiagnosticDescriptor UnityLifecycle = new DiagnosticDescriptor(
            UnityLifecycleId,
            "Unity subscription needs lifecycle ownership",
            "Subscription in {0} must be disposed in the matching Unity lifecycle or use SignalListenerBehaviour",
            "Unity",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "MonoBehaviour subscriptions should use explicit enable/disable or destroy ownership.");

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            DiscardedSubscription,
            AsyncVoidHandler,
            StaticPublisher,
            UnityLifecycle);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context) {
            var invocation = (IInvocationOperation)context.Operation;
            if (!IsSubscription(invocation.TargetMethod)) return;

            if (invocation.Parent is IExpressionStatementOperation) {
                context.ReportDiagnostic(Diagnostic.Create(DiscardedSubscription, invocation.Syntax.GetLocation()));
            }

            if (IsSynchronousSignal(invocation.TargetMethod.ContainingType) &&
                invocation.Arguments.Length != 0 &&
                IsAsyncHandler(invocation.Arguments[0].Value)) {
                context.ReportDiagnostic(Diagnostic.Create(AsyncVoidHandler, invocation.Arguments[0].Syntax.GetLocation()));
            }

            if (context.ContainingSymbol is IMethodSymbol method && IsUnityLifecycleMethod(method) &&
                InheritsFrom(method.ContainingType, "UnityEngine", "MonoBehaviour") &&
                !InheritsFrom(method.ContainingType, "Xpand.Events.Unity", "SignalListenerBehaviour")) {
                context.ReportDiagnostic(Diagnostic.Create(UnityLifecycle, invocation.Syntax.GetLocation(), method.Name));
            }
        }

        private static void AnalyzeField(SymbolAnalysisContext context) {
            var field = (IFieldSymbol)context.Symbol;
            if (!field.IsStatic || !(field.Type is INamedTypeSymbol type) || !IsSignalType(type)) return;
            context.ReportDiagnostic(Diagnostic.Create(StaticPublisher, field.Locations[0], field.Name));
        }

        private static bool IsSubscription(IMethodSymbol method) {
            return method.Name == "Subscribe" && IsSignalType(method.ContainingType);
        }

        private static bool IsSignalType(INamedTypeSymbol type) {
            INamedTypeSymbol definition = type.OriginalDefinition;
            string namespaceName = definition.ContainingNamespace.ToDisplayString();
            return (definition.Name == "Signal" && namespaceName == "Xpand.Events") ||
                   (definition.Name == "AsyncSignal" && namespaceName == "Xpand.Events.Async");
        }

        private static bool IsSynchronousSignal(INamedTypeSymbol type) {
            INamedTypeSymbol definition = type.OriginalDefinition;
            return definition.Name == "Signal" && definition.ContainingNamespace.ToDisplayString() == "Xpand.Events";
        }

        private static bool IsAsyncHandler(IOperation operation) {
            while (operation is IConversionOperation conversion) operation = conversion.Operand;
            if (operation is IDelegateCreationOperation creation) operation = creation.Target;
            if (operation is IAnonymousFunctionOperation anonymous) return anonymous.Symbol.IsAsync;
            if (operation is IMethodReferenceOperation methodReference) return methodReference.Method.IsAsync;
            return false;
        }

        private static bool IsUnityLifecycleMethod(IMethodSymbol method) {
            return method.Name == "Awake" || method.Name == "Start" || method.Name == "OnEnable";
        }

        private static bool InheritsFrom(INamedTypeSymbol? type, string namespaceName, string typeName) {
            for (INamedTypeSymbol? current = type; current != null; current = current.BaseType) {
                if (current.Name == typeName && current.ContainingNamespace.ToDisplayString() == namespaceName) return true;
            }

            return false;
        }
    }
}
