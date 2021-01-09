﻿// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: ServiceLocator.cs  Last modified: 2020-07-20@00:50 by Tim Long

using TA.Utils.Core.Diagnostics;

namespace SimulatorChannel
    {
    internal sealed class ServiceLocator
        {
        private static ILog loggingService;

        public static ILog LoggingService
            {
            get => loggingService ?? (loggingService = new DegenerateLoggerService());
            set => loggingService = value;
            }
        }
    }