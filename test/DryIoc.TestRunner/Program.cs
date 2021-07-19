﻿using System;
using System.Diagnostics;
using DryIoc.IssuesTests;

namespace DryIoc.UnitTests
{
    public class Program
    {
        public static void Main()
        {
            RunAllTests();
    
            // new OpenGenericsTests().Run();
            // new GHIssue391_Deadlock_during_Resolve().Run();
            // new GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration().Run();
            // new GHIssue380_ExportFactory_throws_Container_disposed_exception().Run();
            // new GHIssue402_Inconsistent_transient_disposable_behavior_when_using_Made().Run();
            // new GHIssue406_Allow_the_registration_of_the_partially_closed_implementation_type().Run();
        }

        public static void RunAllTests()
        {
            var failed = false;
            var totalTestPassed = 0;
            void Run(Func<int> run, string name = null)
            {
                var testsName = name ?? run.Method.DeclaringType.FullName;
                try
                {
                    var testsPassed = run();
                    totalTestPassed += testsPassed;
                    Console.WriteLine($"{testsPassed,-4} of {testsName}");
                }
                catch (Exception ex)
                {
                    failed = true;
                    Console.WriteLine($"ERROR: Tests `{testsName}` failed with '{ex}'");
                }
            }

            var sw = Stopwatch.StartNew();

            Console.WriteLine();
            Console.WriteLine("NETCOREAPP2.1: Running UnitTests and IssueTests...");
            Console.WriteLine();

            var tests = new ITest[] 
            {
                new OpenGenericsTests(),
                new DynamicRegistrationsTests(),
                new GHIssue378_InconsistentResolutionFailure(),
                new GHIssue380_ExportFactory_throws_Container_disposed_exception(),
                new GHIssue391_Deadlock_during_Resolve(),
                new GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration(),
                new GHIssue402_Inconsistent_transient_disposable_behavior_when_using_Made(),
                new GHIssue406_Allow_the_registration_of_the_partially_closed_implementation_type(),
            };

            // Parallel.ForEach(tests, x => Run(x.Run)); // todo: @perf enable and test when more tests are added
            foreach (var x in tests) Run(x.Run);

            if (failed)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: Some tests are FAILED!");
                Environment.ExitCode = 1; // error exit code
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"{totalTestPassed,-4} of all tests are passing in {sw.ElapsedMilliseconds} ms.");
        }
    }
}
