using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Xpand.Events.Analyzers;

namespace Xpand.Events.Tests.Signals {
    public sealed class AnalyzerTests {
        [Test]
        public async Task ReportsDiscardedSubscriptionToken() {
            Diagnostic[] diagnostics = await AnalyzeAsync(@"
using Xpand.Events;
class Consumer {
    void Run() {
        var signal = new Signal<int>();
        signal.Subscribe(_ => { });
    }
}");

            Assert.That(diagnostics.Select(diagnostic => diagnostic.Id),
                Does.Contain(XpandEventsAnalyzer.DiscardedSubscriptionId));
        }

        [Test]
        public async Task ReportsAsyncVoidHandlerOnSynchronousSignal() {
            Diagnostic[] diagnostics = await AnalyzeAsync(@"
using System.Threading.Tasks;
using Xpand.Events;
class Consumer {
    void Run() {
        var signal = new Signal<int>();
        var token = signal.Subscribe(async _ => await Task.Yield());
    }
}");

            Assert.That(diagnostics.Select(diagnostic => diagnostic.Id),
                Does.Contain(XpandEventsAnalyzer.AsyncVoidHandlerId));
        }

        [Test]
        public async Task ReportsStaticPublisher() {
            Diagnostic[] diagnostics = await AnalyzeAsync(@"
using Xpand.Events;
class Consumer {
    private static readonly Signal<int> Global = new Signal<int>();
}");

            Assert.That(diagnostics.Select(diagnostic => diagnostic.Id),
                Does.Contain(XpandEventsAnalyzer.StaticPublisherId));
        }

        [Test]
        public async Task ReportsUnityLifecycleSubscription() {
            Diagnostic[] diagnostics = await AnalyzeAsync(@"
using System;
using Xpand.Events;
namespace UnityEngine { public class MonoBehaviour { } }
class Consumer : UnityEngine.MonoBehaviour {
    private readonly Signal<int> signal = new Signal<int>();
    private IDisposable token;
    void OnEnable() {
        token = signal.Subscribe(_ => { });
    }
}");

            Assert.That(diagnostics.Select(diagnostic => diagnostic.Id),
                Does.Contain(XpandEventsAnalyzer.UnityLifecycleId));
        }

        [Test]
        public async Task StoredSubscriptionOutsideUnityLifecycleIsAccepted() {
            Diagnostic[] diagnostics = await AnalyzeAsync(@"
using System;
using Xpand.Events;
class Consumer {
    IDisposable Run(Signal<int> signal) {
        return signal.Subscribe(_ => { });
    }
}");

            Assert.That(diagnostics, Is.Empty);
        }

        private static async Task<Diagnostic[]> AnalyzeAsync(string source) {
            string trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
            MetadataReference[] references = trustedAssemblies
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Concat(new[] {
                    MetadataReference.CreateFromFile(typeof(global::Xpand.Events.Signal<int>).Assembly.Location)
                })
                .ToArray();
            CSharpCompilation compilation = CSharpCompilation.Create(
                "AnalyzerSample",
                new[] { CSharpSyntaxTree.ParseText(source) },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new XpandEventsAnalyzer());
            ImmutableArray<Diagnostic> diagnostics = await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync();
            return diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start).ToArray();
        }
    }
}
