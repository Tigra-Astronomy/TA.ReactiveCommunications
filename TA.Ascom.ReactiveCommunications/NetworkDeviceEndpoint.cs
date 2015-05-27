// This file is part of the TA.Ascom.ReactiveCommunications project
// 
// Copyright © 2015 Tigra Astronomy, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so,. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: NetworkDeviceEndpoint.cs  Last modified: 2015-05-26@22:03 by Tim Long

namespace TA.Ascom.ReactiveCommunications
    {
    /// <summary>
    ///     Represents a network endpoint, with a hostaddress
    ///     and port number.
    /// </summary>
    public class NetworkDeviceEndpoint : DeviceEndpoint
        {
        public NetworkDeviceEndpoint(string host, int port)
            {
            Host = host;
            Port = port;
            }

        public int Port { get; private set; }

        public string Host { get; private set; }

        public override string ToString()
            {
            return string.Format("{0}:{1}", Host, Port);
            }
        }
    }