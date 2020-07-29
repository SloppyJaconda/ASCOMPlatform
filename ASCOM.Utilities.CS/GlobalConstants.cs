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

namespace ASCOM.Utilities.CS
{
// Common constants for the ASCOM.Utilities namesapce

static class GlobalConstants
{
    internal readonly static string SERIAL_FILE_NAME_VARNAME = "SerTraceFile"; // Constant naming the profile trace file variable name
    internal readonly static string SERIAL_AUTO_FILENAME = @"C:\SerialTraceAuto.txt"; // Special value to indicate use of automatic trace filenames
    internal readonly static string SERIAL_DEFAULT_FILENAME = @"C:\SerialTrace.txt"; // Default manual trace filename
    internal readonly static string SERIAL_DEBUG_TRACE_VARNAME = "SerDebugTrace"; // Constant naming the profile trace file variable name
    internal readonly static string SERIALPORT_COM_PORT_SETTINGS = "COMPortSettings";
    internal readonly static string SERIAL_FORCED_COMPORTS_VARNAME = SERIALPORT_COM_PORT_SETTINGS + @"\ForceCOMPorts"; // Constant listing COM ports that will be forced to be present
    internal readonly static string SERIAL_IGNORE_COMPORTS_VARNAME = SERIALPORT_COM_PORT_SETTINGS + @"\IgnoreCOMPorts"; // Constant listing COM ports that will be ignored if present

    // Utilities configuration constants
    internal readonly static string TRACE_XMLACCESS = "Trace XMLAccess";
    internal readonly static bool TRACE_XMLACCESS_DEFAULT = false;
    internal readonly static string TRACE_PROFILE = "Trace Profile";
    internal readonly static bool TRACE_PROFILE_DEFAULT = false;
    internal readonly static string TRACE_UTIL = "Trace Util";
    internal readonly static bool TRACE_UTIL_DEFAULT = false;
    internal readonly static string TRACE_TIMER = "Trace Timer";
    internal readonly static bool TRACE_TIMER_DEFAULT = false;
    internal readonly static string SERIAL_TRACE_DEBUG = "Serial Trace Debug";
    internal readonly static bool SERIAL_TRACE_DEBUG_DEFAULT = false;
    internal readonly static string SIMULATOR_TRACE = "Trace Simulators";
    internal readonly static bool SIMULATOR_TRACE_DEFAULT = false;
    internal readonly static string DRIVERACCESS_TRACE = "Trace DriverAccess";
    internal readonly static bool DRIVERACCESS_TRACE_DEFAULT = false;
    internal readonly static string CHOOSER_USE_CREATEOBJECT = "Chooser Use CreateObject";
    internal readonly static bool CHOOSER_USE_CREATEOBJECT_DEFAULT = false;
    internal readonly static string ABANDONED_MUTEXT_TRACE = "Trace Abandoned Mutexes";
    internal readonly static bool ABANDONED_MUTEX_TRACE_DEFAULT = false;
    internal readonly static string ASTROUTILS_TRACE = "Trace Astro Utils";
    internal readonly static bool ASTROUTILS_TRACE_DEFAULT = false;
    internal readonly static string NOVAS_TRACE = "Trace NOVAS";
    internal readonly static bool NOVAS_TRACE_DEFAULT = false;
    internal readonly static string SERIAL_WAIT_TYPE = "Serial Wait Type";
    internal readonly static ASCOM.Utilities.Serial.WaitType SERIAL_WAIT_TYPE_DEFAULT = Serial.WaitType.WaitForSingleObject;
    internal readonly static string SUPPRESS_ALPACA_DRIVER_ADMIN_DIALOGUE = "Suppress Alpaca Driver Admin Dialogue";
    internal readonly static bool SUPPRESS_ALPACA_DRIVER_ADMIN_DIALOGUE_DEFAULT = false;
    internal readonly static string PROFILE_MUTEX_NAME = "ASCOMProfileMutex"; // Name and timout value for the Profile mutex than ensures only one profile action happens at a time
    internal readonly static int PROFILE_MUTEX_TIMEOUT = 5000;

    // Trace settings values, these are used to persist trace values on a per user basis
    internal readonly static string TRACE_TRANSFORM = "Trace Transform";
    internal readonly static bool TRACE_TRANSFORM_DEFAULT = false;
    internal readonly static string REGISTRY_UTILITIES_FOLDER = @"Software\ASCOM\Utilities";
    internal readonly static string TRACE_CACHE = "Trace Cache";
    internal readonly static bool TRACE_CACHE_DEFAULT = false;
    internal readonly static string TRACE_EARTHROTATION_DATA_FORM = "Trace Earth Rotation Data Form";
    internal readonly static bool TRACE_EARTHROTATION_DATA_FORM_DEFAULT = false;

    // Settings for the ASCOM Windows event log
    internal readonly static string EVENT_SOURCE = "ASCOM Platform"; // Name of the the event source
    internal readonly static string EVENTLOG_NAME = "ASCOM"; // Name of the event log as it appears in Windows event viewer
    internal readonly static string EVENTLOG_MESSAGES = @"ASCOM\EventLogMessages.txt";
    internal readonly static string EVENTLOG_ERRORS = @"ASCOM\EventLogErrors.txt";

    // RegistryAccess constants
    internal readonly static string REGISTRY_ROOT_KEY_NAME = @"SOFTWARE\ASCOM"; // Location of ASCOM profile in HKLM registry hive
    internal readonly static string REGISTRY_5_BACKUP_SUBKEY = "Platform5Original"; // Location that the original Plartform 5 Profile will be copied to before migrating the 5.5 Profile back to the registry
    internal readonly static string REGISTRY_55_BACKUP_SUBKEY = "Platform55Original"; // Location that the original Plartform 5.5 Profile will be copied to before removing Platform 5 and 5.5
    internal readonly static string PLATFORM_VERSION_NAME = "PlatformVersion";
    // XML constants used by XMLAccess and RegistryAccess classes
    internal readonly static string COLLECTION_DEFAULT_VALUE_NAME = "***** DefaultValueName *****"; // Name identifier label
    internal readonly static string COLLECTION_DEFAULT_UNSET_VALUE = "===== ***** UnsetValue ***** ====="; // Value identifier label
    internal readonly static string VALUES_FILENAME = "Profile.xml"; // Name of file to contain profile xml information
    internal readonly static string VALUES_FILENAME_ORIGINAL = "ProfileOriginal.xml"; // Name of file to contain original profile xml information
    internal readonly static string VALUES_FILENAME_NEW = "ProfileNew.xml"; // Name of file to contain original profile xml information

    internal readonly static string PROFILE_NAME = "Profile"; // Name of top level XML element
    internal readonly static string SUBKEY_NAME = "SubKey"; // Profile subkey element name
    internal readonly static string DEFAULT_ELEMENT_NAME = "DefaultElement"; // Default value label
    internal readonly static string VALUE_ELEMENT_NAME = "Element"; // Profile value element name
    internal readonly static string NAME_ATTRIBUTE_NAME = "Name"; // Profile value name attribute
    internal readonly static string VALUE_ATTRIBUTE_NAME = "Value"; // Profile element value attribute

    // XML constants used by ASCOMProfile class to serialise and de-serialise a profile
    // These are public so that they can be used by applications to work directly with the returned XML
    public readonly static string XML_SUBKEYNAME_ELEMENTNAME = "SubKeyName";
    public readonly static string XML_DEFAULTVALUE_ELEMENTNAME = "DefaultValue";
    public readonly static string XML_NAME_ELEMENTNAME = "Name";
    public readonly static string XML_DATA_ELEMENTNAME = "Data";
    public readonly static string XML_SUBKEY_ELEMENTNAME = "SubKey";
    public readonly static string XML_VALUE_ELEMENTNAME = "Value";
    public readonly static string XML_VALUES_ELEMENTNAME = "Values";

    // Location of the lists of 32bit and 64bit only drivers and PlatformVersion exception lists
    public readonly static string DRIVERS_32BIT = "Drivers Not Compatible With 64bit Applications"; // 32bit only registry location
    public readonly static string DRIVERS_64BIT = "Drivers Not Compatible With 32bit Applications"; // 64bit only registry location
    internal readonly static string PLATFORM_VERSION_EXCEPTIONS = "ForcePlatformVersion";
    internal readonly static string PLATFORM_VERSION_SEPARATOR_EXCEPTIONS = "ForcePlatformVersionSeparator";

    internal readonly static string FORCE_SYSTEM_TIMER = "ForceSystemTimer"; // Location of executables for which we must force system timer rather than forms timer

    // Installer Variables
    public readonly static string PLATFORM_INSTALLER_PROPDUCT_CODE = "{8961E141-B307-4882-ABAD-77A3E76A40C1}"; // {8961E141-B307-4882-ABAD-77A3E76A40C1}
    public readonly static string DEVELOPER_INSTALLER_PROPDUCT_CODE = "{4A195DC6-7DF9-459E-8F93-60B61EB45288}";

    // Contact driver author message
    internal readonly static string DRIVER_AUTHOR_MESSAGE_DRIVER = "Please contact the driver author and request an updated driver.";
    internal readonly static string DRIVER_AUTHOR_MESSAGE_INSTALLER = "Please contact the driver author and request an updated installer.";

    // Location of Platform version in Profile
    internal readonly static string PLATFORM_INFORMATION_SUBKEY = "Platform";
    internal readonly static string PLATFORM_VERSION = "Platform Version";
    internal readonly static string PLATFORM_VERSION_DEFAULT_BAD_VALUE = "0.0.0.0";

    // Other constants
    internal readonly static double ABSOLUTE_ZERO_CELSIUS = -273.15;
    internal readonly static string TRACE_LOGGER_PATH = @"\ASCOM"; // Path to TraceLogger directory from My Documents
    internal readonly static string TRACE_LOGGER_FILENAME_BASE = @"\Logs "; // Fixed part of TraceLogger file name.  Note: The trailing space must be retained!
    internal readonly static string TRACE_LOGGER_FILE_NAME_DATE_FORMAT = "yyyy-MM-dd";
    internal readonly static string TRACE_LOGGER_SYSTEM_PATH = @"\ASCOM\SystemLogs"; // Location where "System" user logs will be placed

    internal enum EventLogErrors : int
    {
        EventLogCreated = 0,
        ChooserFormLoad = 1,
        MigrateProfileVersions = 2,
        MigrateProfileRegistryKey = 3,
        RegistryProfileMutexTimeout = 4,
        XMLProfileMutexTimeout = 5,
        XMLAccessReadError = 6,
        XMLAccessRecoveryPreviousVersion = 7,
        XMLAccessRecoveredOK = 8,
        ChooserSetupFailed = 9,
        ChooserDriverFailed = 10,
        ChooserException = 11,
        Chooser32BitOnlyException = 12,
        Chooser64BitOnlyException = 13,
        FocusSimulatorNew = 14,
        FocusSimulatorSetup = 15,
        TelescopeSimulatorNew = 16,
        TelescopeSimulatorSetup = 17,
        VB6HelperProfileException = 18,
        DiagnosticsLoadException = 19,
        DriverCompatibilityException = 20,
        TimerSetupException = 21,
        DiagnosticsHijackedCOMRegistration = 22,
        UninstallASCOMInfo = 23,
        UninstallASCOMError = 24,
        ProfileExplorerException = 25,
        InstallTemplatesInfo = 26,
        InstallTemplatesError = 27,
        TraceLoggerException = 28,
        TraceLoggerMutexTimeOut = 29,
        TraceLoggerMutexAbandoned = 30,
        RegistryProfileMutexAbandoned = 31,
        EarthRotationUpdate = 32
    }
}
}