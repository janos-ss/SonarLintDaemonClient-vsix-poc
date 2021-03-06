﻿//------------------------------------------------------------------------------
// <copyright file="VSPackage1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Grpc.Core;
using Sonarlint;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSIXProject1
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(PackageGuidString)]
    [ProvideBindingPath]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class VSPackage1 : Package
    {
        /// <summary>
        /// VSPackage1 GUID string.
        /// </summary>
        public const string PackageGuidString = "f77859cb-ab84-46f4-94a4-9203eb59d710";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage1"/> class.
        /// </summary>
        public VSPackage1()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            runPOC();
        }

        private void runPOC()
        {
            Channel channel = new Channel("localhost:8050", ChannelCredentials.Insecure);
            var client = new StandaloneSonarLint.StandaloneSonarLintClient(channel);

            // sanity check ...
            var details = client.GetRuleDetails(new RuleKey { Key = "javascript:S2757" });
            Console.WriteLine("rule details = " + details);

            var inputFile = new InputFile();
            inputFile.Path = @"c:/Users/Janos Gyerik/Documents/Visual Studio 2015/Projects/SonarLintDaemonClient/resources/Hello.js";
            inputFile.Charset = "UTF-8";

            var request = new AnalysisReq();
            request.BaseDir = @"c:/work/tmp/daemon";
            request.WorkDir = @"c:/work/tmp/daemon";
            request.File.Add(inputFile);

            using (var call = client.Analyze(request))
            {
                ProcessIssues(call).Wait();
            }

            channel.ShutdownAsync().Wait();
        }

        private static async System.Threading.Tasks.Task ProcessIssues(AsyncServerStreamingCall<Issue> call)
        {
            while (await call.ResponseStream.MoveNext())
            {
                Issue issue = call.ResponseStream.Current;
                Console.WriteLine(issue);
            }
        }

        #endregion
    }
}
