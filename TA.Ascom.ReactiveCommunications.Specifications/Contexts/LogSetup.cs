// This file is part of the Shareware Registration Solution project
// 
// Copyright © 2014 TiGra Networks, all rights reserved.
// 
// File: LogSetup.cs  Created: 2014-07-31@22:42
// Last modified: 2014-07-31@22:42 by Tim

using Machine.Specifications;
using Machine.Specifications.Annotations;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace TN.Prs.RegistrationServices.Specifications.TestHelpers
    {
    [UsedImplicitly]
    public class LogSetup : IAssemblyContext
        {
        static Logger log;

        public void OnAssemblyStart()
            {
            var configuration = new LoggingConfiguration();
            var unitTestRunnerTarget = new TraceTarget();
            configuration.AddTarget("Unit test runner", unitTestRunnerTarget);
            var logEverything = new LoggingRule("*", LogLevel.Debug, unitTestRunnerTarget);
            configuration.LoggingRules.Add(logEverything);
            LogManager.Configuration = configuration;
            log = LogManager.GetCurrentClassLogger();
            log.Info("Logging initialized");
            }

        public void OnAssemblyComplete()
            {
            }
        }
    }
