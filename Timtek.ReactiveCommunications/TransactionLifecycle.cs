﻿// This file is part of the Timtek.ReactiveCommunications project
// 
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: TransactionLifecycle.cs  Last modified: 2020-07-20@00:51 by Tim Long

namespace Timtek.ReactiveCommunications;

/// <summary>States of the transaction life cycle.</summary>
public enum TransactionLifecycle
{
    /// <summary>The transaction has been committed and failed to complete.</summary>
    Failed = 0,
    /// <summary>The transaction has been created but not yet committed.</summary>
    Created = 1,
    /// <summary>The transaction has been committed and is currently in progress.</summary>
    InProgress = 2,
    /// <summary>The transaction has been committed and completed successfully.</summary>
    Completed = 3
}