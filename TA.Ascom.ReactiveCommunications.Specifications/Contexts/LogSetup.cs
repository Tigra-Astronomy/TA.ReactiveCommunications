// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2018 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: LogSetup.cs  Last modified: 2018-08-26@06:12 by Tim Long

using JetBrains.Annotations;
using Machine.Specifications;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace TA.Ascom.ReactiveCommunications.Specifications.Contexts
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
            unitTestRunnerTarget.Layout = "${time}|${pad:padding=-5:inner=${uppercase:${level}}}|${message}";
            var logEverything = new LoggingRule("*", LogLevel.Debug, unitTestRunnerTarget);
            configuration.LoggingRules.Add(logEverything);
            LogManager.Configuration = configuration;
            log = LogManager.GetCurrentClassLogger();
            log.Info("Logging initialized");
            }

        public void OnAssemblyComplete()
            {
            log.Info("Assembly tests complete");
            }
        }
    }