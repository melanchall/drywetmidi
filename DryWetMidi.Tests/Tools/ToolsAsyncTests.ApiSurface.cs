using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed partial class ToolsAsyncTests
    {
        [Test]
        public void AsyncApis_CoverEverySynchronousOverload()
        {
            var types = new[]
            {
                typeof(CsvSerializer), typeof(Splitter), typeof(Merger), typeof(Quantizer),
                typeof(QuantizerUtilities), typeof(Repeater), typeof(RepeaterUtilities),
                typeof(Resizer), typeof(Sanitizer)
            };

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var sync in methods.Where(m => !m.Name.EndsWith("Async", StringComparison.Ordinal)))
                {
                    var parameters = sync.GetParameters();
                    var asyncMethod = methods.SingleOrDefault(m =>
                        m.Name == sync.Name + "Async" &&
                        m.GetParameters().Length == parameters.Length + 2 &&
                        m.GetParameters().Take(parameters.Length).Select(p => p.ParameterType).SequenceEqual(parameters.Select(p => p.ParameterType)));

                    ClassicAssert.IsNotNull(asyncMethod, type.Name + "." + sync + ": Async overload is missing.");
                    ClassicAssert.AreEqual(typeof(IProgress<AsyncOperationProgress>), asyncMethod.GetParameters()[parameters.Length].ParameterType, type.Name + "." + sync + ": Async progress parameter type is invalid.");
                    ClassicAssert.AreEqual(typeof(CancellationToken), asyncMethod.GetParameters().Last().ParameterType, type.Name + "." + sync + ": Async cancellation parameter type is invalid.");
                    ClassicAssert.IsTrue(asyncMethod.GetParameters().Last().IsOptional, type.Name + "." + sync + ": Async cancellation parameter is not optional.");
                }
            }
        }
    }
}
