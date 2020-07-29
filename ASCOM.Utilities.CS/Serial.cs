using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using ASCOM;
using System.IO.Ports;
using ASCOM.Utilities.CS.Interfaces;
using ASCOM.Utilities.CS.Exceptions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Thread;
using System.Runtime.Remoting.Contexts;
namespace ASCOM.Utilities.CS
{
// Implements the Serial component



// PortSpeed enum
/// <summary>

/// ''' Enumeration of serial port speeds for use with the Serial port

/// ''' </summary>

/// ''' <remarks>This contains an additional speed 230,400 baud that the COM component doesn't support.</remarks>
[Guid("06BB25BA-6E75-4d1b-8BB4-BA629454AE38")]
    [ComVisible(true)]
    public enum SerialSpeed : int // Defined at ACOM.Utilities level
    {
        ps300 = 300,
        ps1200 = 1200,
        ps2400 = 2400,
        ps4800 = 4800,
        ps9600 = 9600,
        ps14400 = 14400,
        ps19200 = 19200,
        ps28800 = 28800,
        ps38400 = 38400,
        ps57600 = 57600,
        ps115200 = 115200,
        ps230400 = 230400
    }

    /// <summary>

    /// ''' Number of stop bits appended to a serial character

    /// ''' </summary>

    /// ''' <remarks>

    /// ''' This enumeration specifies the number of stop bits to use. Stop bits separate each unit of data 

    /// ''' on an asynchronous serial connection. 

    /// ''' The None option is not supported. Setting the StopBits property to None raises an 

    /// ''' ArgumentOutOfRangeException. 

    /// ''' </remarks>
    [Guid("17D30DAA-C767-43fd-8AF4-5E149D5C771F")]
    [ComVisible(true)]
    public enum SerialStopBits
    {
        /// <summary>
        ///     ''' No stop bits are used. This value is not supported. Setting the StopBits property to None raises an ArgumentOutOfRangeException. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        None = 0,
        /// <summary>
        ///     ''' One stop bit is used. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        One = 1,
        /// <summary>
        ///     ''' 1.5 stop bits are used. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        OnePointFive = 3,
        /// <summary>
        ///     ''' Two stop bits are used. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        Two = 2
    }

    /// <summary>

    /// ''' The type of parity used on the serial port

    /// ''' </summary>

    /// ''' <remarks>

    /// ''' Parity is an error-checking procedure in which the number of 1s must always be the 

    /// ''' same — either even or odd — for each group of bits that is transmitted without error. 

    /// ''' Parity is one of the parameters that must be 

    /// ''' agreed upon by both sending and receiving parties before transmission can take place. 

    /// ''' </remarks>
    [Guid("92B19711-B44F-4642-9F96-5A20397B8FD1")]
    [ComVisible(true)]
    public enum SerialParity
    {
        /// <summary>
        ///     ''' Sets the parity bit so that the count of bits set is an even number. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        Even = 2,
        /// <summary>
        ///     ''' Leaves the parity bit set to 1. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        Mark = 3,
        /// <summary>
        ///     ''' No parity check occurs. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        None = 0,
        /// <summary>
        ///     ''' Sets the parity bit so that the count of bits set is an odd number. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        Odd = 1,
        /// <summary>
        ///     ''' Leaves the parity bit set to 0. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        Space = 4
    }

    /// <summary>

    /// ''' The control protocol used by the serial port

    /// ''' </summary>

    /// ''' <remarks></remarks>
    [Guid("55A24A18-02C8-47df-A048-2E0982E1E4FE")]
    [ComVisible(true)]
    public enum SerialHandshake
    {
        /// <summary>
        ///     ''' No control is used for the handshake. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        None = 0,
        /// <summary>
        ///     ''' Request-to-Send (RTS) hardware flow control is used. RTS signals that data is available 
        ///     ''' for transmission. If the input buffer becomes full, the RTS line will be set to false. 
        ///     ''' The RTS line will be set to true when more room becomes available in the input buffer. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        RequestToSend = 2,
        /// <summary>
        ///     ''' Both the Request-to-Send (RTS) hardware control and the XON/XOFF software controls are used. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        RequestToSendXonXoff = 3,
        /// <summary>
        ///     ''' The XON/XOFF software control protocol is used. The XOFF control is sent to stop 
        ///     ''' the transmission of data. The XON control is sent to resume the transmission. 
        ///     ''' These software controls are used instead of Request to Send (RTS) and Clear to Send (CTS) 
        ///     ''' hardware controls. 
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        XonXoff = 1
    }

    /// <summary>

    /// ''' Creates a .NET serial port and provides a simple set of commands to use it.

    /// ''' </summary>

    /// ''' <remarks>This object provides an easy to use interface to a serial (COM) port. 

    /// ''' It provides ASCII and binary I/O with controllable timeout.

    /// ''' The interface is callable from any .NET client.

    /// ''' <para>The platform allows you to control use of the DTR and RTS/CTS lines for a particular 

    /// ''' COM port and to remove or force listing of individual COM ports in the AvailableComPorts 

    /// ''' list through configuration in the ASCOM Profile.

    /// ''' Please see the Tools and Features section of this help file for further details.</para> 

    /// ''' </remarks>

    /// ''' <example>

    /// ''' Example of how to create and use an ASCOM serial port.

    /// ''' <code lang="vbnet" title="ASCOM Serial Port Example" 

    /// ''' source="..\ASCOM Platform Examples\Examples\SerialExamples.vb"

    /// ''' />

    /// '''</example>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("0B8E6DC4-7451-4484-BE15-0D0727569FB8")]
    [ComVisible(true)]
    public class Serial : ISerial, IDisposable
    {

        // State variables holding com port configuration
        private SerialPort m_Port;
        private string m_PortName;
        private int m_ReceiveTimeout;
        private SerialSpeed m_Speed;
        private bool m_Connected;
        private int m_DataBits;
        private bool m_DTREnable;
        private bool m_RTSEnable;
        private SerialHandshake m_Handshake;
        private SerialParity m_Parity;
        private SerialStopBits m_StopBits;

        private string TraceFile; // Current trace file name
        private bool disposed = false; // IDisposable variable to detect redundant calls
        private bool DebugTrace = false; // Flag specifying type of information to record in log file
        private Stopwatch ts = new Stopwatch();

        private TraceLogger Logger;
        private System.Text.Encoding TextEncoding;
        private string m_SerTraceFile = SERIAL_DEFAULT_FILENAME; // Set the default trace file name

        private RegistryAccess SerialProfile = null/* TODO Change to default(_) if this is not a reference type */;
        private Generic.SortedList<string, string> ForcedCOMPorts;
        private Generic.SortedList<string, string> IgnoredCOMPorts;

        private System.Threading.Semaphore SerSemaphore;
        private bool SerPortInUse = false;

        private System.Threading.Semaphore CallCountSemaphore = new System.Threading.Semaphore(1, 1);
        private long CallCount; // Counter for calls to this component

        private const int TIMEOUT_NUMBER = Constants.vbObjectError + 0x402;
        private const string TIMEOUT_MESSAGE = "Timed out waiting for received data";
        private const int SEMAPHORE_TIMEOUT = 1000;

        // Serial port parameters
        private const int SERIALPORT_ENCODING = 1252;
        private const string SERIALPORT_DEFAULT_NAME = "COM1";
        private const int SERIALPORT_DEFAULT_TIMEOUT = 5000;
        private const SerialSpeed SERIALPORT_DEFAULT_SPEED = SerialSpeed.ps9600;
        private const int SERIALPORT_DEFAULT_DATABITS = 8;
        private const bool SERIALPORT_DEFAULT_DTRENABLE = true;
        private const bool SERIALPORT_DEFAULT_RTSENABLE = false;
        private const SerialHandshake SERIALPORT_DEFAULT_HANDSHAKE = SerialHandshake.None;
        private const SerialParity SERIALPORT_DEFAULT_PARITY = SerialParity.None;
        private const SerialStopBits SERIALPORT_DEFAULT_STOPBITS = SerialStopBits.One;
        private const int AVAILABLE_PORTS_SERIAL_TIMEOUT = 500; // Number of milliseconds to allow for a port to respond to the open command during available port scanning
        private const int AVAILABLE_PORTS_SERIAL_TIMEOUT_REPORT_THRESHOLD = 1000; // Number of milliseconds above which to report a long open time during available port scanning

        // 18/3/12 Peter - Experimental change in behaviour of read routines that may address serial issues with some drivers, added at Chris Rowland's suggestion
        // This new switch is initialised in the state that the Platform has been using since Platform 5.5
        // The new behaviour is enabled by creating a ReadPolling value in the COMPortSettings key with the contents True, anything else results in normal behaviour
        private const bool SERIALPORT_DEFAULT_POLLING = false;
        private const string SERIAL_READ_POLLING = "ReadPolling";
        private bool UseReadPolling = SERIALPORT_DEFAULT_POLLING;

        private WaitType TypeOfWait = Serial.WaitType.ManualResetEvent;


        private enum SerialCommandType
        {
            AvailableCOMPorts,
            ClearBuffers,
            Connected,
            Receive,
            Receivebyte,
            ReceiveCounted,
            ReceiveCountedBinary,
            ReceiveTerminated,
            ReceiveTerminatedBinary,
            ReceiveTimeout,
            ReceiveTimeoutMs,
            Transmit,
            TransmitBinary
        }

        internal enum WaitType
        {
            ManualResetEvent,
            Sleep,
            WaitForSingleObject
        }

        public Serial()
        {
            string TraceFileName = "";
            int WorkerThreads, CompletionThreads;

            SerSemaphore = new System.Threading.Semaphore(1, 1); // Create a new semaphore to control access to the serial port

            m_Connected = false; // Set inital values
            m_PortName = SERIALPORT_DEFAULT_NAME;
            m_ReceiveTimeout = SERIALPORT_DEFAULT_TIMEOUT;
            m_Speed = SERIALPORT_DEFAULT_SPEED;
            m_DataBits = SERIALPORT_DEFAULT_DATABITS;
            m_DTREnable = SERIALPORT_DEFAULT_DTRENABLE;
            m_RTSEnable = SERIALPORT_DEFAULT_RTSENABLE;
            m_Handshake = SERIALPORT_DEFAULT_HANDSHAKE;
            m_Parity = SERIALPORT_DEFAULT_PARITY;
            m_StopBits = SERIALPORT_DEFAULT_STOPBITS;

            TextEncoding = System.Text.Encoding.GetEncoding(SERIALPORT_ENCODING); // Initialise text encoding for use by transmitbinary
            try
            {
                SerialProfile = new RegistryAccess(); // Profile class that can retrieve the value of tracefile
                TraceFileName = SerialProfile.GetProfile("", SERIAL_FILE_NAME_VARNAME);
                Logger = new TraceLogger(TraceFileName, "Serial");
                if (TraceFileName != "")
                    Logger.Enabled = true;

                // Get debug trace level on / off
                DebugTrace = GetBool(SERIAL_TRACE_DEBUG, SERIAL_TRACE_DEBUG_DEFAULT);

                // Get the type of wait to use
                TypeOfWait = GetWaitType(SERIAL_WAIT_TYPE, SERIAL_WAIT_TYPE_DEFAULT);
                LogMessage("New", "Worker thread synchronisation by: " + TypeOfWait.ToString());

                ThreadPool.GetMinThreads(out WorkerThreads, out CompletionThreads);
                if (DebugTrace)
                    Logger.LogMessage("New", "Minimum Threads: " + WorkerThreads + " " + CompletionThreads);
                ThreadPool.GetMaxThreads(out WorkerThreads, out CompletionThreads);
                if (DebugTrace)
                    Logger.LogMessage("New", "Maximum Threads: " + WorkerThreads + " " + CompletionThreads);
                ThreadPool.GetAvailableThreads(out WorkerThreads, out CompletionThreads);
                if (DebugTrace)
                    Logger.LogMessage("New", "Available Threads: " + WorkerThreads + " " + CompletionThreads);
            }
            catch (Exception ex)
            {
                Interaction.MsgBox("Serial:New exception " + ex.ToString());
            }
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        /// <summary>
        ///     ''' Disposes of resources used by the profile object
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Serial()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
            base.Finalize();
        }

        /// <summary>
        ///     ''' Disposes of resources used by the profile object - called by IDisposable interface
        ///     ''' </summary>
        ///     ''' <param name="disposing"></param>
        ///     ''' <remarks></remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (!SerialProfile == null)
                {
                    try
                    {
                        SerialProfile.Dispose();
                    }
                    catch
                    {
                    }
                    SerialProfile = null;
                }
                if (!m_Port == null)
                {
                    try
                    {
                        m_Port.Dispose();
                    }
                    catch
                    {
                    }
                    m_Port = null;
                }
                if (!Logger == null)
                {
                    try
                    {
                        Logger.Dispose();
                    }
                    catch
                    {
                    }
                    Logger = null;
                }
                if (!SerSemaphore == null)
                {
                    try
                    {
                        SerSemaphore.Release();
                    }
                    catch
                    {
                    }
                    try
                    {
                        SerSemaphore.Close();
                    }
                    catch
                    {
                    }
                    SerSemaphore = null;
                }
            }

            this.disposed = true;
        }

        /// <summary>
        ///     ''' Gets or sets the number of data bits in each byte
        ///     ''' </summary>
        ///     ''' <value>The number of data bits in each byte, default is 8 data bits</value>
        ///     ''' <returns>Integer number of data bits in each byte</returns>
        ///     ''' <exception cref="ArgumentOutOfRangeException">The data bits value is less than 5 or more than 8</exception>
        ///     ''' <remarks></remarks>
        public int DataBits
        {
            get
            {
                return m_DataBits;
            }
            set
            {
                m_DataBits = NumDataBits;
                Logger.LogMessage("DataBits", "Set to: " + NumDataBits.ToString());
            }
        }

        /// <summary>
        ///     ''' Gets or sets the state of the DTR line
        ///     ''' </summary>
        ///     ''' <value>The state of the DTR line, default is enabled</value>
        ///     ''' <returns>Boolean true/false indicating enabled/disabled</returns>
        ///     ''' <remarks></remarks>
        public bool DTREnable
        {
            get
            {
                return m_DTREnable;
            }
            set
            {
                m_DTREnable = Enabled;
                Logger.LogMessage("DTREnable", "Set to: " + Enabled.ToString());
            }
        }

        /// <summary>
        ///     ''' Gets or sets use of the RTS handshake control line
        ///     ''' </summary>
        ///     ''' <value>The state of RTS line use, default is disabled (false)</value>
        ///     ''' <returns>Boolean true/false indicating RTS line use enabled/disabled</returns>
        ///     ''' <remarks>By default the serial component will not drive the RTS line. If RTSEnable is true, the RTS line will be raised before
        ///     ''' characters are sent. Please also see the associated <see cref="Handshake"/> property.</remarks>
        public bool RTSEnable
        {
            get
            {
                return m_RTSEnable;
            }
            set
            {
                m_RTSEnable = Enabled;
                Logger.LogMessage("RTSEnable", "Set to: " + Enabled.ToString());
            }
        }

        /// <summary>
        ///     ''' Gets or sets the type of serial handshake used on the serial link
        ///     ''' </summary>
        ///     ''' <value>The type of flow control handshake used on the serial line, default is none</value>
        ///     ''' <returns>One of the SerialHandshake enumeration values</returns>
        ///     ''' <remarks>Use of the RTS line can additionally be controlled by the <see cref="RTSEnable"/> property.</remarks>
        public SerialHandshake Handshake
        {
            get
            {
                return m_Handshake;
            }
            set
            {
                m_Handshake = HandshakeType;
                Logger.LogMessage("HandshakeType", "Set to: " + Enum.GetName(typeof(SerialHandshake), HandshakeType));
            }
        }

        /// <summary>
        ///     ''' Gets or sets the type of parity check used over the serial link
        ///     ''' </summary>
        ///     ''' <value>The type of parity check used over the serial link, default none</value>
        ///     ''' <returns>One of the SerialParity enumeration values</returns>
        ///     ''' <remarks></remarks>
        public SerialParity Parity
        {
            get
            {
                return m_Parity;
            }
            set
            {
                m_Parity = ParityType;
                Logger.LogMessage("Parity", "Set to: " + Enum.GetName(typeof(SerialParity), ParityType));
            }
        }

        /// <summary>
        ///     ''' Gets or sets the number of stop bits used on the serial link
        ///     ''' </summary>
        ///     ''' <value>the number of stop bits used on the serial link, default 1</value>
        ///     ''' <returns>One of the SerialStopBits enumeration values</returns>
        ///     ''' <remarks></remarks>
        public SerialStopBits StopBits
        {
            get
            {
                return m_StopBits;
            }
            set
            {
                m_StopBits = NumStopBits;
                Logger.LogMessage("NumStopBits", "Set to: " + Enum.GetName(typeof(SerialStopBits), NumStopBits));
            }
        }

        /// <summary>
        ///     ''' Gets or sets the connected state of the ASCOM serial port.
        ///     ''' </summary>
        ///     ''' <value>Connected state of the serial port.</value>
        ///     ''' <returns><c>True</c> if the serial port is connected.</returns>
        ///     ''' <remarks>Set this property to True to connect to the serial (COM) port. You can read the property to determine if the object is connected. </remarks>
        ///     ''' <exception cref="Exceptions.InvalidValueException">Throws this exception if the requested 
        ///     ''' COM port does not exist.</exception>
        public bool Connected
        {
            get
            {
                return m_Connected;
            }
            set
            {
                ThreadData TData = new ThreadData();
                Stopwatch TS = new Stopwatch();

                // set the RTSEnable and DTREnable states from the registry,
                // NOTE this overrides any other settings, but only if it's set.
                string buf;
                bool b, ForcedRTS, ForcedDTR;

                TS = Stopwatch.StartNew(); // Initialise stopwatch

                // Foorce RTS if required
                buf = SerialProfile.GetProfile(SERIALPORT_COM_PORT_SETTINGS + @"\" + m_PortName, "RTSEnable");
                if (bool.TryParse(buf, out b))
                {
                    m_RTSEnable = b;
                    ForcedRTS = true;
                }

                // Force DTR if required
                buf = SerialProfile.GetProfile(SERIALPORT_COM_PORT_SETTINGS + @"\" + m_PortName, "DTREnable");
                if (bool.TryParse(buf, out b))
                {
                    m_DTREnable = b;
                    ForcedDTR = true;
                }

                // Force Read Polling if required
                buf = SerialProfile.GetProfile(SERIALPORT_COM_PORT_SETTINGS, SERIAL_READ_POLLING);
                if (bool.TryParse(buf, out b))
                    UseReadPolling = b;

                try
                {
                    Logger.LogMessage("Set Connected", Connecting.ToString());
                    if (Connecting)
                    {
                        Logger.LogMessage("Set Connected", "Using COM port: " + m_PortName + " Baud rate: " + m_Speed.ToString() + " Timeout: " + m_ReceiveTimeout.ToString() + " DTR: " + m_DTREnable.ToString() + " ForcedDTR: " + ForcedDTR.ToString() + " RTS: " + m_RTSEnable.ToString() + " ForcedRTS: " + ForcedRTS.ToString() + " Handshake: " + m_Handshake.ToString() + " Encoding: " + SERIALPORT_ENCODING.ToString());
                        Logger.LogMessage("Set Connected", "Transmission format - Bits: " + m_DataBits + " Parity: " + Enum.GetName(m_Parity.GetType(), m_Parity) + " Stop bits: " + Enum.GetName(m_StopBits.GetType(), m_StopBits));
                        if (UseReadPolling)
                            Logger.LogMessage("Set Connected", "Reading COM Port through Read Polling");
                        else
                            Logger.LogMessage("Set Connected", "Reading COM port through Interrupt Handling");
                    }

                    TData.SerialCommand = SerialCommandType.Connected;
                    TData.Connecting = Connecting;
                    TData.TransactionID = GetTransactionID("Set Connected");
                    ThreadPool.QueueUserWorkItem(ConnectedWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("Set Connected", "Completed: " + TData.Completed + ", Duration: " + TS.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!TData.LastException == null)
                        throw TData.LastException;
                    TData = null;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("Set Connected", ex.ToString());
                    throw;
                }
            }
        }

        private void ConnectedWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;

            try
            {
                if (DebugTrace)
                    Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "Connected: " + TData.Connecting);

                if (TData.Connecting)
                {
                    if (!m_Connected)
                    {
                        if (m_Port == null)
                            m_Port = new System.IO.Ports.SerialPort(m_PortName);
                        else
                            m_Port.PortName = m_PortName;

                        // Set up serial port
                        m_Port.BaudRate = m_Speed;
                        m_Port.ReadTimeout = m_ReceiveTimeout;
                        m_Port.WriteTimeout = m_ReceiveTimeout;

                        // Ensure we can get all 256 possible values, default encoding is ASCII that only gives first 127 values
                        m_Port.Encoding = System.Text.Encoding.GetEncoding(SERIALPORT_ENCODING);

                        // Set handshaking and control signals
                        m_Port.DtrEnable = m_DTREnable;
                        m_Port.RtsEnable = m_RTSEnable;
                        m_Port.Handshake = (System.IO.Ports.Handshake)m_Handshake;

                        // Set transmission format
                        m_Port.DataBits = m_DataBits;
                        m_Port.Parity = (System.IO.Ports.Parity)m_Parity;
                        m_Port.StopBits = (System.IO.Ports.StopBits)m_StopBits;

                        // Open port for communication
                        m_Port.Open();
                        m_Connected = true;
                        Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "Port connected OK");
                    }
                    else
                        Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "Port already connected");
                }
                else if (m_Connected)
                {
                    m_Connected = false;
                    m_Port.DiscardOutBuffer();
                    m_Port.DiscardInBuffer();
                    Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "Cleared buffers");
                    m_Port.Close();
                    Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "Closed port");
                    m_Port.Dispose();
                    Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "Port disposed OK");
                }
                else
                    Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "Port already disconnected");
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ConnectedWorker", FormatIDs(TData.TransactionID) + "EXCEPTION: ConnectedWorker - " + ex.Message + " " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Gets or sets the number of the ASCOM serial port (Default is 1, giving COM1 as the serial port name).
        ///     ''' </summary>
        ///     ''' <value>COM port number of the ASCOM serial port.</value>
        ///     ''' <returns>Integer, number of the ASCOM serial port</returns>
        ///     ''' <remarks>This works for serial port names of the form COMnnn. Use PortName if your COM port name does not fit the form COMnnn.</remarks>
        public int Port
        {
            // Set and get port names of the form COMnn. This only works with port names of that form
            // See PortName for a more flexible version that can deal with any valid port name.
            get
            {
                return System.Convert.ToInt32(Strings.Mid(m_PortName, 4));
            }
            set
            {
                m_PortName = "COM" + value.ToString();
                Logger.LogMessage("Port", "Set to: " + value.ToString());
            }
        }

        /// <summary>
        ///     ''' The maximum time that the ASCOM serial port will wait for incoming receive data (seconds, default = 5)
        ///     ''' </summary>
        ///     ''' <value>Integer, serial port timeout in seconds</value>
        ///     ''' <returns>Integer, serial port timeout in seconds.</returns>
        ///     ''' <remarks>The minimum delay timout that can be set through this command is 1 seconds. Use ReceiveTimeoutMs to set a timeout less than 1 second.</remarks>
        ///     ''' <exception cref="InvalidValueException">Thrown when <i>value</i> is invalid (outside the range 1 to 120 seconds.)</exception>
        public int ReceiveTimeout
        {
            // Get and set the receive timeout
            get
            {
                return System.Convert.ToInt32(m_ReceiveTimeout / (double)1000);
            }
            set
            {
                ThreadData TData = new ThreadData();
                try
                {
                    m_ReceiveTimeout = value * 1000; // Save the requested value
                    if (m_Connected)
                    {
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveTimeout", "Start");
                        TData.SerialCommand = SerialCommandType.ReceiveTimeout;
                        TData.TimeoutValue = value;
                        TData.TransactionID = GetTransactionID("ReceiveTimeout");
                        ThreadPool.QueueUserWorkItem(ReceiveTimeoutWorker, TData);
                        WaitForThread(TData); // Sleep this thread until serial operation is complete

                        if (DebugTrace)
                            Logger.LogMessage("ReceiveTimeout", "Completed: " + TData.Completed);
                        if (!(TData.LastException == null))
                            throw TData.LastException;
                        TData = null;
                    }
                    else
                        Logger.LogMessage("ReceiveTimeout", "Set to: " + value + " seconds");
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("ReceiveTimeout", ex.ToString());
                    throw;
                }
            }
        }

        private void ReceiveTimeoutWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;
            int Value;
            try
            {
                Value = TData.TimeoutValue;
                if (DebugTrace)
                    Logger.LogMessage("ReceiveTimeout", FormatIDs(TData.TransactionID) + "Set Start - TimeOut: " + Value + "seconds");

                Value = Value * 1000; // Timeout is measured in milliseconds
                if ((Value <= 0) | (Value > 120000))
                    throw new InvalidValueException("ReceiveTimeout", Strings.Format((double)Value / (double)1000, "0.0"), "1 to 120 seconds");
                m_ReceiveTimeout = Value;
                if (m_Connected)
                {
                    if (DebugTrace)
                        Logger.LogMessage("ReceiveTimeout", FormatIDs(TData.TransactionID) + "Connected so writing to serial port");
                    if (GetSemaphore("ReceiveTimeout", TData.TransactionID))
                    {
                        try
                        {
                            m_Port.WriteTimeout = Value;
                            m_Port.ReadTimeout = Value;
                            Logger.LogMessage("ReceiveTimeout", FormatIDs(TData.TransactionID) + "Written to serial port OK");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage("ReceiveTimeout", FormatIDs(TData.TransactionID) + "EXCEPTION: " + ex.ToString());
                            throw;
                        }
                        finally
                        {
                            ReleaseSemaphore("ReceiveTimeout", TData.TransactionID);
                        }
                    }
                    else
                    {
                        Logger.LogMessage("ReceiveTimeout", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                        throw new SerialPortInUseException("Serial:ReceiveTimeout - unable to get serial port semaphore before timeout.");
                    }
                }
                Logger.LogMessage("ReceiveTimeout", FormatIDs(TData.TransactionID) + "Set to: " + Value / (double)1000 + " seconds");
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveTimeout", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' The maximum time that the ASCOM serial port will wait for incoming receive data (milliseconds, default = 5000)
        ///     ''' </summary>
        ///     ''' <value>Integer, serial port timeout in milli-seconds</value>
        ///     ''' <returns>Integer, serial port timeout in milli-seconds</returns>
        ///     ''' <remarks>If a timeout occurs, an IO timeout error is raised. See ReceiveTimeout for an alternate form 
        ///     ''' using the timeout in seconds. </remarks>
        ///     ''' <exception cref="InvalidValueException">Thrown when <i>value</i> is invalid.</exception>
        public int ReceiveTimeoutMs
        {
            // Get and set the receive timeout in ms
            get
            {
                return m_ReceiveTimeout;
            }
            set
            {
                ThreadData TData = new ThreadData();
                try
                {
                    m_ReceiveTimeout = value; // Save the requested value
                    if (m_Connected)
                    {
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveTimeoutMs", "Start");
                        TData.SerialCommand = SerialCommandType.ReceiveTimeoutMs;
                        TData.TimeoutValueMs = value;
                        TData.TransactionID = GetTransactionID("ReceiveTimeoutMs");
                        ThreadPool.QueueUserWorkItem(ReceiveTimeoutMsWorker, TData);
                        WaitForThread(TData); // Sleep this thread until serial operation is complete

                        if (DebugTrace)
                            Logger.LogMessage("ReceiveTimeoutMs", "Completed: " + TData.Completed);
                        if (!TData.LastException == null)
                            throw TData.LastException;
                        TData = null;
                    }
                    else
                        Logger.LogMessage("ReceiveTimeoutMs", "Set to: " + value + " milli-seconds");
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("ReceiveTimeoutMs", ex.ToString());
                    throw;
                }
            }
        }

        private void ReceiveTimeoutMsWorker(object TDataObject)
        {
            int Value;
            ThreadData TData = (ThreadData)TDataObject;
            try
            {
                Value = TData.TimeoutValueMs;

                if (DebugTrace)
                    Logger.LogMessage("ReceiveTimeoutMs", FormatIDs(TData.TransactionID) + "Set Start - TimeOut: " + Value.ToString() + "mS");

                if ((Value <= 0) | (Value > 120000))
                    throw new InvalidValueException("ReceiveTimeoutMs", Value.ToString(), "1 to 120000 milliseconds");
                m_ReceiveTimeout = Value;
                if (m_Connected)
                {
                    if (DebugTrace)
                        Logger.LogMessage("ReceiveTimeoutMs", FormatIDs(TData.TransactionID) + "Connected so writing to serial port");
                    if (GetSemaphore("ReceiveTimeoutMs", TData.TransactionID))
                    {
                        try
                        {
                            m_Port.WriteTimeout = Value;
                            m_Port.ReadTimeout = Value;
                            Logger.LogMessage("ReceiveTimeoutMs", FormatIDs(TData.TransactionID) + "Written to serial port OK");
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage("ReceiveTimeoutMs", FormatIDs(TData.TransactionID) + "EXCEPTION: " + ex.ToString());
                            throw;
                        }
                        finally
                        {
                            ReleaseSemaphore("ReceiveTimeoutMs", TData.TransactionID);
                        }
                    }
                    else
                    {
                        Logger.LogMessage("ReceiveTimeoutMs", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                        throw new SerialPortInUseException("Serial:ReceiveTimeoutMs - unable to get serial port semaphore before timeout.");
                    }
                }
                Logger.LogMessage("ReceiveTimeoutMs", FormatIDs(TData.TransactionID) + "Set to: " + Value.ToString() + "ms");
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveTimeoutMs", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Gets and sets the baud rate of the ASCOM serial port
        ///     ''' </summary>
        ///     ''' <value>Port speed using the PortSpeed enum</value>
        ///     ''' <returns>Port speed as a SerialSpeed enum value</returns>
        ///     ''' <remarks>This is modelled on the COM component with possible values enumerated in the PortSpeed enum.</remarks>
        public SerialSpeed Speed
        {
            // Get and set the port speed using the portspeed enum
            get
            {
                return m_Speed;
            }
            set
            {
                m_Speed = value;
                Logger.LogMessage("Speed", "Set to: " + Enum.GetName(typeof(SerialSpeed), value));
            }
        }

        /// <summary>
        ///     ''' Clears the ASCOM serial port receive and transmit buffers
        ///     ''' </summary>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <remarks></remarks>
        public void ClearBuffers()
        {
            ThreadData TData = new ThreadData();
            Stopwatch TS = new Stopwatch();

            TS = Stopwatch.StartNew(); // Initialise stopwatch

            try
            {
                if (m_Connected)
                {
                    if (DebugTrace)
                        Logger.LogMessage("ClearBuffers", "Start");
                    TData.SerialCommand = SerialCommandType.ClearBuffers;
                    TData.TransactionID = GetTransactionID("ClearBuffers");
                    ThreadPool.QueueUserWorkItem(ClearBuffersWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("ClearBuffers", "Completed: " + TData.Completed + ", Duration: " + TS.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!TData.LastException == null)
                        throw TData.LastException;
                    TData = null;
                }
                else
                    Logger.LogMessage("ClearBuffers", "***** ClearBuffers ignored because the port is not connected!");
            }
            catch (Exception ex)
            {
                Logger.LogMessage("ClearBuffers", ex.ToString());
                throw;
            }
        }

        private void ClearBuffersWorker(object TDataObject)
        {
            // Clear both comm buffers
            ThreadData TData = (ThreadData)TDataObject;

            try
            {
                if (DebugTrace)
                    Logger.LogMessage("ClearBuffersWorker", FormatIDs(TData.TransactionID) + " " + Thread.CurrentThread.ManagedThreadId + "Start");
                if (GetSemaphore("ClearBuffersWorker", TData.TransactionID))
                {
                    try
                    {
                        if (!(m_Port == null))
                        {
                            if (m_Port.IsOpen)
                            {
                                m_Port.DiscardInBuffer();
                                m_Port.DiscardOutBuffer();
                                Logger.LogMessage("ClearBuffersWorker", FormatIDs(TData.TransactionID) + "Completed");
                            }
                            else
                                Logger.LogMessage("ClearBuffersWorker", FormatIDs(TData.TransactionID) + "Command ignored as port is not open");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("ClearBuffersWorker", FormatIDs(TData.TransactionID) + "EXCEPTION: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("ClearBuffersWorker ", TData.TransactionID);
                    }
                }
                else
                {
                    Logger.LogMessage("ClearBuffersWorker", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ClearBuffersWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Receive at least one text character from the ASCOM serial port
        ///     ''' </summary>
        ///     ''' <returns>The characters received</returns>
        ///     ''' <exception cref="System.TimeoutException">Thrown when a receive timeout occurs.</exception>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks>This method reads all of the characters currently in the serial receive buffer. It will not return 
        ///     ''' unless it reads at least one character. A timeout will cause a TimeoutException to be raised. Use this for 
        ///     ''' text data, as it returns a String. </remarks>
        public string Receive()
        {
            // Return all characters in the receive buffer
            string Result;
            ThreadData TData = new ThreadData();
            Stopwatch TS = new Stopwatch();

            TS = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("Receive", "Start");
                    TData.SerialCommand = SerialCommandType.Receive;
                    TData.TransactionID = GetTransactionID("Receive");
                    ThreadPool.QueueUserWorkItem(ReceiveWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("Receive", "Completed: " + TData.Completed + ", Duration: " + TS.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!(TData.LastException == null))
                        throw TData.LastException;
                    Result = TData.ResultString;
                    TData = null;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("Receive", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("Receive", ex.ToString());
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.Receive command");
            return Result;
        }

        private void ReceiveWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;
            string Received = "";

            try
            {
                if (DebugTrace)
                    Logger.LogMessage("ReceiveWorker", FormatIDs(TData.TransactionID) + "Start");
                if (GetSemaphore("ReceiveWorker", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogStart("ReceiveWorker", FormatIDs(TData.TransactionID) + "< ");
                        Received = ReadChar("ReceiveWorker", TData.TransactionID);
                        Received = Received + m_Port.ReadExisting();
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveWorker", FormatIDs(TData.TransactionID) + "< " + Received);
                        else
                            Logger.LogFinish(Received);
                    }
                    catch (TimeoutException ex)
                    {
                        Logger.LogMessage("ReceiveWorker", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.ToString());
                        throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("ReceiveWorker", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("ReceiveWorker", TData.TransactionID);
                    }
                    TData.ResultString = Received;
                }
                else
                {
                    Logger.LogMessage("ReceiveWorker", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new SerialPortInUseException("Serial:Receive - unable to get serial port semaphore before timeout.");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Receive one binary byte from the ASCOM serial port
        ///     ''' </summary>
        ///     ''' <returns>The received byte</returns>
        ///     ''' <exception cref="System.TimeoutException">Thrown when a receive timeout occurs.</exception>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks>Use this for 8-bit (binary data). If a timeout occurs, a TimeoutException is raised. </remarks>
        public byte ReceiveByte()
        {
            ThreadData TData = new ThreadData();
            byte RetVal;
            Stopwatch TS = new Stopwatch();

            TS = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("ReceiveByte", "Start");
                    TData.SerialCommand = SerialCommandType.Receivebyte;
                    TData.TransactionID = GetTransactionID("ReceiveByte");
                    ThreadPool.QueueUserWorkItem(ReceiveByteWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("ReceiveByte", "Completed: " + TData.Completed + ", Duration: " + TS.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!TData.LastException == null)
                        throw TData.LastException;
                    RetVal = TData.ResultByte;
                    TData = null;
                    return RetVal;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("ReceiveByte", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("ReceiveByte", ex.ToString());
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.ReceiveByte command");
        }

        private void ReceiveByteWorker(object TDataObject)
        {
            // Return a single byte of data
            ThreadData TData = (ThreadData)TDataObject;
            byte RetVal;

            try
            {
                if (DebugTrace)
                    Logger.LogMessage("ReceiveByteWorker", FormatIDs(TData.TransactionID) + "Start");
                if (GetSemaphore("ReceiveByteWorker", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogStart("ReceiveByteWorker", FormatIDs(TData.TransactionID) + "< ");
                        RetVal = ReadByte("ReceiveByteWorker", TData.TransactionID);
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveByteWorker", FormatIDs(TData.TransactionID) + " " + Strings.Chr(RetVal), true);
                        else
                            Logger.LogFinish(Strings.Chr(RetVal), true);
                    }
                    catch (TimeoutException ex)
                    {
                        Logger.LogMessage("ReceiveByteWorker", FormatIDs(TData.TransactionID) + "EXCEPTION: " + ex.ToString());
                        throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("ReceiveByteWorker", FormatIDs(TData.TransactionID) + "EXCEPTION: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("ReceiveByteWorker", TData.TransactionID);
                    }
                    TData.ResultByte = RetVal;
                }
                else
                {
                    Logger.LogMessage("ReceiveByteWorker", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new SerialPortInUseException("Serial:ReceiveByte - unable to get serial port semaphore before timeout.");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveByteWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Receive exactly the given number of characters from the ASCOM serial port and return as a string
        ///     ''' </summary>
        ///     ''' <param name="Count">The number of characters to return</param>
        ///     ''' <returns>String of length "Count" characters</returns>
        ///     ''' <exception cref="System.TimeoutException">Thrown when a receive timeout occurs.</exception>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks>If a timeout occurs a TimeoutException is raised.</remarks>
        public string ReceiveCounted(int Count)
        {
            ThreadData TData = new ThreadData();
            string RetVal;
            Stopwatch TS = new Stopwatch();

            TS = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("ReceiveCounted", "Start");
                    TData.SerialCommand = SerialCommandType.ReceiveCounted;
                    TData.Count = Count;
                    TData.TransactionID = GetTransactionID("ReceiveCounted");
                    ThreadPool.QueueUserWorkItem(ReceiveCountedWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("ReceiveCounted", "Completed: " + TData.Completed + ", Duration: " + TS.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!TData.LastException == null)
                        throw TData.LastException;
                    RetVal = TData.ResultString;
                    TData = null;
                    return RetVal;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("ReceiveCounted", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("ReceiveCounted", ex.ToString());
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.ReceiveCounted command");
        }

        private void ReceiveCountedWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;
            int i;
            string Received = "";

            try
            {
                // Return a fixed number of characters
                if (DebugTrace)
                    Logger.LogMessage("ReceiveCountedWorker", FormatIDs(TData.TransactionID) + "Start - count: " + TData.Count.ToString());
                if (GetSemaphore("ReceiveCountedWorker", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogStart("ReceiveCountedWorker " + TData.Count.ToString(), FormatIDs(TData.TransactionID) + "< ");
                        for (i = 1; i <= TData.Count; i++)
                            Received = Received + ReadChar("ReceiveCountedWorker", TData.TransactionID);
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveCountedWorker", FormatIDs(TData.TransactionID) + "< " + Received);
                        else
                            Logger.LogFinish(Received);
                    }
                    catch (TimeoutException ex)
                    {
                        Logger.LogMessage("ReceiveCountedWorker", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.Message);
                        throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("ReceiveCountedWorker", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.Message);
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("ReceiveCountedWorker", TData.TransactionID);
                    }
                    TData.ResultString = Received;
                }
                else
                {
                    Logger.LogMessage("ReceiveCountedWorker", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new SerialPortInUseException("ReceiveCounted - unable to get serial port semaphore before timeout.");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveCountedWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Receive exactly the given number of characters from the ASCOM serial port and return as a byte array
        ///     ''' </summary>
        ///     ''' <param name="Count">The number of characters to return</param>
        ///     ''' <returns>Byte array of size "Count" elements</returns>
        ///     ''' <exception cref="System.TimeoutException">Thrown when a receive timeout occurs.</exception>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks>
        ///     ''' <para>If a timeout occurs, a TimeoutException is raised. </para>
        ///     ''' <para>This function exists in the COM component but is not documented in the help file.</para>
        ///     ''' </remarks>
        public byte[] ReceiveCountedBinary(int Count)
        {
            byte[] Result;
            ThreadData TData = new ThreadData();
            Stopwatch TS = new Stopwatch();

            TS = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("ReceiveCountedBinary", "Start");
                    TData.SerialCommand = SerialCommandType.ReceiveCountedBinary;
                    TData.Count = Count;
                    TData.TransactionID = GetTransactionID("ReceiveCountedBinary");
                    ThreadPool.QueueUserWorkItem(ReceiveCountedBinaryWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("ReceiveCountedBinary", "Completed: " + TData.Completed + ", Duration: " + TS.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!(TData.LastException == null))
                        throw TData.LastException;
                    Result = TData.ResultByteArray;
                    TData = null;
                    return Result;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("ReceiveCountedBinary", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("ReceiveCountedBinary", ex.ToString());
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.ReceiveCountedBinary command");
        }

        private void ReceiveCountedBinaryWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;
            int i;
            byte[] Received = new byte[1];
            System.Text.Encoding TextEncoding;

            try
            {
                TextEncoding = System.Text.Encoding.GetEncoding(1252);

                if (DebugTrace)
                    Logger.LogMessage("ReceiveCountedBinaryWorker ", FormatIDs(TData.TransactionID) + "Start - count: " + TData.Count.ToString());

                if (GetSemaphore("ReceiveCountedBinaryWorker ", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogStart("ReceiveCountedBinaryWorker " + TData.Count.ToString(), FormatIDs(TData.TransactionID) + "< ");
                        for (i = 0; i <= TData.Count - 1; i++)
                        {
                            var oldReceived = Received;
                            Received = new byte[i + 1];
                            if (oldReceived != null)
                                Array.Copy(oldReceived, Received, Math.Min(i + 1, oldReceived.Length));
                            Received[i] = ReadByte("ReceiveCountedBinaryWorker ", TData.TransactionID);
                        }
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveCountedBinaryWorker ", FormatIDs(TData.TransactionID) + "< " + TextEncoding.GetString(Received), true);
                        else
                            Logger.LogFinish(TextEncoding.GetString(Received), true);
                    }
                    catch (TimeoutException ex)
                    {
                        Logger.LogMessage("ReceiveCountedBinaryWorker ", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(TextEncoding.GetString(Received)) + "EXCEPTION: " + ex.Message);
                        throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("ReceiveCountedBinaryWorker ", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(TextEncoding.GetString(Received)) + "EXCEPTION: " + ex.Message);
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("ReceiveCountedBinaryWorker ", TData.TransactionID);
                    }
                    TData.ResultByteArray = Received;
                }
                else
                {
                    Logger.LogMessage("ReceiveCountedBinaryWorker ", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveCountedBinaryWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Receive characters from the ASCOM serial port until the given terminator string is seen
        ///     ''' </summary>
        ///     ''' <param name="Terminator">The character string that indicates end of message</param>
        ///     ''' <returns>Received characters including the terminator string</returns>
        ///     ''' <exception cref="System.TimeoutException">Thrown when a receive timeout occurs.</exception>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks>If a timeout occurs, a TimeoutException is raised.</remarks>
        public string ReceiveTerminated(string Terminator)
        {
            ThreadData TData = new ThreadData();
            string RetVal;

            ts = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("ReceiveTerminated", "Start");
                    TData.SerialCommand = SerialCommandType.ReceiveTerminated;
                    TData.Terminator = Terminator;
                    TData.TransactionID = GetTransactionID("ReceiveTerminated");
                    ThreadPool.QueueUserWorkItem(ReceiveTerminatedWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("ReceiveTerminated", "Completed: " + TData.Completed + ", Duration: " + ts.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!TData.LastException == null)
                        throw TData.LastException;
                    RetVal = TData.ResultString;
                    TData = null;
                    return RetVal;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("ReceiveTerminated", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("ReceiveTerminated", ex.ToString());
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.ReceiveTerminated command");
        }

        private void ReceiveTerminatedWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;
            bool Terminated;
            int tLen;
            string Received = "";

            try
            {
                // Return all characters up to and including a specified terminator string
                if (DebugTrace)
                    Logger.LogMessage("ReceiveTerminatedWorker ", FormatIDs(TData.TransactionID) + "Start - terminator: \"" + TData.Terminator.ToString() + "\"");
                // Check for bad terminator string
                if (TData.Terminator == "")
                    throw new InvalidValueException("ReceiveTerminated Terminator", "Null or empty string", "Character or character string");

                if (GetSemaphore("ReceiveTerminatedWorker ", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogStart("ReceiveTerminatedWorker " + TData.Terminator.ToString(), FormatIDs(TData.TransactionID) + "< ");
                        tLen = Strings.Len(TData.Terminator);
                        do
                        {
                            Received = Received + ReadChar("ReceiveTerminatedWorker ", TData.TransactionID);
                            if (Strings.Len(Received) >= tLen)
                            {
                                if (Strings.Right(Received, tLen) == TData.Terminator)
                                    Terminated = true;
                            }
                        }
                        while (!Terminated)// Build up the string one char at a time
    ;
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveTerminatedWorker ", FormatIDs(TData.TransactionID) + "< " + Received);
                        else
                            Logger.LogFinish(Received);
                    }
                    catch (InvalidValueException ex)
                    {
                        Logger.LogMessage("ReceiveTerminatedWorker ", FormatIDs(TData.TransactionID) + "EXCEPTION - Terminator cannot be a null string");
                        throw;
                    }
                    catch (TimeoutException ex)
                    {
                        Logger.LogMessage("ReceiveTerminatedWorker ", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.ToString());
                        throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("ReceiveTerminatedWorker ", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("ReceiveTerminatedWorker ", TData.TransactionID);
                    }
                    TData.ResultString = Received;
                }
                else
                {
                    Logger.LogMessage("ReceiveTerminatedWorker", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new SerialPortInUseException("Serial:ReceiveTerminated - unable to get serial port semaphore before timeout.");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveTerminatedWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Receive characters from the ASCOM serial port until the given terminator bytes are seen, return as a byte array
        ///     ''' </summary>
        ///     ''' <param name="TerminatorBytes">Array of bytes that indicates end of message</param>
        ///     ''' <returns>Byte array of received characters</returns>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks>
        ///     ''' <para>If a timeout occurs, a TimeoutException is raised.</para>
        ///     ''' <para>This function exists in the COM component but is not documented in the help file.</para>
        ///     ''' </remarks>
        public byte[] ReceiveTerminatedBinary(byte[] TerminatorBytes)
        {
            byte[] Result;
            ThreadData TData = new ThreadData();

            ts = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("ReceiveTerminatedBinary", "Start");
                    TData.SerialCommand = SerialCommandType.ReceiveCounted;
                    TData.TerminatorBytes = TerminatorBytes;
                    TData.TransactionID = GetTransactionID("ReceiveTerminatedBinary");
                    ThreadPool.QueueUserWorkItem(ReceiveTerminatedBinaryWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("ReceiveTerminatedBinary", "Completed: " + TData.Completed + ", Duration: " + ts.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!(TData.LastException == null))
                        throw TData.LastException;
                    Result = TData.ResultByteArray;
                    TData = null;
                    return Result;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("ReceiveTerminatedBinary", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("ReceiveTerminatedBinary", ex.ToString());
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.ReceiveTerminatedBinary command");
        }

        private void ReceiveTerminatedBinaryWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;
            bool Terminated;
            int tLen;
            string Terminator;
            string Received = "";
            System.Text.Encoding TextEncoding;

            try
            {
                TextEncoding = System.Text.Encoding.GetEncoding(1252);
                Terminator = TextEncoding.GetString(TData.TerminatorBytes);

                if (DebugTrace)
                    Logger.LogMessage("ReceiveTerminatedBinaryWorker ", FormatIDs(TData.TransactionID) + "Start - terminator: \"" + Terminator.ToString() + "\"");
                // Check for bad terminator string
                if (Terminator == "")
                    throw new InvalidValueException("ReceiveTerminatedBinary Terminator", "Null or empty string", "Character or character string");

                if (GetSemaphore("ReceiveTerminatedBinaryWorker ", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogStart("ReceiveTerminatedBinaryWorker " + Terminator.ToString(), FormatIDs(TData.TransactionID) + "< ");

                        tLen = Strings.Len(Terminator);
                        do
                        {
                            Received = Received + ReadChar("ReceiveTerminatedBinaryWorker ", TData.TransactionID);
                            if (Strings.Len(Received) >= tLen)
                            {
                                if (Strings.Right(Received, tLen) == Terminator)
                                    Terminated = true;
                            }
                        }
                        while (!Terminated)// Build up the string one char at a time
    ;
                        if (DebugTrace)
                            Logger.LogMessage("ReceiveTerminatedBinaryWorker ", FormatIDs(TData.TransactionID) + "< " + Received, true);
                        else
                            Logger.LogFinish(Received, true);
                    }
                    catch (InvalidValueException ex)
                    {
                        Logger.LogMessage("ReceiveTerminatedBinaryWorker ", FormatIDs(TData.TransactionID) + "EXCEPTION - Terminator cannot be a null string");
                        throw;
                    }
                    catch (TimeoutException ex)
                    {
                        Logger.LogMessage("ReceiveTerminatedBinaryWorker ", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.ToString());
                        throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("ReceiveTerminatedBinaryWorker ", FormatIDs(TData.TransactionID) + " " + FormatRXSoFar(Received) + "EXCEPTION: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("ReceiveTerminatedBinaryWorker ", TData.TransactionID);
                    }
                    TData.ResultByteArray = TextEncoding.GetBytes(Received);
                }
                else
                {
                    Logger.LogMessage("ReceiveTerminatedBinaryWorker ", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    // Throw New SerialPortInUseException("Serial:ReceiveTerminatedBinary - unable to get serial port semaphore before timeout.")
                    throw new System.Runtime.InteropServices.COMException(TIMEOUT_MESSAGE, TIMEOUT_NUMBER);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("ReceiveTerminatedBinaryWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Transmits a string through the ASCOM serial port
        ///     ''' </summary>
        ///     ''' <param name="Data">String to transmit</param>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks></remarks>
        public void Transmit(string Data)
        {
            ThreadData TData = new ThreadData();

            ts = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("Transmit", "Start");
                    TData.SerialCommand = SerialCommandType.Transmit;
                    TData.TransmitString = Data;
                    TData.TransactionID = GetTransactionID("Transmit");
                    ThreadPool.QueueUserWorkItem(TransmitWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("Transmit", "Completed: " + TData.Completed + ", Duration: " + ts.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!TData.LastException == null)
                        throw TData.LastException;
                    TData = null;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("Transmit", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("Transmit", ex.ToString());
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.Transmit command");
        }

        private void TransmitWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;

            try
            {
                // Send provided string to the serial port
                if (DebugTrace)
                    Logger.LogMessage("TransmitWorker", FormatIDs(TData.TransactionID) + "> " + TData.TransmitString);
                if (GetSemaphore("TransmitWorker", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogMessage("TransmitWorker", FormatIDs(TData.TransactionID) + "> " + TData.TransmitString);
                        m_Port.Write(TData.TransmitString);
                        if (DebugTrace)
                            Logger.LogMessage("TransmitWorker", FormatIDs(TData.TransactionID) + "Completed");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("TransmitWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("TransmitWorker", TData.TransactionID);
                    }
                }
                else
                {
                    Logger.LogMessage("TransmitWorker", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new SerialPortInUseException("Serial:Transmit - unable to get serial port semaphore before timeout.");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("TransmitWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Transmit an array of binary bytes through the ASCOM serial port 
        ///     ''' </summary>
        ///     ''' <param name="Data">Byte array to transmit</param>
        ///     ''' <exception cref="SerialPortInUseException">Thrown when unable to acquire the serial port</exception>
        ///     ''' <exception cref="NotConnectedException">Thrown when this command is used before setting Connect = True</exception>
        ///     ''' <remarks></remarks>
        public void TransmitBinary(byte[] Data)
        {
            string Result;
            ThreadData TData = new ThreadData();

            ts = Stopwatch.StartNew(); // Initialise stopwatch

            if (m_Connected)
            {
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("TransmitBinary", "Start");
                    TData.SerialCommand = SerialCommandType.ReceiveCounted;
                    TData.TransmitBytes = Data;
                    TData.TransactionID = GetTransactionID("TransmitBinary");
                    ThreadPool.QueueUserWorkItem(TransmitBinaryWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("TransmitBinary", "Completed: " + TData.Completed + ", Duration: " + ts.Elapsed.TotalMilliseconds.ToString("0.0"));
                    if (!(TData.LastException == null))
                        throw TData.LastException;
                    Result = TData.ResultString;
                    TData = null;
                }
                catch (TimeoutException ex)
                {
                    Logger.LogMessage("TransmitBinary", ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("TransmitBinary", ex.ToString());
                    // Throw New ASCOM.Utilities.Exceptions.SerialPortInUseException("Serial Port Issue", ex)
                    throw;
                }
            }
            else
                throw new ASCOM.NotConnectedException("Serial port is not connected - you cannot use the Serial.TransmitBinary command");
        }

        private void TransmitBinaryWorker(object TDataObject)
        {
            ThreadData TData = (ThreadData)TDataObject;
            string TxString;

            try
            {
                TxString = TextEncoding.GetString(TData.TransmitBytes);
                if (DebugTrace)
                    Logger.LogMessage("TransmitBinaryWorker ", FormatIDs(TData.TransactionID) + "> " + TxString + " (HEX" + MakeHex(TxString) + ") ");

                if (GetSemaphore("TransmitBinaryWorker ", TData.TransactionID))
                {
                    try
                    {
                        if (!DebugTrace)
                            Logger.LogMessage("TransmitBinaryWorker", FormatIDs(TData.TransactionID) + "> " + TxString + " (HEX" + MakeHex(TxString) + ") ");
                        m_Port.Write(TData.TransmitBytes, 0, TData.TransmitBytes.Length);
                        if (DebugTrace)
                            Logger.LogMessage("TransmitBinaryWorker ", FormatIDs(TData.TransactionID) + "Completed");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("TransmitBinaryWorker ", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                        throw;
                    }
                    finally
                    {
                        ReleaseSemaphore("TransmitBinaryWorker ", TData.TransactionID);
                    }
                }
                else
                {
                    Logger.LogMessage("TransmitBinaryWorker ", FormatIDs(TData.TransactionID) + "Throwing SerialPortInUse exception");
                    throw new SerialPortInUseException("TransmitBinary - unable to get serial port semaphore before timeout.");
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("TransmitBinaryWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                TData.ThreadCompleted();
            }
        }

        // These are additional funcitons not provided in the original Helper and Helper2 components
        /// <summary>
        ///     ''' Sets the ASCOM serial port name as a string
        ///     ''' </summary>
        ///     ''' <value>Serial port name to be used</value>
        ///     ''' <returns>Current serial port name</returns>
        ///     ''' <remarks>This property allows any serial port name to be used, even if it doesn't conform to a format of COMnnn
        ///     ''' If the required port name is of the form COMnnn then Serial.Port=nnn and Serial.PortName="COMnnn" are 
        ///     ''' equivalent.</remarks>
        public string PortName
        {
            // Allows the full COM port name to be read and set. This works for COM ports of any name
            get
            {
                return m_PortName;
            }
            set
            {
                m_PortName = value;
                Logger.LogMessage("PortName", "Set to: " + value);
            }
        }

        /// <summary>
        ///     ''' Returns a list of all available ASCOM serial ports with COMnnn ports sorted into ascending port number order
        ///     ''' </summary>
        ///     ''' <value>String array of available serial ports</value>
        ///     ''' <returns>A string array of available serial ports</returns>
        ///     ''' <remarks><b>Update in platform 6.0.0.0</b> This call uses the .NET Framework to retrieve available 
        ///     ''' COM ports and this has been found not to return names of some USB serial adapters. Additional 
        ///     ''' code has been added to attempt to open all COM ports up to COM32. Any ports that can be 
        ///     ''' successfully opened are now returned alongside the ports returned by the .NET call.
        ///     ''' <para>If this new approach still does not detect a COM port it can be forced to appear in the list by adding its name
        ///     ''' as a string entry in the ForceCOMPorts key of the ASCOM Profile. In the event that this scanning causes issues, a COM port can be 
        ///     ''' omitted from the scan by adding its name as a string entry in the IgnoreCOMPorts key of the ASCOM Profile.</para></remarks>
        public string[] AvailableCOMPorts
        {
            // Returns a list of all available COM ports sorted into ascending COM port number order
            get
            {
                Generic.List<string> PortNames;
                PortNameComparer myPortNameComparer = new PortNameComparer();
                ThreadData TData = new ThreadData();

                try
                {
                    // Get the current lists of forced and ignored COM ports
                    ForcedCOMPorts = SerialProfile.EnumProfile(SERIAL_FORCED_COMPORTS_VARNAME);
                    try
                    {
                        ForcedCOMPorts.Remove("");
                    }
                    catch
                    {
                    } // Remove the default value
                    foreach (Generic.KeyValuePair<string, string> kvp in ForcedCOMPorts)
                        Logger.LogMessage("AvailableCOMPorts", "Forcing COM port " + kvp.Key + " " + kvp.Value + " to appear");

                    IgnoredCOMPorts = SerialProfile.EnumProfile(SERIAL_IGNORE_COMPORTS_VARNAME);
                    try
                    {
                        IgnoredCOMPorts.Remove("");
                    }
                    catch
                    {
                    } // Remove the default value
                    foreach (Generic.KeyValuePair<string, string> kvp in IgnoredCOMPorts)
                        Logger.LogMessage("AvailableCOMPorts", "Ignoring COM port " + kvp.Key + " " + kvp.Value);
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("AvailableCOMPorts Profile", ex.ToString());
                }

                // Find COM port names from the framework
                if (DebugTrace)
                    Logger.LogMessage("AvailableCOMPorts", "Entered AvailableCOMPorts");
                PortNames = new Generic.List<string>(SerialPort.GetPortNames());
                if (DebugTrace)
                    Logger.LogMessage("AvailableCOMPorts", "Retrieved port names using SerialPort.GetPortNames");

                // Add any forced ports that aren't already in the list
                foreach (Generic.KeyValuePair<string, string> PortName in ForcedCOMPorts)
                {
                    if (!PortNames.Contains(Trim(PortName.Key)))
                        PortNames.Add(Trim(PortName.Key));
                }

                // Add any ignored ports that aren't already in the list so that these are not scanned
                foreach (Generic.KeyValuePair<string, string> PortName in IgnoredCOMPorts)
                {
                    if (!PortNames.Contains(Trim(PortName.Key)))
                        PortNames.Add(Trim(PortName.Key));
                }

                // Some ports aren't returned by the framework method so probe for them
                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("AvailableCOMPorts", "Start");
                    TData.SerialCommand = SerialCommandType.AvailableCOMPorts;
                    TData.AvailableCOMPorts = PortNames;
                    TData.TransactionID = GetTransactionID("AvailableCOMPorts");
                    ThreadPool.QueueUserWorkItem(AvailableCOMPortsWorker, TData);
                    WaitForThread(TData); // Sleep this thread until serial operation is complete

                    if (DebugTrace)
                        Logger.LogMessage("AvailableCOMPorts", "Completed: " + TData.Completed);
                    if (!(TData.LastException == null))
                        throw TData.LastException;
                    PortNames = TData.AvailableCOMPorts;
                    TData = null;
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("AvailableCOMPorts", ex.ToString());
                    throw;
                }

                // Now remove the ports that are to be ignored and log the rest
                foreach (Generic.KeyValuePair<string, string> PortName in IgnoredCOMPorts)
                {
                    if (PortNames.Contains(Trim(PortName.Key)))
                        PortNames.Remove(Trim(PortName.Key));
                }

                PortNames.Sort(myPortNameComparer); // Use specialised comparer to get the sort order right

                foreach (string PortName in PortNames)
                    Logger.LogMessage("AvailableCOMPorts", PortName);
                if (DebugTrace)
                    Logger.LogMessage("AvailableCOMPorts", "Finished");
                return PortNames.ToArray;
            }
        }

        private void AvailableCOMPortsWorker(object TDataObject)
        {
            // Test for available COM ports
            ThreadData TData = (ThreadData)TDataObject;
            int PortNumber;
            string PortName = "";
            Generic.List<string> RetVal;
            Stopwatch SWatch = new Stopwatch();
            SerialPort SerPort = new SerialPort();

            try
            {
                if (DebugTrace)
                    Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Started");

                try
                {
                    if (DebugTrace)
                        Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Port probe started");
                    RetVal = TData.AvailableCOMPorts;
                    SerPort.ReadTimeout = AVAILABLE_PORTS_SERIAL_TIMEOUT; // Set low timeouts so the process completes quickly
                    SerPort.WriteTimeout = AVAILABLE_PORTS_SERIAL_TIMEOUT;

                    for (PortNumber = 0; PortNumber <= 32; PortNumber++)
                    {
                        try
                        {
                            PortName = "COM" + PortNumber.ToString();
                            if (!RetVal.Contains(PortName))
                            {
                                if (DebugTrace)
                                    Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Starting to probe port " + PortNumber);

                                SWatch.Reset(); // Reset and start the timer stopwatch
                                SWatch.Start();
                                SerPort.PortName = PortName; // Set the port name and attempt to open it
                                SerPort.Open();
                                SerPort.Close();
                                SWatch.Stop(); // Stop the timer

                                // If we get here without an exception, the port must exist, so check whether it took a long time and report if it did
                                if (SWatch.ElapsedMilliseconds >= AVAILABLE_PORTS_SERIAL_TIMEOUT_REPORT_THRESHOLD)
                                    Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Probing port " + PortName + " took  a long time: " + SWatch.ElapsedMilliseconds + "ms");

                                // Its real so add it to the list, i.e. no exception was generated!
                                RetVal.Add(PortName);
                                if (DebugTrace)
                                    Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Port " + PortNumber + " exists, elapsed time: " + SWatch.ElapsedMilliseconds + "ms");
                            }
                            else if (DebugTrace)
                                Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Skiping probe as port  " + PortName + " is already known to exist");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Port exists but is in use so add it to the list
                            RetVal.Add(PortName);
                            if (DebugTrace)
                                Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Port " + PortNumber + " UnauthorisedAccessException, elapsed time: " + SWatch.ElapsedMilliseconds + "ms");
                        }
                        catch (Exception ex)
                        {
                            if (DebugTrace)
                                Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Port " + PortNumber + " Exception, found is, elapsed time: " + SWatch.ElapsedMilliseconds + "ms " + ex.Message);
                        }
                    }
                    TData.AvailableCOMPorts = RetVal; // Save updated array for return to the calling thread

                    if (DebugTrace)
                        Logger.LogMessage("AvailableCOMPortsWorker ", FormatIDs(TData.TransactionID) + "Completed");
                }
                catch (Exception ex)
                {
                    Logger.LogMessage("AvailableCOMPortsWorker ", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    Logger.LogMessage("AvailableCOMPortsWorker", FormatIDs(TData.TransactionID) + "Exception: " + ex.ToString());
                }
                catch
                {
                }
                try
                {
                    TData.LastException = ex;
                }
                catch
                {
                }
            }
            finally
            {
                try
                {
                    SerPort.Dispose();
                }
                catch
                {
                } // Dispose of the COM port
                try
                {
                    SerPort = null;
                }
                catch
                {
                }
                TData.ThreadCompleted();
            }
        }

        /// <summary>
        ///     ''' Adds a message to the ASCOM serial trace file
        ///     ''' </summary>
        ///     ''' <param name="Caller">String identifying the module logging the message</param>
        ///     ''' <param name="Message">Message text to be logged.</param>
        ///     ''' <remarks>
        ///     ''' <para>This can be called regardless of whether logging is enabled</para>
        ///     ''' </remarks>
        public void LogMessage(string Caller, string Message)
        {
            // Allows a driver to include its own comments in the serial log
            Logger.LogMessage(Caller, Message);
        }

        private string FormatRXSoFar(string p_Chars)
        {
            if (p_Chars != "")
                return p_Chars + " ";
            return "";
        }

        /// <summary>
        ///     ''' Translates a supplied string into hex characters
        ///     ''' </summary>
        ///     ''' <param name="p_Msg">The string to convert</param>
        ///     ''' <returns>A string with each character represented as [xx]</returns>
        ///     ''' <remarks></remarks>
        private string MakeHex(string p_Msg)
        {
            string l_Msg = "";
            int i, CharNo;
            // Present all characters in [0xHH] format
            for (i = 1; i <= Strings.Len(p_Msg); i++)
            {
                CharNo = Strings.Asc(Strings.Mid(p_Msg, i, 1));
                l_Msg = l_Msg + "[" + Strings.Right("00" + Conversion.Hex(CharNo), 2) + "]";
            }
            return l_Msg;
        }

        private bool GetSemaphore(string p_Caller, long MyCallNumber) // Gets the serial control semaphore and returns success or failure
        {
            bool GotSemaphore;
            DateTime StartTime;
            StartTime = DateTime.Now; // Save the current start time so we will wait longer than a port timeout before giving up on getting the semaphore
            try
            {
                if (DebugTrace)
                    Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "Entered GetSemaphore ");
                GotSemaphore = SerSemaphore.WaitOne(m_ReceiveTimeout + 2000, false); // Peter 17/11/09 Converted to use the framework 2.0 rather than 3.5 call
                if (DebugTrace)
                {
                    if (GotSemaphore)
                        Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "Got Semaphore OK");
                    else
                        Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "Failed to get Semaphore, returning: False");
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage("GetSemaphore", MyCallNumber.ToString() + Thread.CurrentThread.ManagedThreadId + " " + "Exception: " + ex.ToString() + " " + ex.StackTrace);
            }
            // Return False 'Didn't get the semaphore so return false
            return GotSemaphore;
        }

        private void ReleaseSemaphore(string p_Caller, long MyCallNumber) // Release the semaphore if we have it but don't error if there is a problem
        {
            try
            {
                if (DebugTrace)
                    Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "Entered ReleaseSemaphore " + Thread.CurrentThread.ManagedThreadId);
                SerSemaphore.Release();
                if (DebugTrace)
                    Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "Semaphore released OK");
            }
            catch (SemaphoreFullException ex)
            {
                Logger.LogMessage("ReleaseSemaphore", FormatIDs(MyCallNumber) + "SemaphoreFullException: " + ex.ToString() + " " + ex.StackTrace);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("ReleaseSemaphore", FormatIDs(MyCallNumber) + "Exception: " + ex.ToString() + " " + ex.StackTrace);
            }
        }

        private long GetTransactionID(string p_Caller)
        {
            long ReturnValue;
            try
            {
                if (DebugTrace)
                    Logger.LogMessage(p_Caller, FormatIDs(0) + "Entered GetNextCount ");
                CallCountSemaphore.WaitOne();
                if (DebugTrace)
                    Logger.LogMessage(p_Caller, FormatIDs(0) + "Got CallCountMutex");

                if (CallCount != long.MaxValue)
                    CallCount += 1;
                else
                    CallCount = 0;

                ReturnValue = CallCount;
                CallCountSemaphore.Release();
                if (DebugTrace)
                    Logger.LogMessage(p_Caller, FormatIDs(ReturnValue) + "Released CallCountMutex");
            }
            catch (Exception ex)
            {
                Logger.LogMessage(p_Caller, "EXCEPTION: " + ex.ToString());
                throw;
            }
            return ReturnValue;
        }

        private byte ReadByte(string p_Caller, long MyCallNumber)
        {
            DateTime StartTime;
            byte RxByte;
            byte[] RxBytes = new byte[11];
            StartTime = DateTime.Now;
            if (DebugTrace)
                Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "Entered ReadByte: " + UseReadPolling);
            if (UseReadPolling)
            {
                while ((m_Port.BytesToRead == 0))
                {
                    if ((DateTime.Now - StartTime).TotalMilliseconds > m_ReceiveTimeout)
                    {
                        Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "ReadByte timed out waitng for a byte to read, throwing TimeoutException");
                        throw new TimeoutException("Serial port timed out waiting to read a byte");
                    }
                    Thread.Sleep(1);
                }
            }
            RxByte = System.Convert.ToByte(m_Port.ReadByte());
            if (DebugTrace)
                Logger.LogMessage(p_Caller, FormatIDs(MyCallNumber) + "ReadByte returning result - \"" + RxByte.ToString() + "\"");
            return RxByte;
        }

        private char ReadChar(string p_Caller, long TransactionID)
        {
            DateTime StartTime;
            char RxChar;
            char[] RxChars = new char[11];
            StartTime = DateTime.Now;
            if (DebugTrace)
                Logger.LogMessage(p_Caller, FormatIDs(TransactionID) + "Entered ReadChar: " + UseReadPolling);
            if (UseReadPolling)
            {
                while ((m_Port.BytesToRead == 0))
                {
                    if ((DateTime.Now - StartTime).TotalMilliseconds > m_ReceiveTimeout)
                    {
                        Logger.LogMessage(p_Caller, FormatIDs(TransactionID) + "ReadByte timed out waitng for a character to read, throwing TimeoutException");
                        throw new TimeoutException("Serial port timed out waiting to read a character");
                    }
                    Thread.Sleep(1);
                }
            }
            RxChar = Strings.Chr(m_Port.ReadByte());
            if (DebugTrace)
                Logger.LogMessage(p_Caller, FormatIDs(TransactionID) + "ReadChar returning result - \"" + RxChar + "\"");
            return RxChar;
        }

        internal string FormatIDs(long TransactionID)
        {
            ;/* Cannot convert LocalDeclarationStatementSyntax, System.NotSupportedException: StaticKeyword not supported!
   at ICSharpCode.CodeConverter.CSharp.SyntaxKindExtensions.ConvertToken(SyntaxKind t, TokenContext context)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifier(SyntaxToken m, TokenContext context)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.<ConvertModifiersCore>d__15.MoveNext()
   at System.Linq.Enumerable.WhereEnumerableIterator`1.MoveNext()
   at Microsoft.CodeAnalysis.SyntaxTokenList.CreateNode(IEnumerable`1 tokens)
   at ICSharpCode.CodeConverter.CSharp.CommonConversions.ConvertModifiers(IEnumerable`1 modifiers, TokenContext context, Boolean isVariableOrConst)
   at ICSharpCode.CodeConverter.CSharp.VisualBasicConverter.MethodBodyVisitor.VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
   at Microsoft.CodeAnalysis.VisualBasic.Syntax.LocalDeclarationStatementSyntax.Accept[TResult](VisualBasicSyntaxVisitor`1 visitor)
   at Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxVisitor`1.Visit(SyntaxNode node)
   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.ConvertWithTrivia(SyntaxNode node)
   at ICSharpCode.CodeConverter.CSharp.CommentConvertingMethodBodyVisitor.DefaultVisit(SyntaxNode node)

Input: 
        Static LastTransactionID As Long, TransactionIDString As String

 */
            if (DebugTrace)
            {
                if (TransactionID != 0)
                {
                    LastTransactionID = TransactionID;
                    TransactionIDString = TransactionID.ToString();
                }
                else
                    TransactionIDString = Strings.Left(Strings.Space(8), Strings.Len(LastTransactionID.ToString()));

                return TransactionIDString + " " + Thread.CurrentThread.ManagedThreadId + " ";
            }
            else
                return "";
        }

        internal class PortNameComparer : System.Collections.Generic.IComparer<string>
        {
            // IComparer implementation that compares COM port names based on the numeric port number
            // rather than the whole port name string

            internal int Compare(string x, string y)
            {
                string xs, ys;
                xs = x.ToString(); // Make sure we are working with strings
                ys = y.ToString();
                if ((Strings.Len(xs) >= 4) & (Strings.Len(ys) >= 4))
                {
                    if ((Strings.Left(xs, 3).ToUpperInvariant() == "COM") & (Strings.Left(ys, 3).ToUpperInvariant() == "COM"))
                    {
                        if (Information.IsNumeric(Strings.Mid(xs, 4)) & Information.IsNumeric(Strings.Mid(ys, 4)))
                            return Comparer.Default.Compare(System.Convert.ToInt32(Strings.Mid(xs, 4)), System.Convert.ToInt32(Strings.Mid(ys, 4)));// Compare based on port numbers
                    }
                }
                return Comparer.Default.Compare(x, y); // Compare based on port names
            }
        }



        const UInt32 INFINITE = 0xFFFFFFFFU;
        const UInt32 WAIT_ABANDONED = 0x80U;
        const UInt32 WAIT_OBJECT_0 = 0x0U;
        const UInt32 WAIT_TIMEOUT = 0x102U;

        /// <summary>
        ///     ''' OS level blocking wait for an event 
        ///     ''' </summary>
        ///     ''' <param name="handle">The triggering even't handle</param>
        ///     ''' <param name="milliseconds">Length of time to wait before timing out</param>
        ///     ''' <returns>Status, 0 = success</returns>
        ///     ''' <remarks></remarks>
        [DllImport("kernel32", SetLastError = true)]
        private static UInt32 WaitForSingleObject(IntPtr handle, UInt32 milliseconds)
        {
        }

        /// <summary>
        ///     ''' Sleep the calling thread until the worker thread has completed
        ///     ''' </summary>
        ///     ''' <param name="TData">ThreadData class holding required inputs, outputs and thread management information</param>
        ///     ''' <remarks></remarks>
        private void WaitForThread(ThreadData TData)
        {
            uint RetCode;

            // WARNING WARNING WARNING - Never put a logging statement here, before the sync command!

            // It will release the primary thread in VB6 drivers, which results in the next serial command being scheduled before
            // this one is completed and you get out of order serial command execution.

            // It is vital that the primary thread is fully blocked in order for VB6 drivers to work as expected. This means that some .NET sync primitives
            // such as ManualResetEvent CANNOT be used because they have been designed by MS to pump messages in the background while waiting
            // and this results in behaviur described above!

            // Execute the correct wait according to the set configuration
            switch (TypeOfWait)
            {
                case WaitType.ManualResetEvent:
                    {
                        if (DebugTrace)
                        {
                            Stopwatch ts = new Stopwatch();
                            ts.Start();
                            TData.ManualResetEvent.WaitOne(Timeout.Infinite);
                            LogMessage("WaitForThread", FormatIDs(TData.TransactionID) + "Completed ManualResetEvent OK, Command: " + TData.SerialCommand.ToString() + " Elapsed: " + ts.Elapsed.TotalMilliseconds);
                        }
                        else
                            TData.ManualResetEvent.WaitOne(Timeout.Infinite);
                        break;
                    }

                case WaitType.Sleep:
                    {
                        if (DebugTrace)
                        {
                            Stopwatch ts = new Stopwatch();
                            ts.Start();
                            do
                            {
                                Thread.Sleep(1);
                                LogMessage("WaitForThread", FormatIDs(TData.TransactionID) + "Command: " + TData.SerialCommand.ToString() + " Elapsed: " + ts.Elapsed.TotalMilliseconds);
                            }
                            while (!TData.Completed);
                            LogMessage("WaitForThread", FormatIDs(TData.TransactionID) + "Completed Sleep OK, Command: " + TData.SerialCommand.ToString() + " Elapsed: " + ts.Elapsed.TotalMilliseconds);
                        }
                        else
                            do
                                Thread.Sleep(1);
                            while (!TData.Completed);
                        break;
                    }

                case WaitType.WaitForSingleObject:
                    {
                        if (DebugTrace)
                        {
                            Stopwatch ts = new Stopwatch();
                            ts.Start();
                            RetCode = WaitForSingleObject(TData.ManualResetEvent.SafeWaitHandle.DangerousGetHandle(), INFINITE);
                            switch (RetCode)
                            {
                                case WAIT_OBJECT_0:
                                    {
                                        LogMessage("WaitForThread", FormatIDs(TData.TransactionID) + "Completed WaitForSingleObject OK, " + "Command: " + TData.SerialCommand.ToString() + " Elapsed: " + ts.Elapsed.TotalMilliseconds);
                                        break;
                                    }

                                case WAIT_ABANDONED:
                                    {
                                        LogMessage("***WaitForThread***", FormatIDs(TData.TransactionID) + "Completed WaitForSingleObject - ABANDONED, Return code: 0x" + RetCode.ToString("X8") + ", Command: " + TData.SerialCommand.ToString() + " Elapsed: " + ts.Elapsed.TotalMilliseconds);
                                        break;
                                    }

                                case WAIT_TIMEOUT:
                                    {
                                        LogMessage("***WaitForThread***", FormatIDs(TData.TransactionID) + "Completed WaitForSingleObject - TIMEOUT, Return code: 0x" + RetCode.ToString("X8") + ", Command: " + TData.SerialCommand.ToString() + " Elapsed: " + ts.Elapsed.TotalMilliseconds);
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            RetCode = WaitForSingleObject(TData.ManualResetEvent.SafeWaitHandle.DangerousGetHandle(), INFINITE);
                            switch (RetCode)
                            {
                                case WAIT_OBJECT_0:
                                    {
                                        break;
                                    }

                                case WAIT_ABANDONED:
                                    {
                                        LogMessage("***WaitForThread***", FormatIDs(TData.TransactionID) + "Completed WaitForSingleObject - ABANDONED, Return code: 0x" + RetCode.ToString("X8") + ", Command: " + TData.SerialCommand.ToString());
                                        break;
                                    }

                                case WAIT_TIMEOUT:
                                    {
                                        LogMessage("***WaitForThread***", FormatIDs(TData.TransactionID) + "Completed WaitForSingleObject - TIMEOUT, Return code: 0x" + RetCode.ToString("X8") + ", Command: " + TData.SerialCommand.ToString());
                                        break;
                                    }
                            }
                        }

                        break;
                    }
            }

            // Check whether we are propcessing out of order, which is a bad thing!
            if (DebugTrace & (TData.TransactionID != CallCount))
                LogMessage("***WaitForThread***", "Transactions out of order! TransactionID CurrentCallCount: " + TData.TransactionID + " " + CallCount);
        }

        /// <summary>
        ///     ''' Worker thread data
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        private class ThreadData
        {
            // Transmit Input values
            public string TransmitString;
            public byte[] TransmitBytes;

            // Receive Input values
            public string Terminator;
            public byte[] TerminatorBytes;
            public int Count;

            // Output values
            public Exception LastException;
            public byte ResultByte;
            public byte[] ResultByteArray;
            public string ResultString;
            public char ResultChar;

            // AvailableCOMPorts value
            public Generic.List<string> AvailableCOMPorts;

            // Control values
            public SerialCommandType SerialCommand;
            public bool Completed;
            public ManualResetEvent ManualResetEvent;
            public long TransactionID;

            // Management
            public bool Connecting;
            public int TimeoutValueMs;
            public int TimeoutValue;

            /// <summary>
            ///         ''' Initialises a new ThreadData synchronisation object
            ///         ''' </summary>
            ///         ''' <remarks></remarks>
            public ThreadData()
            {
                // Initialise values to required state
                this.Completed = false;
                this.LastException = null;
                this.ManualResetEvent = new ManualResetEvent(false);
            }

            /// <summary>
            ///         ''' Signals that this thread has completed its work
            ///         ''' </summary>
            ///         ''' <remarks></remarks>
            public void ThreadCompleted()
            {
                try
                {
                    Completed = true;
                }
                catch
                {
                }
                try
                {
                    ManualResetEvent.Set();
                }
                catch
                {
                }
            }
        }
    }
}