// This file is part of the TA.Ascom.ReactiveCommunications project
//
// Copyright © 2015-2020 Tigra Astronomy, all rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
//
// File: SimulatorStateMachine.cs  Last modified: 2020-07-16@02:26 by Tim Long

using System;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using TA.Utils.Core.Diagnostics;

namespace SimulatorChannel.StateMachine
    {
    /// <summary>Class SimulatorStateMachine. This class cannot be inherited.</summary>
    internal sealed class SimulatorStateMachine
        {
        /// <summary>
        ///     Thread synchronization object which tells waiting threads when the state machine is in the
        ///     'idle' or 'ready' state.
        /// </summary>
        public readonly ManualResetEvent InReadyState = new ManualResetEvent(false);

        private readonly ILog log;
        private readonly Subject<char> receiveSubject = new Subject<char>();
        private readonly IDisposable receiveSubscription;
        private readonly IClock timeSource;
        private readonly Subject<char> transmitSubject = new Subject<char>();

        /// <summary>Stores all of the parameters for the simulated hardware status.</summary>
        internal HardwareStatus HardwareStatus;

        /// <summary>
        ///     Characters received from the serial port, which accumulate until a valid command has been
        ///     received.
        /// </summary>
        internal StringBuilder ReceivedChars = new StringBuilder();

        /// <summary>
        ///     The current state of the simulated shutter, regardless of dome orientation. The sensor is
        ///     'indeterminate' at power-up and will remain so until the shutter has been run to full travel in
        ///     either direction. Simply being open or closed is not enough. Digital DomeWorks infers the
        ///     shutter position from the fact that travel has occurred for at least 5 seconds and the
        ///     direction of that travel.
        /// </summary>
        internal SensorState SimulatedShutterSensor = SensorState.Indeterminate;

        /// <summary>Initializes a new instance of the <see cref="SimulatorStateMachine" /> class.</summary>
        /// <param name="timeSource">A source of the current time.</param>
        /// <param name="simulatorConfiguration"></param>
        public SimulatorStateMachine(IClock timeSource, SimulatorConfiguration simulatorConfiguration, ILog logger)
            {
            Contract.Requires(timeSource != null);
            log = logger;
            Configuration = simulatorConfiguration;
            this.timeSource = timeSource;
            DomeSupportRingOpen = false;
            ShutterStuck = false;
            HardwareStatus = new HardwareStatus
                {
                AtHome = false,
                Coast = 1,
                CurrentAzimuth = 0,
                DeadZone = 3,
                DomeCircumference = 414,
                DsrSensor = SensorState.Indeterminate,
                FirmwareVersion = "V4",
                HomeClockwise = 16,
                HomeCounterClockwise = 1,
                HomePosition = 8,
                Humidity = 255,
                Lx200Azimuth = 999,
                Offset = 0,
                ShutterSensor = SensorState.Indeterminate,
                Slaved = false,
                Snow = 255,
                // For weather items, 255 means no data
                Temperature = 255,
                TimeStamp = timeSource.GetCurrentTime(),
                UserPins = 0,
                WeatherAge = 128,
                WindDirection = 255,
                WindPeak = 255,
                WindSpeed = 255
                };

            SetAzimuthDependentSensorsAndStates();

            // Set the starting state and begin receiving.
            SimulatorState.Transition(new StateStartup(this));
            var receiveObservable = receiveSubject.AsObservable();
            receiveSubscription = receiveObservable.Subscribe(InputStimulus, EndOfSimulation);
            }

        public SimulatorConfiguration Configuration { get; } = new SimulatorConfiguration();

        /// <summary>
        ///     An observable sequence of characters that simulates data arriving from the dome controller to
        ///     the PC serial port.
        /// </summary>
        public IObservable<char> ObservableResponses => transmitSubject.AsObservable();

        /// <summary>
        ///     Simulate sending characters to the dome controller by calling the observer's
        ///     <see cref="System.IObserver{T}.OnNext(T)" /> method.
        /// </summary>
        public IObserver<char> InputObserver => receiveSubject.AsObserver();

        /// <summary>Gets or sets the target azimuth in degrees.</summary>
        /// <value>The target azimuth degrees.</value>
        internal int TargetAzimuthDegrees
            {
            get => (int) (359.0 * TargetAzimuthTicks / Configuration.DomeCircumferenceTicks);
            set
                {
                if (value < 0 || value > 359)
                    throw new ArgumentOutOfRangeException("value", "Azimuth must be in the range 0 to 359 degrees");

                // Degrees = 359.0 * TargetAzimuthTicks / DomeCircumferenceTicks
                // Degrees * DomeCircumferenceTicks = 359.0 *  TargetAzimuthTicks
                //Degrees * DomeCircumferenceTicks / 359.0 = TargetAzimuthTicks
                TargetAzimuthTicks = (int) (value * HardwareStatus.DomeCircumference / 360.0);
                }
            }

        /// <summary>Gets or sets the target azimuth in ticks.</summary>
        /// <value>The target azimuth in raw encoder ticks.</value>
        internal int TargetAzimuthTicks { get; set; }

        /// <summary>Gets or sets the dome's current azimuth.</summary>
        /// <value>The azimuth.</value>
        /// <remarks>
        ///     When setting the value, the result is 'wrapped' at
        ///     <see cref="SimulatorCommunicationsChannel.HardwareStatus.DomeCircumference" />.
        /// </remarks>
        internal int AzimuthTicks
            {
            get => HardwareStatus.CurrentAzimuth;
            set
                {
                var modulus = value % Configuration.DomeCircumferenceTicks;
                if (modulus < 0)
                    HardwareStatus.CurrentAzimuth = Configuration.DomeCircumferenceTicks + modulus;
                else
                    HardwareStatus.CurrentAzimuth = modulus;
                }
            }

        /// <summary>
        ///     A flag indicating whether the simulation should proceed in real time (<c>true</c>) or at an
        ///     accelerated pace (<c>false</c>).
        /// </summary>
        public bool RealTime => Configuration.Realtime;

        /// <summary>When true, behaves as if the Dome Support Ring Swingout is open.</summary>
        public bool DomeSupportRingOpen { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the shutter is stuck. This allows simulation of failed
        ///     shutter commands. When <c>true</c> the shutter will not move.
        /// </summary>
        /// <value><c>true</c> if [shutter stuck]; otherwise, <c>false</c>.</value>
        public bool ShutterStuck { get; set; }

        private void EndOfSimulation()
            {
            transmitSubject.OnCompleted();
            }

        private void InputStimulus(char c)
            {
            SimulatorState.CurrentState.Stimulus(c);
            }

        /// <summary>
        ///     Sets the state of the Dome Support Ring sensor, based on the current azimuth. Teh sensor has a
        ///     valid value only when the dome is at the home position. At other times, it is Indeterminate.
        /// </summary>
        internal void SetAzimuthDependentSensorsAndStates()
            {
            if (InHomeRange(HardwareStatus.CurrentAzimuth))
                {
                HardwareStatus.DsrSensor = DomeSupportRingOpen ? SensorState.Open : SensorState.Closed;
                HardwareStatus.AtHome = true;
                }
            else
                {
                HardwareStatus.DsrSensor = SensorState.Indeterminate;
                HardwareStatus.AtHome = false;
                }
            }

        /// <summary>
        ///     Writes the tx data followed by a newline sequence. No need to invoke the SentData event because
        ///     Write() will do it.
        /// </summary>
        /// <param name="txData">The tx data.</param>
        internal void WriteLine(string txData)
            {
            if (!txData.EndsWith(Environment.NewLine))
                txData += Environment.NewLine;
            // Send the string one character at a time to the transmitSubject.
            foreach (var c in txData) transmitSubject.OnNext(c);
            }

        /// <summary>
        ///     Determines whether the given azimuth is within the range of values considered to be 'AtHome'.
        ///     On a physical dome, this region typically represents a few degrees of rotation and is
        ///     determined by the length of the sliding contact plates.
        /// </summary>
        /// <param name="azimuth">The azimuth.</param>
        /// <returns>
        ///     <c>true</c> if the azimuth is between <see cref="IHardwareStatus.HomeCounterClockwise" /> and
        ///     <see cref="IHardwareStatus.HomeClockwise" />.
        /// </returns>
        public bool InHomeRange(int azimuth)
            {
            if (HardwareStatus.HomeCounterClockwise > HardwareStatus.HomeClockwise) // Home zone straddles 0 azimuth
                return azimuth >= HardwareStatus.HomeCounterClockwise || azimuth <= HardwareStatus.HomeClockwise;
            return azimuth >= HardwareStatus.HomeCounterClockwise && azimuth <= HardwareStatus.HomeClockwise;
            }

        /// <summary>
        ///     Determines whether the specified target azimuth (in degrees) is within the 'dead zone' based on
        ///     the dome's current position.
        /// </summary>
        /// <param name="targetDegrees">The target position (in degrees).</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     Thrown if the target is outside the range 0..359
        ///     degrees.
        /// </exception>
        /// <returns><c>true</c> if the target is within the dead zone, <c>false</c> otherwise.</returns>
        public bool InDeadZone(int targetDegrees)
            {
            if (targetDegrees < 0 || targetDegrees >= 360)
                throw new ArgumentOutOfRangeException(nameof(targetDegrees), "Must be in the rang 0..359");
            var currentAzimuthDegrees = 359.0 * HardwareStatus.CurrentAzimuth / HardwareStatus.DomeCircumference;
            var zoneLowerBound = (currentAzimuthDegrees - HardwareStatus.DeadZone + 360) % 360;
            var zoneUpperBound = (currentAzimuthDegrees + HardwareStatus.DeadZone) % 360;
            if (zoneLowerBound > zoneUpperBound) // zero-crossing case
                return targetDegrees >= zoneLowerBound || targetDegrees < zoneUpperBound;
            // Normal case
            return targetDegrees >= zoneLowerBound && targetDegrees <= zoneUpperBound;
            }

        [ContractInvariantMethod]
        private void ObjectInvariant()
            {
            Contract.Invariant(InReadyState != null);
            }

        #region Disposable pattern
        /// <summary>Releases the resources used by this object.</summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only
        ///     unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
            {
            if (disposing)
                {
                receiveSubscription?.Dispose();
                transmitSubject?.OnCompleted();
                }
            }

        /// <summary>User request to dispose the object releases both managed and unmanaged resources.</summary>
        public void Dispose()
            {
            Dispose(true);
            GC.SuppressFinalize(this);
            }

        /// <summary>
        ///     Releases unmanaged resources and performs other cleanup operations before the
        ///     <see cref="SimulatorStateMachine" /> is reclaimed by garbage collection.
        /// </summary>
        ~SimulatorStateMachine()
            {
            Dispose(false);
            }
        #endregion

        #region Events and Invokers
        /// <summary>Occurs when [motor configuration changed].</summary>
        public static event EventHandler<MotorConfigurationEventArgs> MotorConfigurationChanged;

        internal void InvokeMotorConfigurationChanged(MotorConfigurationEventArgs e)
            {
            MotorConfigurationChanged?.Invoke(this, e);
            }

        /// <summary>
        ///     Occurs when the azimuth changes. This event will occur once per encoder tick during dome
        ///     rotation.
        /// </summary>
        public static event EventHandler<AzimuthChangedEventArgs> AzimuthChanged;

        /// <summary>Invokes the azimuth changed event.</summary>
        /// <param name="e">The <see cref="AzimuthChangedEventArgs" /> instance containing the event data.</param>
        internal void InvokeAzimuthChanged(AzimuthChangedEventArgs e)
            {
            AzimuthChanged?.Invoke(this, e);
            }

        /// <summary>Occurs when data is received on the serial port.</summary>
        public static event EventHandler ReceivedData;

        /// <summary>Invokes the received data event.</summary>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        public void InvokeReceivedData(EventArgs e)
            {
            ReceivedData?.Invoke(this, e);
            }

        /// <summary>Occurs when data is sent to the serial port.</summary>
        public static event EventHandler SentData;

        /// <summary>Invokes the sent data event.</summary>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private static void InvokeSentData(EventArgs e)
            {
            SentData?.Invoke(null, e);
            }
        #endregion
        }
    }