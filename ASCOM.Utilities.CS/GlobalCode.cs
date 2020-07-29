
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
using Microsoft.Win32;
using System.Runtime.InteropServices;
using ASCOM.Utilities;
using ASCOM.Utilities.Serial;

namespace ASCOM.Utilities.CS
{
// These items are shared between the ASCOM.Utilities and ASCOM.Astrometry assemblies




static class COMRegistrationSupport
{
    /// <summary>
    ///     ''' Update a COM registration assembly executable reference (mscoree.dll) from a relative path to an absolute path
    ///     ''' </summary>
    ///     ''' <remarks>This is necessary to ensure that the mscoree.dll can be found when the SetSearchDirectories function has been called in an application e.g. by Inno installer post v5.5.9.
    ///     ''' The COM class name and ClassID are determined from the supplied type definition. If the ClassID cannot be determined it is looked up through the COM registration registry entry through the class's ProgID
    ///     ''' </remarks>
    internal static void COMRegister(Type typeToRegister)
    {
        string className, clsId, mscoree, fullPath;
        object[] attributes;
        TraceLogger TL = null/* TODO Change to default(_) if this is not a reference type */;

        try
        {
            TL = new TraceLogger("", "COMRegister" + typeToRegister.Name);
            TL.Enabled = true;
            TL.LogMessage("COMRegisterActions", "Start");

            // Report the OS and application bitness
            if (VersionCode.OSBits() == VersionCode.Bitness.Bits64)
                TL.LogMessage("OSBits", "64bit OS");
            else
                TL.LogMessage("OSBits", "32bit OS");

            if (VersionCode.ApplicationBits() == VersionCode.Bitness.Bits64)
                TL.LogMessage("ApplicationBits", "64bit application");
            else
                TL.LogMessage("ApplicationBits", "32bit application");

            // Create the fully qualified class name from the namespace and class name
            className = string.Format("{0}.{1}", typeToRegister.Namespace, typeToRegister.Name);
            TL.LogMessage("ClassName", className);

            // Determine the class GUID of the supplied type 
            attributes = typeToRegister.GetCustomAttributes(typeof(GuidAttribute), false); // Get any GUID references in the supplied type - there should always be just one reference

            // Act depending on whether we have found the GUID
            switch (attributes.Length)
            {
                case 0 // No GUID attribute found - this should never happen
               :
                    {
                        TL.LogMessage("COMRegisterActions", "GuidAttribute not found, obtaining the correct class GUID from the COM registration in the registry");

                        clsId = System.Convert.ToString(Registry.ClassesRoot.OpenSubKey(className + @"\CLSID").GetValue("")); // Try plan B to get the GUID from the class's COM registration
                        if (!string.IsNullOrEmpty(clsId))
                            TL.LogMessage("ClassID", clsId);
                        else
                            TL.LogMessage("ClassID", "Could not find ClassID - returned value is null or an empty string");
                        break;
                    }

                case 1 // Found the class GUID attribute so extract and use it
         :
                    {
                        TL.LogMessage("COMRegisterActions", "Found a class GuidAttribute - using it to create the class GUID");

                        clsId = "{" + (GuidAttribute)attributes[0].Value + "}"; // Create the class ID by enclosing the class GUID in braces
                        if (!string.IsNullOrEmpty(clsId))
                            TL.LogMessage("ClassID", clsId);
                        else
                            TL.LogMessage("ClassID", "Could not find ClassID - returned value is null or an empty string");
                        break;
                    }

                default:
                    {
                        TL.LogMessage("COMRegisterActions", string.Format("{0} GuidAttributes found, obtaining the correct class GUID from the COM registration in the registry", attributes.Length));

                        clsId = System.Convert.ToString(Registry.ClassesRoot.OpenSubKey(className + @"\CLSID").GetValue(""));
                        if (!string.IsNullOrEmpty(clsId))
                            TL.LogMessage("ClassID", clsId);
                        else
                            TL.LogMessage("ClassID", "Could not find ClassID - returned value is null or an empty string");
                        break;
                    }
            }

            // If we have a ClassID then use it to update the class's executable relative path to a full path
            if (!string.IsNullOrEmpty(clsId))
            {
                mscoree = System.Convert.ToString(Registry.ClassesRoot.OpenSubKey(string.Format(@"\CLSID\{0}\InProcServer32", clsId)).GetValue(""));
                TL.LogMessage("COMRegisterActions", string.Format("Current mscoree.dll path: {0}", mscoree));

                if ((mscoree.ToUpperInvariant() == "MSCOREE.DLL"))
                {
                    TL.LogMessage("COMRegisterActions", string.Format("The mscoree.dll path is relative: {0}", mscoree));

                    fullPath = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.System), mscoree);
                    TL.LogMessage("COMRegisterActions", string.Format("Full path to the System32 directory: {0}", fullPath));

                    TL.LogMessage("COMRegisterActions", "Setting InProcServer32 value...");
                    Registry.ClassesRoot.OpenSubKey(string.Format(@"\CLSID\{0}\InProcServer32", clsId), true).SetValue("", fullPath);
                    TL.LogMessage("COMRegisterActions", string.Format("InProcServer32value set OK - {0}", fullPath));

                    mscoree = System.Convert.ToString(Registry.ClassesRoot.OpenSubKey(string.Format(@"\CLSID\{0}\InProcServer32", clsId)).GetValue(""));
                    TL.LogMessage("COMRegisterActions", string.Format("New mscoree.dll path: {0}", mscoree));
                }
                else
                    TL.LogMessage("COMRegisterActions", "Path is already absolute - no action taken");
            }
            else
                TL.LogMessage("COMRegisterActions", "Unable to find the class's ClassID - no action taken.");
        }
        catch (Exception ex)
        {
            if (!(TL == null))
                TL.LogMessageCrLf("Exception", ex.ToString());
        }
    }
}



static class RegistryCommonCode
{
    internal static WaitType GetWaitType(string p_Name, ASCOM.Utilities.Serial.WaitType p_DefaultValue)
    {
        WaitType l_Value;
        RegistryKey m_HKCU, m_SettingsKey;

        m_HKCU = Registry.CurrentUser;
        m_HKCU.CreateSubKey(REGISTRY_UTILITIES_FOLDER);
        m_SettingsKey = m_HKCU.OpenSubKey(REGISTRY_UTILITIES_FOLDER, true);

        try
        {
            if (m_SettingsKey.GetValueKind(p_Name) == RegistryValueKind.String)
                l_Value = (WaitType)Enum.Parse(typeof(WaitType), m_SettingsKey.GetValue(p_Name).ToString());
        }
        catch (IOException ex)
        {
            try
            {
                SetName(p_Name, p_DefaultValue.ToString);
                l_Value = p_DefaultValue;
            }
            catch (Exception ex1)
            {
                l_Value = p_DefaultValue;
            }
        }
        catch (Exception ex)
        {
            l_Value = p_DefaultValue;
        }

        m_SettingsKey.Flush(); // Clean up registry keys
        m_SettingsKey.Close();
        m_SettingsKey = null;
        m_HKCU.Flush();
        m_HKCU.Close();
        m_HKCU = null;

        return l_Value;
    }

    internal static bool GetBool(string p_Name, bool p_DefaultValue)
    {
        bool l_Value;
        RegistryKey m_HKCU, m_SettingsKey;

        m_HKCU = Registry.CurrentUser;
        m_HKCU.CreateSubKey(REGISTRY_UTILITIES_FOLDER);
        m_SettingsKey = m_HKCU.OpenSubKey(REGISTRY_UTILITIES_FOLDER, true);

        try
        {
            if (m_SettingsKey.GetValueKind(p_Name) == RegistryValueKind.String)
                l_Value = System.Convert.ToBoolean(m_SettingsKey.GetValue(p_Name));
        }
        catch (IOException ex)
        {
            try
            {
                SetName(p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex1)
            {
                l_Value = p_DefaultValue;
            }
        }
        catch (Exception ex)
        {
            l_Value = p_DefaultValue;
        }
        m_SettingsKey.Flush(); // Clean up registry keys
        m_SettingsKey.Close();
        m_SettingsKey = null;
        m_HKCU.Flush();
        m_HKCU.Close();
        m_HKCU = null;

        return l_Value;
    }

    internal static string GetString(string p_Name, string p_DefaultValue)
    {
        string l_Value = "";
        RegistryKey m_HKCU, m_SettingsKey;

        m_HKCU = Registry.CurrentUser;
        m_HKCU.CreateSubKey(REGISTRY_UTILITIES_FOLDER);
        m_SettingsKey = m_HKCU.OpenSubKey(REGISTRY_UTILITIES_FOLDER, true);

        try
        {
            if (m_SettingsKey.GetValueKind(p_Name) == RegistryValueKind.String)
                l_Value = m_SettingsKey.GetValue(p_Name).ToString();
        }
        catch (IOException ex)
        {
            try
            {
                SetName(p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex1)
            {
                l_Value = p_DefaultValue;
            }
        }
        catch (Exception ex)
        {
            l_Value = p_DefaultValue;
        }
        m_SettingsKey.Flush(); // Clean up registry keys
        m_SettingsKey.Close();
        m_SettingsKey = null;
        m_HKCU.Flush();
        m_HKCU.Close();
        m_HKCU = null;

        return l_Value;
    }

    internal static double GetDouble(RegistryKey p_Key, string p_Name, double p_DefaultValue)
    {
        double l_Value;
        RegistryKey m_HKCU, m_SettingsKey;

        m_HKCU = Registry.CurrentUser;
        m_HKCU.CreateSubKey(REGISTRY_UTILITIES_FOLDER);
        m_SettingsKey = m_HKCU.OpenSubKey(REGISTRY_UTILITIES_FOLDER, true);

        // LogMsg("GetDouble", GlobalVarsAndCode.MessageLevel.msgDebug, p_Name.ToString & " " & p_DefaultValue.ToString)
        try
        {
            if (p_Key.GetValueKind(p_Name) == RegistryValueKind.String)
                l_Value = System.Convert.ToDouble(p_Key.GetValue(p_Name));
        }
        catch (IOException ex)
        {
            try
            {
                SetName(p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex1)
            {
                l_Value = p_DefaultValue;
            }
        }
        catch (Exception ex)
        {
            l_Value = p_DefaultValue;
        }
        m_SettingsKey.Flush(); // Clean up registry keys
        m_SettingsKey.Close();
        m_SettingsKey = null;
        m_HKCU.Flush();
        m_HKCU.Close();
        m_HKCU = null;

        return l_Value;
    }

    internal static DateTime GetDate(string p_Name, DateTime p_DefaultValue)
    {
        DateTime l_Value;
        RegistryKey m_HKCU, m_SettingsKey;

        m_HKCU = Registry.CurrentUser;
        m_HKCU.CreateSubKey(REGISTRY_UTILITIES_FOLDER);
        m_SettingsKey = m_HKCU.OpenSubKey(REGISTRY_UTILITIES_FOLDER, true);

        try
        {
            if (m_SettingsKey.GetValueKind(p_Name) == RegistryValueKind.String)
                l_Value = (DateTime)m_SettingsKey.GetValue(p_Name);
        }
        catch (IOException ex)
        {
            try
            {
                SetName(p_Name, p_DefaultValue.ToString());
                l_Value = p_DefaultValue;
            }
            catch (Exception ex1)
            {
                l_Value = p_DefaultValue;
            }
        }
        catch (Exception ex)
        {
            l_Value = p_DefaultValue;
        }
        m_SettingsKey.Flush(); // Clean up registry keys
        m_SettingsKey.Close();
        m_SettingsKey = null;
        m_HKCU.Flush();
        m_HKCU.Close();
        m_HKCU = null;

        return l_Value;
    }

    internal static void SetName(string p_Name, string p_Value)
    {
        RegistryKey m_HKCU, m_SettingsKey;

        m_HKCU = Registry.CurrentUser;
        m_HKCU.CreateSubKey(REGISTRY_UTILITIES_FOLDER);
        m_SettingsKey = m_HKCU.OpenSubKey(REGISTRY_UTILITIES_FOLDER, true);

        m_SettingsKey.SetValue(p_Name, p_Value.ToString(), RegistryValueKind.String);
        m_SettingsKey.Flush(); // Clean up registry keys
        m_SettingsKey.Close();
        m_SettingsKey = null;
        m_HKCU.Flush();
        m_HKCU.Close();
        m_HKCU = null;
    }
}



static class EventLogCode
{

    /// <summary>
    ///     ''' Add an event record to the ASCOM Windows event log
    ///     ''' </summary>
    ///     ''' <param name="Caller">Name of routine creating the event</param>
    ///     ''' <param name="Msg">Event message</param>
    ///     ''' <param name="Severity">Event severity</param>
    ///     ''' <param name="Id">Id number</param>
    ///     ''' <param name="Except">Initiating exception or Nothing</param>
    ///     ''' <remarks></remarks>
    internal static void LogEvent(string Caller, string Msg, EventLogEntryType Severity, EventLogErrors Id, string Except)
    {
        EventLog ELog;
        string MsgTxt;

        // During Platform 6 RC testing a report was received showing that a failure in this code had caused a bad Profile migration
        // There was no problem with the migration code, the issue was caused by the event log code throwing an unexpected exception back to MigrateProfile
        // It is wrong that an error in logging code should cause a client process to fail, so this code has been 
        // made more robust and ultimately will swallow exceptions silently rather than throwing an unexpected exception back to the caller

        try
        {
            if (!EventLog.SourceExists(EVENT_SOURCE))
            {
                EventLog.CreateEventSource(EVENT_SOURCE, EVENTLOG_NAME);
                ELog = new EventLog(EVENTLOG_NAME, ".", EVENT_SOURCE); // Create a pointer to the event log
                ELog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0); // Force the policy to overwrite oldest
                ELog.MaximumKilobytes = 1024; // Set the maximum log size to 1024kb, the Win 7 minimum size
                ELog.Close(); // Force the log file to be created by closing the log
                ELog.Dispose();
                ELog = null;

                // MSDN documentation advises waiting before writing, first time to a newly created event log file but doesn't say how long...
                // Waiting 3 seconds to allow the log to be created by the OS
                System.Threading.Thread.Sleep(3000);

                // Try and create the initial log message
                ELog = new EventLog(EVENTLOG_NAME, ".", EVENT_SOURCE); // Create a pointer to the event log
                ELog.WriteEntry("Successfully created event log - Policy: " + ELog.OverflowAction.ToString() + ", Size: " + ELog.MaximumKilobytes + "kb", EventLogEntryType.Information, EventLogErrors.EventLogCreated);
                ELog.Close();
                ELog.Dispose();
            }

            // Write the event to the log
            ELog = new EventLog(EVENTLOG_NAME, ".", EVENT_SOURCE); // Create a pointer to the event log

            MsgTxt = Caller + " - " + Msg; // Format the message to be logged
            if (!Except == null)
                MsgTxt += Constants.vbCrLf + Except;
            ELog.WriteEntry(MsgTxt, Severity, Id); // Write the message to the error log

            ELog.Close();
            ELog.Dispose();
        }
        catch (ComponentModel.Win32Exception ex)
        {
            try
            {
                string TodaysDateTime = Strings.Format(DateTime.Now(), "dd MMMM yyyy HH:mm:ss.fff");
                string ErrorLog = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + GlobalConstants.EVENTLOG_ERRORS;
                string MessageLog = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + GlobalConstants.EVENTLOG_MESSAGES;

                // Write to backup eventlog message and error logs
                File.AppendAllText(ErrorLog, TodaysDateTime + " ErrorCode: 0x" + Conversion.Hex(ex.ErrorCode) + " NativeErrorCode: 0x" + Conversion.Hex(ex.NativeErrorCode) + " " + ex.ToString() + Constants.vbCrLf);
                File.AppendAllText(MessageLog, TodaysDateTime + " " + Caller + " " + Msg + " " + Severity.ToString() + " " + Id.ToString + " " + Except + Constants.vbCrLf);
            }
            catch (Exception ex1)
            {
            } // Ignore exceptions here, the PC seems to be in a catastrophic failure!
        }
        catch (Exception ex)
        {
            // Somthing bad happened when writing to the event log so try and log it in a log file on the file system
            try
            {
                string TodaysDateTime = Strings.Format(DateTime.Now(), "dd MMMM yyyy HH:mm:ss.fff");
                string ErrorLog = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + GlobalConstants.EVENTLOG_ERRORS;
                string MessageLog = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + GlobalConstants.EVENTLOG_MESSAGES;

                // Write to backup eventlog message and error logs
                File.AppendAllText(ErrorLog, TodaysDateTime + " " + ex.ToString() + Constants.vbCrLf);
                File.AppendAllText(MessageLog, TodaysDateTime + " " + Caller + " " + Msg + " " + Severity.ToString() + " " + Id.ToString + " " + Except + Constants.vbCrLf);
            }
            catch (Exception ex1)
            {
            } // Ignore exceptions here, the PC seems to be in a catastrophic failure!
        }
    }
}



static class VersionCode
{
    internal static string GetCommonProgramFilesx86()
    {
        if ((IntPtr.Size == 8) | (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            return Environment.GetEnvironmentVariable("CommonProgramFiles(x86)");
        else
            return Environment.GetEnvironmentVariable("CommonProgramFiles");
    }

    internal static void RunningVersions(TraceLogger TL)
    {
        Assembly[] Assemblies; // Define an array of assembly information
        System.AppDomain AppDom = AppDomain.CurrentDomain;

        // Get Operating system information
        System.OperatingSystem OS = System.Environment.OSVersion;

        try
        {
            TL.LogMessage("Versions", "Run on: " + DateTime.Now.ToString("dddd dd MMMM yyyy"));
            TL.LogMessage("Versions", "Main Process: " + Process.GetCurrentProcess().MainModule.FileName); // Get the name of the executable without path or file extension
            FileVersionInfo FV;
            FV = Process.GetCurrentProcess().MainModule.FileVersionInfo; // Get the name of the executable without path or file extension
            TL.LogMessageCrLf("Versions", "  Product:  " + FV.ProductName + " " + FV.ProductVersion);
            TL.LogMessageCrLf("Versions", "  File:     " + FV.FileDescription + " " + FV.FileVersion);
            TL.LogMessageCrLf("Versions", "  Language: " + FV.Language);
            TL.BlankLine();
        }
        catch (Exception ex)
        {
            TL.LogMessage("Versions", "Exception EX0: " + ex.ToString());
        }

        try // Make sure this code never throws an exception back to the caller
        {
            TL.LogMessage("Versions", "OS Version: " + OS.Platform + ", Service Pack: " + OS.ServicePack + ", Full: " + OS.VersionString);
            switch (OSBits())
            {
                case Bitness.Bits32:
                    {
                        TL.LogMessage("Versions", "Operating system is 32bit");
                        break;
                    }

                case Bitness.Bits64:
                    {
                        TL.LogMessage("Versions", "Operating system is 64bit");
                        break;
                    }

                default:
                    {
                        TL.LogMessage("Versions", "Operating system is unknown bits, PTR length is: " + System.IntPtr.Size);
                        break;
                    }
            }

            switch (ApplicationBits())
            {
                case Bitness.Bits32:
                    {
                        TL.LogMessage("Versions", "Application is 32bit");
                        break;
                    }

                case Bitness.Bits64:
                    {
                        TL.LogMessage("Versions", "Application is 64bit");
                        break;
                    }

                default:
                    {
                        TL.LogMessage("Versions", "Application is unknown bits, PTR length is: " + System.IntPtr.Size);
                        break;
                    }
            }
            TL.LogMessage("Versions", "");

            // Get common language runtime version
            TL.LogMessage("Versions", "CLR version: " + System.Environment.Version.ToString());

            // Get file system information
            string UserDomainName = System.Environment.UserDomainName;
            string UserName = System.Environment.UserName;
            string MachineName = System.Environment.MachineName;
            int ProcCount = System.Environment.ProcessorCount;
            string SysDir = System.Environment.SystemDirectory;
            long WorkSet = System.Environment.WorkingSet;
            TL.LogMessage("Versions", "Machine name: " + MachineName + " UserName: " + UserName + " DomainName: " + UserDomainName);
            TL.LogMessage("Versions", "Number of processors: " + ProcCount + " System directory: " + SysDir + " Working set size: " + WorkSet + " bytes");
            TL.LogMessage("Versions", "");

            // Get fully qualified paths to particular directories in a non OS specific way
            // There are many more options in the SpecialFolders Enum than are shown here!
            TL.LogMessage("Versions", "My Documents:            " + System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            TL.LogMessage("Versions", "Application Data:        " + System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            TL.LogMessage("Versions", "Common Application Data: " + System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
            TL.LogMessage("Versions", "Program Files:           " + System.Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            TL.LogMessage("Versions", "Common Files:            " + System.Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles));
            TL.LogMessage("Versions", "System:                  " + System.Environment.GetFolderPath(Environment.SpecialFolder.System));
            TL.LogMessage("Versions", "Current:                 " + System.Environment.CurrentDirectory);
            TL.LogMessage("Versions", "");

            // Get loaded assemblies
            Assemblies = AppDom.GetAssemblies(); // Get a list of loaded assemblies
            foreach (Assembly FoundAssembly in Assemblies)
                TL.LogMessage("Versions", "Loaded Assemblies: " + FoundAssembly.GetName().Name + " " + FoundAssembly.GetName().Version.ToString());
            TL.LogMessage("Versions", "");

            // Get assembly versions
            AssemblyInfo(TL, "Entry Assembly", Assembly.GetEntryAssembly());
            AssemblyInfo(TL, "Executing Assembly", Assembly.GetExecutingAssembly());
            TL.BlankLine();
        }
        catch (Exception ex)
        {
            TL.LogMessageCrLf("Versions", "Unexpected exception: " + ex.ToString());
        }
    }

    internal enum Bitness
    {
        Bits32,
        Bits64,
        BitsMSIL,
        BitsUnknown
    }

    internal static Bitness OSBits()
    {
        if (IsWow64())
            return Bitness.Bits64;
        else
            switch (System.IntPtr.Size)
            {
                case 4:
                    {
                        return Bitness.Bits32;
                    }

                case 8:
                    {
                        return Bitness.Bits64;
                    }

                default:
                    {
                        return Bitness.BitsUnknown;
                    }
            }
    }

    internal static Bitness ApplicationBits()
    {
        switch (System.IntPtr.Size)
        {
            case 4:
                {
                    return Bitness.Bits32;
                }

            case 8:
                {
                    return Bitness.Bits64;
                }

            default:
                {
                    return Bitness.BitsUnknown;
                }
        }
    }

    internal static void AssemblyInfo(TraceLogger TL, string AssName, Assembly Ass)
    {
        FileVersionInfo FileVer;
        AssemblyName AssblyName;
        Version Vers;
        string VerString, FVer, FName;
        string Location = null;

        AssName = Strings.Left(AssName + ":" + Strings.Space(20), 19);

        if (!Ass == null)
        {
            try
            {
                AssblyName = Ass.GetName();
                if (AssblyName == null)
                    TL.LogMessage("Versions", AssName + " Assembly name is missing, cannot determine version");
                else
                {
                    Vers = AssblyName.Version;
                    if (Vers == null)
                        TL.LogMessage("Versions", AssName + " Assembly version is missing, cannot determine version");
                    else
                    {
                        VerString = Vers.ToString();
                        if (!string.IsNullOrEmpty(VerString))
                            TL.LogMessage("Versions", AssName + " AssemblyVersion: " + VerString);
                        else
                            TL.LogMessage("Versions", AssName + " Assembly version string is null or empty, cannot determine assembly version");
                    }
                }
            }
            catch (Exception ex)
            {
                TL.LogMessage("AssemblyInfo", "Exception EX1: " + ex.ToString());
            }

            try
            {
                Location = Ass.Location;
                if (string.IsNullOrEmpty(Location))
                    TL.LogMessage("Versions", AssName + "Assembly location is missing, cannot determine file version");
                else
                {
                    FileVer = FileVersionInfo.GetVersionInfo(Location);
                    if (FileVer == null)
                        TL.LogMessage("Versions", AssName + " File version object is null, cannot determine file version number");
                    else
                    {
                        FVer = FileVer.FileVersion;
                        if (!string.IsNullOrEmpty(FVer))
                            TL.LogMessage("Versions", AssName + " FileVersion: " + FVer);
                        else
                            TL.LogMessage("Versions", AssName + " File version string is null or empty, cannot determine file version");
                    }
                }
            }
            catch (Exception ex)
            {
                TL.LogMessage("AssemblyInfo", "Exception EX2: " + ex.ToString());
            }

            try
            {
                AssblyName = Ass.GetName();
                if (AssblyName == null)
                    TL.LogMessage("Versions", AssName + " Assembly name is missing, cannot determine full name");
                else
                {
                    FName = AssblyName.FullName;
                    if (!string.IsNullOrEmpty(FName))
                        TL.LogMessage("Versions", AssName + " Name: " + FName);
                    else
                        TL.LogMessage("Versions", AssName + " Full name string is null or empty, cannot determine full name");
                }
            }
            catch (Exception ex)
            {
                TL.LogMessage("AssemblyInfo", "Exception EX3: " + ex.ToString());
            }

            try
            {
                TL.LogMessage("Versions", AssName + " CodeBase: " + Ass.GetName().CodeBase);
            }
            catch (Exception ex)
            {
                TL.LogMessage("AssemblyInfo", "Exception EX4: " + ex.ToString());
            }

            try
            {
                if (!string.IsNullOrEmpty(Location))
                    TL.LogMessage("Versions", AssName + " Location: " + Location);
                else
                    TL.LogMessage("Versions", AssName + " Location is null or empty, cannot display location");
            }
            catch (Exception ex)
            {
                TL.LogMessage("AssemblyInfo", "Exception EX5: " + ex.ToString());
            }

            try
            {
                TL.LogMessage("Versions", AssName + " From GAC: " + Ass.GlobalAssemblyCache.ToString());
            }
            catch (Exception ex)
            {
                TL.LogMessage("AssemblyInfo", "Exception EX6: " + ex.ToString());
            }
        }
        else
            TL.LogMessage("Versions", AssName + " No assembly found");
    }

    /// <summary>
    ///     ''' Returns true when the application is 32bit and running on a 64bit OS
    ///     ''' </summary>
    ///     ''' <returns>True when the application is 32bit and running on a 64bit OS</returns>
    ///     ''' <remarks></remarks>
    private static bool IsWow64()
    {
        IntPtr value;
        value = System.Diagnostics.Process.GetCurrentProcess().Handle;

        bool retVal;
        if (IsWow64Process(value, ref retVal))
            return retVal;
        else
            return false;
    }

    /// <summary>
    ///     ''' Determines whether the specified process is running under WOW64 i.e. is a 32bit application running on a 64bit OS.
    ///     ''' </summary>
    ///     ''' <param name="hProcess">A handle to the process. The handle must have the PROCESS_QUERY_INFORMATION or PROCESS_QUERY_LIMITED_INFORMATION access right. 
    ///     ''' For more information, see Process Security and Access Rights.Windows Server 2003 and Windows XP:  
    ///     ''' The handle must have the PROCESS_QUERY_INFORMATION access right.</param>
    ///     ''' <param name="wow64Process">A pointer to a value that is set to TRUE if the process is running under WOW64. If the process is running under 
    ///     ''' 32-bit Windows, the value is set to FALSE. If the process is a 64-bit application running under 64-bit Windows, the value is also set to FALSE.</param>
    ///     ''' <returns>If the function succeeds, the return value is a nonzero value. If the function fails, the return value is zero. To get extended 
    ///     ''' error information, call GetLastError.</returns>
    ///     ''' <remarks></remarks>
    [DllImport("Kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    private static bool IsWow64Process(System.IntPtr hProcess, ref bool wow64Process)
    {
    }

    /// <summary>
    ///     ''' Return a message when a driver is not compatible with the requested 32/64bit application type. Returns an empty string if the driver is compatible
    ///     ''' </summary>
    ///     ''' <param name="ProgID">ProgID of the driver to be assessed</param>
    ///     ''' <param name="RequiredBitness">Application bitness for which application compatibility should be tested</param>
    ///     ''' <returns>String compatibility message or empty string if driver is fully compatible</returns>
    ///     ''' <remarks></remarks>
    internal static string DriverCompatibilityMessage(string ProgID, Bitness RequiredBitness, TraceLogger TL)
    {
        PEReader InProcServer = null;
        bool Registered64Bit;
        Bitness InprocServerBitness;
        RegistryKey RK, RKInprocServer32;
        string CLSID, InprocFilePath, CodeBase;
        RegistryKey RK32 = null;
        RegistryKey RK64 = null;
        string AssemblyFullName;
        Assembly LoadedAssembly;
        PortableExecutableKinds peKind;
        ImageFileMachine machine;
        Module[] Modules;

        using (RegistryAccess ProfileStore = new RegistryAccess("DriverCompatibilityMessage")) // Get access to the profile store
        {
            DriverCompatibilityMessage = ""; // Set default return value as OK
            TL.LogMessage("DriverCompatibility", "     ProgID: " + ProgID + ", Bitness: " + RequiredBitness.ToString());
            // Parse the COM registry section to determine whether this ProgID is an in-process DLL server.
            // If it is then parse the executable to determine whether it is a 32bit only driver and gie a suitable message if it is
            // Picks up some COM registration issues as well as a by-product.
            if (RequiredBitness == Bitness.Bits64)
            {
                RK = Registry.ClassesRoot.OpenSubKey(ProgID + @"\CLSID", false); // Look in the 64bit section first
                if (!RK == null)
                {
                    CLSID = RK.GetValue("").ToString(); // Get the CLSID for this ProgID
                    RK.Close();

                    RK = Registry.ClassesRoot.OpenSubKey(@"CLSID\" + CLSID); // Check the 64bit registry section for this CLSID
                    if (RK == null)
                    {
                        TL.LogMessage("DriverCompatibility", "     No entry in the 64bit registry, checking the 32bit registry");
                        RK = Registry.ClassesRoot.OpenSubKey(@"Wow6432Node\CLSID\" + CLSID); // Check the 32bit registry section
                        Registered64Bit = false;
                    }
                    else
                    {
                        TL.LogMessage("DriverCompatibility", "     Found entry in the 64bit registry");
                        Registered64Bit = true;
                    }
                    if (!RK == null)
                    {
                        RKInprocServer32 = RK.OpenSubKey("InprocServer32");
                        RK.Close();
                        if (!RKInprocServer32 == null)
                        {
                            InprocFilePath = RKInprocServer32.GetValue("", "").ToString(); // Get the file location from the default position
                            CodeBase = RKInprocServer32.GetValue("CodeBase", "").ToString(); // Get the codebase if present to override the default value
                            if (CodeBase != "")
                                InprocFilePath = CodeBase;

                            if ((Strings.Trim(InprocFilePath).ToUpperInvariant() == "MSCOREE.DLL"))
                            {
                                // If this assembly is in the GAC, we should have an "Assembly" registry entry with the full assmbly name, 
                                TL.LogMessage("DriverCompatibility", "     Found MSCOREE.DLL");

                                AssemblyFullName = RKInprocServer32.GetValue("Assembly", "").ToString(); // Get the full name
                                TL.LogMessage("DriverCompatibility", "     Found full name: " + AssemblyFullName);
                                if (AssemblyFullName != "")
                                {
                                    try
                                    {
                                        LoadedAssembly = Assembly.ReflectionOnlyLoad(AssemblyFullName);
                                        // OK that wen't well so we have an MSIL version!
                                        InprocFilePath = LoadedAssembly.CodeBase; // Get the codebase for testing below
                                        TL.LogMessage("DriverCompatibilityMSIL", "     Found file path: " + InprocFilePath);
                                        TL.LogMessage("DriverCompatibilityMSIL", "     Found full name: " + LoadedAssembly.FullName + " ");
                                        Modules = LoadedAssembly.GetLoadedModules();
                                        Modules[0].GetPEKind(out peKind, out machine);
                                        if ((peKind & PortableExecutableKinds.Required32Bit) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind Required32bit");
                                        if ((peKind & PortableExecutableKinds.PE32Plus) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind PE32Plus");
                                        if ((peKind & PortableExecutableKinds.ILOnly) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind ILOnly");
                                        if ((peKind & PortableExecutableKinds.NotAPortableExecutableImage) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind Not PE Executable");
                                    }
                                    catch (IOException ex)
                                    {
                                        // That failed so try to load an x86 version
                                        TL.LogMessageCrLf("DriverCompatibility", "Could not find file, trying x86 version - " + ex.Message);

                                        try
                                        {
                                            LoadedAssembly = Assembly.ReflectionOnlyLoad(AssemblyFullName + ", processorArchitecture=x86");
                                            // OK that wen't well so we have an x86 only version!
                                            InprocFilePath = LoadedAssembly.CodeBase; // Get the codebase for testing below
                                            TL.LogMessage("DriverCompatibilityX86", "     Found file path: " + InprocFilePath);
                                            Modules = LoadedAssembly.GetLoadedModules();
                                            Modules[0].GetPEKind(out peKind, out machine);
                                            if ((peKind & PortableExecutableKinds.Required32Bit) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind Required32bit");
                                            if ((peKind & PortableExecutableKinds.PE32Plus) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind PE32Plus");
                                            if ((peKind & PortableExecutableKinds.ILOnly) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind ILOnly");
                                            if ((peKind & PortableExecutableKinds.NotAPortableExecutableImage) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind Not PE Executable");
                                        }
                                        catch (IOException ex1)
                                        {
                                            // That failed so try to load an x64 version
                                            TL.LogMessageCrLf("DriverCompatibilityX64", "Could not find file, trying x64 version - " + ex.Message);

                                            try
                                            {
                                                LoadedAssembly = Assembly.ReflectionOnlyLoad(AssemblyFullName + ", processorArchitecture=x64");
                                                // OK that wen't well so we have an x64 only version!
                                                InprocFilePath = LoadedAssembly.CodeBase; // Get the codebase for testing below
                                                TL.LogMessage("DriverCompatibilityX64", "     Found file path: " + InprocFilePath);
                                                Modules = LoadedAssembly.GetLoadedModules();
                                                Modules[0].GetPEKind(out peKind, out machine);
                                                if ((peKind & PortableExecutableKinds.Required32Bit) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind Required32bit");
                                                if ((peKind & PortableExecutableKinds.PE32Plus) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind PE32Plus");
                                                if ((peKind & PortableExecutableKinds.ILOnly) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind ILOnly");
                                                if ((peKind & PortableExecutableKinds.NotAPortableExecutableImage) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind Not PE Executable");
                                            }
                                            catch (Exception ex2)
                                            {
                                                // Ignore exceptions here and leave MSCOREE.DLL as the InprocFilePath, this will fail below and generate an "incompatible driver" message
                                                TL.LogMessageCrLf("DriverCompatibilityX64", ex1.ToString());
                                            }
                                        }

                                        catch (Exception ex1)
                                        {
                                            // Ignore exceptions here and leave MSCOREE.DLL as the InprocFilePath, this will fail below and generate an "incompatible driver" message
                                            TL.LogMessageCrLf("DriverCompatibilityX32", ex1.ToString());
                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        // Ignore exceptions here and leave MSCOREE.DLL as the InprocFilePath, this will fail below and generate an "incompatible driver" message
                                        TL.LogMessageCrLf("DriverCompatibility", ex.ToString());
                                    }
                                }
                                else
                                {
                                    // No Assembly entry so we can't load the assembly, we'll just have to take a chance!
                                    TL.LogMessage("DriverCompatibility", "'AssemblyFullName is null so we can't load the assembly, we'll just have to take a chance!");
                                    InprocFilePath = ""; // Set to null to bypass tests
                                    TL.LogMessage("DriverCompatibility", "     Set InprocFilePath to null string");
                                }
                            }

                            if ((Strings.Right(Strings.Trim(InprocFilePath), 4).ToUpperInvariant() == ".DLL"))
                            {
                                // We have an assembly or other technology DLL, outside the GAC, in the file system
                                try
                                {
                                    InProcServer = new PEReader(InprocFilePath, TL); // Get hold of the executable so we can determine its characteristics
                                    InprocServerBitness = InProcServer.BitNess;
                                    if (InprocServerBitness == Bitness.Bits32)
                                    {
                                        if (Registered64Bit)
                                            DriverCompatibilityMessage = "This 32bit only driver won't work in a 64bit application even though it is registered as a 64bit COM driver." + Constants.vbCrLf + DRIVER_AUTHOR_MESSAGE_DRIVER;
                                        else
                                            DriverCompatibilityMessage = "This 32bit only driver won't work in a 64bit application even though it is correctly registered as a 32bit COM driver." + Constants.vbCrLf + DRIVER_AUTHOR_MESSAGE_DRIVER;
                                    }
                                    else if (Registered64Bit)
                                    {
                                    }
                                    else
                                        DriverCompatibilityMessage = "This 64bit capable driver is only registered as a 32bit COM driver." + Constants.vbCrLf + DRIVER_AUTHOR_MESSAGE_INSTALLER;
                                }
                                catch (FileNotFoundException ex)
                                {
                                    DriverCompatibilityMessage = "Cannot find the driver executable: " + Constants.vbCrLf + "\"" + InprocFilePath + "\"";
                                }
                                catch (Exception ex)
                                {
                                    LogEvent("DriverCompatibilityMessage", "Exception parsing " + ProgID + ", \"" + InprocFilePath + "\"", EventLogEntryType.Error, EventLogErrors.DriverCompatibilityException, ex.ToString());
                                    DriverCompatibilityMessage = "PEReader Exception, please check ASCOM application Event Log for details";
                                }

                                if (!InProcServer == null)
                                {
                                    InProcServer.Dispose();
                                    InProcServer = null;
                                }
                            }
                            else
                                // No codebase so can't test this driver, don't give an error message, just have to take a chance!
                                TL.LogMessage("DriverCompatibility", "No codebase so can't test this driver, don't give an error message, just have to take a chance!");
                            RKInprocServer32.Close(); // Clean up the InProcServer registry key
                        }
                        else
                        {
                        }
                    }
                    else
                        DriverCompatibilityMessage = "Unable to find a CLSID entry for this driver, please re-install.";
                }
                else
                    DriverCompatibilityMessage = "This driver is not registered for COM (can't find ProgID), please re-install.";
            }
            else
            {
                RK = Registry.ClassesRoot.OpenSubKey(ProgID + @"\CLSID", false); // Look in the 32bit registry

                if (!RK == null)
                {
                    TL.LogMessage("DriverCompatibility", "     Found 32bit ProgID registration");
                    CLSID = RK.GetValue("").ToString(); // Get the CLSID for this ProgID
                    RK.Close();
                    RK = null;

                    if (OSBits() == Bitness.Bits64)
                    {
                        try
                        {
                            RK32 = ProfileStore.OpenSubKey3264(Registry.ClassesRoot, @"CLSID\" + CLSID, false, RegistryAccess.RegWow64Options.KEY_WOW64_32KEY);
                        }
                        catch (Exception ex)
                        {
                        }// Ignore any exceptions, they just mean the operation wasn't successful

                        try
                        {
                            RK64 = ProfileStore.OpenSubKey3264(Registry.ClassesRoot, @"CLSID\" + CLSID, false, RegistryAccess.RegWow64Options.KEY_WOW64_64KEY);
                        }
                        catch (Exception ex)
                        {
                        }// Ignore any exceptions, they just mean the operation wasn't successful
                    }
                    else
                    {
                        RK = Registry.ClassesRoot.OpenSubKey(@"CLSID\" + CLSID); // Check the 32bit registry section for this CLSID
                        TL.LogMessage("DriverCompatibility", "     Running on a 32bit OS, 32Bit Registered: " + (!RK == null));
                    }

                    if (OSBits() == Bitness.Bits64)
                    {
                        TL.LogMessage("DriverCompatibility", "     Running on a 64bit OS, 32bit Registered: " + (!RK32 == null) + ", 64Bit Registered: " + (!RK64 == null));
                        if (!RK32 == null)
                            RK = RK32;
                        else
                            RK = RK64;
                    }

                    if (!RK == null)
                    {
                        TL.LogMessage("DriverCompatibility", "     Found CLSID entry");
                        RKInprocServer32 = RK.OpenSubKey("InprocServer32");
                        RK.Close();
                        if (!RKInprocServer32 == null)
                        {
                            InprocFilePath = RKInprocServer32.GetValue("", "").ToString(); // Get the file location from the default position
                            CodeBase = RKInprocServer32.GetValue("CodeBase", "").ToString(); // Get the codebase if present to override the default value
                            if (CodeBase != "")
                                InprocFilePath = CodeBase;

                            if ((Strings.Trim(InprocFilePath).ToUpperInvariant() == "MSCOREE.DLL"))
                            {
                                // If this assembly is in the GAC, we should have an "Assembly" registry entry with the full assmbly name, 
                                TL.LogMessage("DriverCompatibility", "     Found MSCOREE.DLL");

                                AssemblyFullName = RKInprocServer32.GetValue("Assembly", "").ToString(); // Get the full name
                                TL.LogMessage("DriverCompatibility", "     Found full name: " + AssemblyFullName);
                                if (AssemblyFullName != "")
                                {
                                    try
                                    {
                                        LoadedAssembly = Assembly.ReflectionOnlyLoad(AssemblyFullName);
                                        // OK that wen't well so we have an MSIL version!
                                        InprocFilePath = LoadedAssembly.CodeBase; // Get the codebase for testing below
                                        TL.LogMessage("DriverCompatibilityMSIL", "     Found file path: " + InprocFilePath);
                                        TL.LogMessage("DriverCompatibilityMSIL", "     Found full name: " + LoadedAssembly.FullName + " ");
                                        Modules = LoadedAssembly.GetLoadedModules();
                                        Modules[0].GetPEKind(out peKind, out machine);
                                        if ((peKind & PortableExecutableKinds.Required32Bit) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind Required32bit");
                                        if ((peKind & PortableExecutableKinds.PE32Plus) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind PE32Plus");
                                        if ((peKind & PortableExecutableKinds.ILOnly) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind ILOnly");
                                        if ((peKind & PortableExecutableKinds.NotAPortableExecutableImage) != 0)
                                            TL.LogMessage("DriverCompatibilityMSIL", "     Kind Not PE Executable");
                                    }
                                    catch (IOException ex)
                                    {
                                        // That failed so try to load an x86 version
                                        TL.LogMessageCrLf("DriverCompatibility", "Could not find file, trying x86 version - " + ex.Message);

                                        try
                                        {
                                            LoadedAssembly = Assembly.ReflectionOnlyLoad(AssemblyFullName + ", processorArchitecture=x86");
                                            // OK that wen't well so we have an x86 only version!
                                            InprocFilePath = LoadedAssembly.CodeBase; // Get the codebase for testing below
                                            TL.LogMessage("DriverCompatibilityX86", "     Found file path: " + InprocFilePath);
                                            Modules = LoadedAssembly.GetLoadedModules();
                                            Modules[0].GetPEKind(out peKind, out machine);
                                            if ((peKind & PortableExecutableKinds.Required32Bit) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind Required32bit");
                                            if ((peKind & PortableExecutableKinds.PE32Plus) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind PE32Plus");
                                            if ((peKind & PortableExecutableKinds.ILOnly) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind ILOnly");
                                            if ((peKind & PortableExecutableKinds.NotAPortableExecutableImage) != 0)
                                                TL.LogMessage("DriverCompatibilityX86", "     Kind Not PE Executable");
                                        }
                                        catch (IOException ex1)
                                        {
                                            // That failed so try to load an x64 version
                                            TL.LogMessageCrLf("DriverCompatibilityX64", "Could not find file, trying x64 version - " + ex.Message);

                                            try
                                            {
                                                LoadedAssembly = Assembly.ReflectionOnlyLoad(AssemblyFullName + ", processorArchitecture=x64");
                                                // OK that wen't well so we have an x64 only version!
                                                InprocFilePath = LoadedAssembly.CodeBase; // Get the codebase for testing below
                                                TL.LogMessage("DriverCompatibilityX64", "     Found file path: " + InprocFilePath);
                                                Modules = LoadedAssembly.GetLoadedModules();
                                                Modules[0].GetPEKind(out peKind, out machine);
                                                if ((peKind & PortableExecutableKinds.Required32Bit) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind Required32bit");
                                                if ((peKind & PortableExecutableKinds.PE32Plus) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind PE32Plus");
                                                if ((peKind & PortableExecutableKinds.ILOnly) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind ILOnly");
                                                if ((peKind & PortableExecutableKinds.NotAPortableExecutableImage) != 0)
                                                    TL.LogMessage("DriverCompatibilityX64", "     Kind Not PE Executable");
                                            }
                                            catch (Exception ex2)
                                            {
                                                // Ignore exceptions here and leave MSCOREE.DLL as the InprocFilePath, this will fail below and generate an "incompatible driver" message
                                                TL.LogMessageCrLf("DriverCompatibilityX64", ex1.ToString());
                                            }
                                        }

                                        catch (Exception ex1)
                                        {
                                            // Ignore exceptions here and leave MSCOREE.DLL as the InprocFilePath, this will fail below and generate an "incompatible driver" message
                                            TL.LogMessageCrLf("DriverCompatibilityX32", ex1.ToString());
                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        // Ignore exceptions here and leave MSCOREE.DLL as the InprocFilePath, this will fail below and generate an "incompatible driver" message
                                        TL.LogMessageCrLf("DriverCompatibility", ex.ToString());
                                    }
                                }
                                else
                                {
                                    // No Assembly entry so we can't load the assembly, we'll just have to take a chance!
                                    TL.LogMessage("DriverCompatibility", "'AssemblyFullName is null so we can't load the assembly, we'll just have to take a chance!");
                                    InprocFilePath = ""; // Set to null to bypass tests
                                    TL.LogMessage("DriverCompatibility", "     Set InprocFilePath to null string");
                                }
                            }

                            if ((Strings.Right(Strings.Trim(InprocFilePath), 4).ToUpperInvariant() == ".DLL"))
                            {
                                // We have an assembly or other technology DLL, outside the GAC, in the file system
                                try
                                {
                                    InProcServer = new PEReader(InprocFilePath, TL); // Get hold of the executable so we can determine its characteristics
                                    if (InProcServer.BitNess == Bitness.Bits64)
                                        DriverCompatibilityMessage = "This is a 64bit only driver and is not compatible with this 32bit application." + Constants.vbCrLf + DRIVER_AUTHOR_MESSAGE_DRIVER;
                                }
                                catch (FileNotFoundException ex)
                                {
                                    DriverCompatibilityMessage = "Cannot find the driver executable: " + Constants.vbCrLf + "\"" + InprocFilePath + "\"";
                                }
                                catch (Exception ex)
                                {
                                    LogEvent("DriverCompatibilityMessage", "Exception parsing " + ProgID + ", \"" + InprocFilePath + "\"", EventLogEntryType.Error, EventLogErrors.DriverCompatibilityException, ex.ToString());
                                    DriverCompatibilityMessage = "PEReader Exception, please check ASCOM application Event Log for details";
                                }

                                if (!InProcServer == null)
                                {
                                    InProcServer.Dispose();
                                    InProcServer = null;
                                }
                            }
                            else
                                // No codebase or not a DLL so can't test this driver, don't give an error message, just have to take a chance!
                                TL.LogMessage("DriverCompatibility", "No codebase or not a DLL so can't test this driver, don't give an error message, just have to take a chance!");
                            RKInprocServer32.Close(); // Clean up the InProcServer registry key
                        }
                        else
                            // Please leave this empty clause here so the logic is clear!
                            TL.LogMessage("DriverCompatibility", "This is not an inprocess DLL so no need to test further and no error message to return");
                    }
                    else
                    {
                        DriverCompatibilityMessage = "Unable to find a CLSID entry for this driver, please re-install.";
                        TL.LogMessage("DriverCompatibility", "     Could not find CLSID entry!");
                    }
                }
                else
                    DriverCompatibilityMessage = "This driver is not registered for COM (can't find ProgID), please re-install.";
            }
        }
        TL.LogMessage("DriverCompatibility", "     Returning: \"" + DriverCompatibilityMessage + "\"");
        return DriverCompatibilityMessage;
    }
}



// Try 'Get the list of 32bit only drivers
// Drivers32Bit = ProfileStore.EnumProfile(DRIVERS_32BIT)
// Catch ex1 As Exception
// Ignore any exceptions from this call e.g. if there are no 32bit only devices installed
// Just create an empty list
// Drivers32Bit = New Generic.SortedList(Of String, String)
// LogEvent("ChooserForm", "Exception creating SortedList of 32bit only applications", EventLogEntryType.Error, EventLogErrors.Chooser32BitOnlyException, ex1.ToString)
// End Try

// Try 'Get the list of 64bit only drivers
// Drivers64Bit = ProfileStore.EnumProfile(DRIVERS_64BIT)
// Catch ex1 As Exception
// Ignore any exceptions from this call e.g. if there are no 64bit only devices installed
// Just create an empty list
// Drivers64Bit = New Generic.SortedList(Of String, String)
// LogEvent("ChooserForm", "Exception creating SortedList of 64bit only applications", EventLogEntryType.Error, EventLogErrors.Chooser64BitOnlyException, ex1.ToString)
// End Try

// If (ApplicationBits() = Bitness.Bits64) And (Drivers32Bit.ContainsKey(ProgID)) Then 'This is a 32bit driver being accessed by a 64bit application
// DriverCompatibilityMessage = "This 32bit driver is not compatible with your 64bit application." & vbCrLf & _
// "Please contact the driver author to see if there is a 64bit compatible version."
// End If
// If (ApplicationBits() = Bitness.Bits32) And (Drivers64Bit.ContainsKey(ProgID)) Then 'This is a 64bit driver being accessed by a 32bit application
// DriverCompatibilityMessage = "This 64bit driver is not compatible with your 32bit application." & vbCrLf & _
// "Please contact the driver author to see if there is a 32bit compatible version."
// End If



static class AscomSharedCode
{
    internal static string ConditionPlatformVersion(string PlatformVersion, RegistryAccess Profile, TraceLogger TL)
    {
        string ModuleFileName, ForcedFileNameKey;
        SortedList<string, string> ForcedFileNames, ForcedSeparators;
        PerformanceCounter PC;

        ConditionPlatformVersion = PlatformVersion; // Set default action to return the supplied vaue
        try
        {
            ModuleFileName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName); // Get the name of the executable without path or file extension
            if (!TL == null)
                TL.LogMessage("ConditionPlatformVersion", "  ModuleFileName: \"" + ModuleFileName + "\" \"" + Process.GetCurrentProcess().MainModule.FileName + "\"");
            if (Strings.Left(ModuleFileName.ToUpperInvariant(), 3) == "IS-")
            {
                if (!TL == null)
                    TL.LogMessage("ConditionPlatformVersion", "    Inno installer temporary executable detected, searching for parent process!");
                if (!TL == null)
                    TL.LogMessage("ConditionPlatformVersion", "    Old Module Filename: " + ModuleFileName);
                PC = new PerformanceCounter("Process", "Creating Process ID", Process.GetCurrentProcess().ProcessName);
                ModuleFileName = Path.GetFileNameWithoutExtension(Process.GetProcessById(System.Convert.ToInt32(PC.NextValue())).MainModule.FileName);
                if (!TL == null)
                    TL.LogMessage("ConditionPlatformVersion", "    New Module Filename: " + ModuleFileName);
                PC.Close();
                PC.Dispose();
            }

            // Force any particular platform version number this application requires
            ForcedFileNames = Profile.EnumProfile(PLATFORM_VERSION_EXCEPTIONS); // Get the list of filenames requiring specific versions

            foreach (KeyValuePair<string, string> ForcedFileName in ForcedFileNames) // Check each forced file in turn 
            {
                if (!TL == null)
                    TL.LogMessage("ConditionPlatformVersion", "  ForcedFileName: \"" + ForcedFileName.Key + "\" \"" + ForcedFileName.Value + "\" \"" + Strings.UCase(Path.GetFileNameWithoutExtension(ForcedFileName.Key)) + "\" \"" + Strings.UCase(Path.GetFileName(ForcedFileName.Key)) + "\" \"" + Strings.UCase(ForcedFileName.Key) + "\" \"" + ForcedFileName.Key + "\" \"" + Strings.UCase(ModuleFileName) + "\"");
                if (ForcedFileName.Key.Contains("."))
                    ForcedFileNameKey = Path.GetFileNameWithoutExtension(ForcedFileName.Key);
                else
                    ForcedFileNameKey = ForcedFileName.Key;

                // If the current file matches a forced file name then return the required Platform version
                // 6.0 SP1 Check now uses StartsWith in order to catch situations where people rename the installer after download
                if (ForcedFileNameKey != "")
                {
                    if (ModuleFileName.StartsWith(ForcedFileNameKey, StringComparison.OrdinalIgnoreCase))
                    {
                        ConditionPlatformVersion = ForcedFileName.Value;
                        if (!TL == null)
                            TL.LogMessage("ConditionPlatformVersion", "  Matched file: \"" + ModuleFileName + "\" \"" + ForcedFileNameKey + "\"");
                    }
                }
            }

            ForcedSeparators = Profile.EnumProfile(PLATFORM_VERSION_SEPARATOR_EXCEPTIONS); // Get the list of filenames requiring specific versions

            foreach (KeyValuePair<string, string> ForcedSeparator in ForcedSeparators) // Check each forced file in turn 
            {
                if (!TL == null)
                    TL.LogMessage("ConditionPlatformVersion", "  ForcedFileName: \"" + ForcedSeparator.Key + "\" \"" + ForcedSeparator.Value + "\" \"" + Strings.UCase(Path.GetFileNameWithoutExtension(ForcedSeparator.Key)) + "\" \"" + Strings.UCase(Path.GetFileName(ForcedSeparator.Key)) + "\" \"" + Strings.UCase(ForcedSeparator.Key) + "\" \"" + ForcedSeparator.Key + "\" \"" + Strings.UCase(ModuleFileName) + "\"");
                if (ForcedSeparator.Key.Contains("."))
                {
                }
                else
                {
                }

                if (Strings.UCase(Path.GetFileNameWithoutExtension(ForcedSeparator.Key)) == Strings.UCase(ModuleFileName))
                {
                    if (string.IsNullOrEmpty(ForcedSeparator.Value))
                    {
                        ConditionPlatformVersion = ConditionPlatformVersion.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator());
                        if (!TL == null)
                            TL.LogMessage("ConditionPlatformVersion", "  String IsNullOrEmpty: \"" + ConditionPlatformVersion + "\"");
                    }
                    else
                    {
                        ConditionPlatformVersion = ConditionPlatformVersion.Replace(".", ForcedSeparator.Value);
                        if (!TL == null)
                            TL.LogMessage("ConditionPlatformVersion", "  String Is: \"" + ForcedSeparator.Value + "\" \"" + ConditionPlatformVersion + "\"");
                    }

                    if (!TL == null)
                        TL.LogMessage("ConditionPlatformVersion", "  Matched file: \"" + ModuleFileName + "\" \"" + ForcedSeparator.Key + "\"");
                }
            }
        }
        catch (Exception ex)
        {
            if (!TL == null)
                TL.LogMessageCrLf("ConditionPlatformVersion", "Exception: " + ex.ToString());
            LogEvent("ConditionPlatformVersion", "Exception: ", EventLogEntryType.Error, EventLogErrors.VB6HelperProfileException, ex.ToString());
        }
        if (!TL == null)
            TL.LogMessage("ConditionPlatformVersion", "  Returning: \"" + ConditionPlatformVersion + "\"");
    }
}



/// <summary>

/// ''' 

/// ''' </summary>
internal class PEReader : IDisposable
{
    internal const int CLR_HEADER = 14; // Header number of the CLR information, if present
    private const int MAX_HEADERS_TO_CHECK = 1000; // Safety limit to ensure that we don't lock up the machine if we get a PE image that indicates it has a huge number of header directories

    // Possible error codes when an assembly is loaded for reflection
    private const int COR_E_BADIMAGEFORMAT = 0x8007000B;
    private const int CLDB_E_FILE_OLDVER = 0x80131107;
    private const int CLDB_E_INDEX_NOTFOUND = 0x80131124;
    private const int CLDB_E_FILE_CORRUPT = 0x8013110E;
    private const int COR_E_NEWER_RUNTIME = 0x8013101B;
    private const int COR_E_ASSEMBLYEXPECTED = 0x80131018;
    private const int ERROR_BAD_EXE_FORMAT = 0x800700C1;
    private const int ERROR_EXE_MARKED_INVALID = 0x800700C0;
    private const int CORSEC_E_INVALID_IMAGE_FORMAT = 0x8013141D;
    private const int ERROR_NOACCESS = 0x800703E6;
    private const int ERROR_INVALID_ORDINAL = 0x800700B6;
    private const int ERROR_INVALID_DLL = 0x80070482;
    private const int ERROR_FILE_CORRUPT = 0x80070570;
    private const int COR_E_LOADING_REFERENCE_ASSEMBLY = 0x80131058;
    private const int META_E_BAD_SIGNATURE = 0x80131192;

    // Executable machine types
    private const ushort IMAGE_FILE_MACHINE_I386 = 0x14; // x86
    private const ushort IMAGE_FILE_MACHINE_IA64 = 0x200; // Intel(Itanium)
    private const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664; // x64


    internal enum CLR_FLAGS
    {
        CLR_FLAGS_ILONLY = 0x1,
        CLR_FLAGS_32BITREQUIRED = 0x2,
        CLR_FLAGS_IL_LIBRARY = 0x4,
        CLR_FLAGS_STRONGNAMESIGNED = 0x8,
        CLR_FLAGS_NATIVE_ENTRYPOINT = 0x10,
        CLR_FLAGS_TRACKDEBUGDATA = 0x10000
    }

    internal enum SubSystemType
    {
        NATIVE = 1 // The binary doesn't need a subsystem. This is used for drivers.
,
        WINDOWS_GUI = 2 // The image is a Win32 graphical binary. (It can still open a console with AllocConsole() but won't get one automatically at startup.)
,
        WINDOWS_CUI = 3 // The binary is a Win32 console binary. (It will get a console per default at startup, or inherit the parent's console.)
,
        UNKNOWN_4 = 4 // Unknown allocation
,
        OS2_CUI = 5 // The binary is a OS/2 console binary. (OS/2 binaries will be in OS/2 format, so this value will seldom be used in a PE file.)
,
        UNKNOWN_6 = 6 // Unknown allocation
,
        POSIX_CUI = 7 // The binary uses the POSIX console subsystem.
,
        NATIVE_WINDOWS = 8,
        WINDOWS_CE_GUI = 9,
        EFI_APPLICATION = 10 // Extensible Firmware Interface (EFI) application.
,
        EFI_BOOT_SERVICE_DRIVER = 11 // EFI driver with boot services.
,
        EFI_RUNTIME_DRIVER = 12 // EFI driver with run-time services.
,
        EFI_ROM = 13 // EFI ROM image.
,
        XBOX = 14 // Xbox sy stem.
,
        UNKNOWN_15 = 15 // Unknown allocation
,
        WINDOWS_BOOT_APPLICATION = 16 // Boot application.
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_DOS_HEADER
    {
        internal UInt16 e_magic; // Magic number
        internal UInt16 e_cblp; // Bytes on last page of file
        internal UInt16 e_cp; // Pages in file
        internal UInt16 e_crlc; // Relocations
        internal UInt16 e_cparhdr; // Size of header in paragraphs
        internal UInt16 e_minalloc; // Minimum extra paragraphs needed
        internal UInt16 e_maxalloc; // Maximum extra paragraphs needed
        internal UInt16 e_ss; // Initial (relative) SS value
        internal UInt16 e_sp; // Initial SP value
        internal UInt16 e_csum; // Checksum
        internal UInt16 e_ip; // Initial IP value
        internal UInt16 e_cs; // Initial (relative) CS value
        internal UInt16 e_lfarlc; // File address of relocation table
        internal UInt16 e_ovno; // Overlay number
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal UInt16[] e_res1; // Reserved words
        internal UInt16 e_oemid; // OEM identifier (for e_oeminfo)
        internal UInt16 e_oeminfo; // 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        internal UInt16[] e_res2; // Reserved words
        internal UInt32 e_lfanew; // File address of new exe header
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_NT_HEADERS
    {
        internal UInt32 Signature;
        internal IMAGE_FILE_HEADER FileHeader;
        internal IMAGE_OPTIONAL_HEADER32 OptionalHeader32;
        internal IMAGE_OPTIONAL_HEADER64 OptionalHeader64;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_FILE_HEADER
    {
        internal UInt16 Machine;
        internal UInt16 NumberOfSections;
        internal UInt32 TimeDateStamp;
        internal UInt32 PointerToSymbolTable;
        internal UInt32 NumberOfSymbols;
        internal UInt16 SizeOfOptionalHeader;
        internal UInt16 Characteristics;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_OPTIONAL_HEADER32
    {
        internal UInt16 Magic;
        internal Byte MajorLinkerVersion;
        internal Byte MinorLinkerVersion;
        internal UInt32 SizeOfCode;
        internal UInt32 SizeOfInitializedData;
        internal UInt32 SizeOfUninitializedData;
        internal UInt32 AddressOfEntryPoint;
        internal UInt32 BaseOfCode;
        internal UInt32 BaseOfData;
        internal UInt32 ImageBase;
        internal UInt32 SectionAlignment;
        internal UInt32 FileAlignment;
        internal UInt16 MajorOperatingSystemVersion;
        internal UInt16 MinorOperatingSystemVersion;
        internal UInt16 MajorImageVersion;
        internal UInt16 MinorImageVersion;
        internal UInt16 MajorSubsystemVersion;
        internal UInt16 MinorSubsystemVersion;
        internal UInt32 Win32VersionValue;
        internal UInt32 SizeOfImage;
        internal UInt32 SizeOfHeaders;
        internal UInt32 CheckSum;
        internal UInt16 Subsystem;
        internal UInt16 DllCharacteristics;
        internal UInt32 SizeOfStackReserve;
        internal UInt32 SizeOfStackCommit;
        internal UInt32 SizeOfHeapReserve;
        internal UInt32 SizeOfHeapCommit;
        internal UInt32 LoaderFlags;
        internal UInt32 NumberOfRvaAndSizes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_OPTIONAL_HEADER64
    {
        internal UInt16 Magic;
        internal Byte MajorLinkerVersion;
        internal Byte MinorLinkerVersion;
        internal UInt32 SizeOfCode;
        internal UInt32 SizeOfInitializedData;
        internal UInt32 SizeOfUninitializedData;
        internal UInt32 AddressOfEntryPoint;
        internal UInt32 BaseOfCode;
        internal UInt64 ImageBase;
        internal UInt32 SectionAlignment;
        internal UInt32 FileAlignment;
        internal UInt16 MajorOperatingSystemVersion;
        internal UInt16 MinorOperatingSystemVersion;
        internal UInt16 MajorImageVersion;
        internal UInt16 MinorImageVersion;
        internal UInt16 MajorSubsystemVersion;
        internal UInt16 MinorSubsystemVersion;
        internal UInt32 Win32VersionValue;
        internal UInt32 SizeOfImage;
        internal UInt32 SizeOfHeaders;
        internal UInt32 CheckSum;
        internal UInt16 Subsystem;
        internal UInt16 DllCharacteristics;
        internal UInt64 SizeOfStackReserve;
        internal UInt64 SizeOfStackCommit;
        internal UInt64 SizeOfHeapReserve;
        internal UInt64 SizeOfHeapCommit;
        internal UInt32 LoaderFlags;
        internal UInt32 NumberOfRvaAndSizes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal IMAGE_DATA_DIRECTORY[] DataDirectory;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_DATA_DIRECTORY
    {
        internal UInt32 VirtualAddress;
        internal UInt32 Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_SECTION_HEADER
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        internal string Name;
        internal Misc Misc;
        internal UInt32 VirtualAddress;
        internal UInt32 SizeOfRawData;
        internal UInt32 PointerToRawData;
        internal UInt32 PointerToRelocations;
        internal UInt32 PointerToLinenumbers;
        internal UInt16 NumberOfRelocations;
        internal UInt16 NumberOfLinenumbers;
        internal UInt32 Characteristics;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct Misc
    {
        [FieldOffset(0)]
        internal UInt32 PhysicalAddress;
        [FieldOffset(0)]
        internal UInt32 VirtualSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IMAGE_COR20_HEADER
    {
        internal UInt32 cb;
        internal UInt16 MajorRuntimeVersion;
        internal UInt16 MinorRuntimeVersion;
        internal IMAGE_DATA_DIRECTORY MetaData;       // // Symbol table and startup information
        internal UInt32 Flags;
        internal UInt32 EntryPointToken;
        internal IMAGE_DATA_DIRECTORY Resources;        // // Binding information
        internal IMAGE_DATA_DIRECTORY StrongNameSignature;
        internal IMAGE_DATA_DIRECTORY CodeManagerTable;        // // Regular fixup and binding information
        internal IMAGE_DATA_DIRECTORY VTableFixups;
        internal IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
        internal IMAGE_DATA_DIRECTORY ManagedNativeHeader;        // // Precompiled image info (internal use only - set to zero)
    }

    private readonly IMAGE_DOS_HEADER dosHeader;
    private IMAGE_NT_HEADERS ntHeaders;
    private readonly IMAGE_COR20_HEADER CLR;
    private readonly Generic.IList<IMAGE_SECTION_HEADER> sectionHeaders = new Generic.List<IMAGE_SECTION_HEADER>();
    private uint TextBase;
    private BinaryReader reader;
    private Stream stream;
    private bool IsAssembly = false;
    private AssemblyName AssemblyInfo;
    private Assembly SuppliedAssembly;
    private string AssemblyDeterminationType;
    private bool OS32BitCompatible = false;
    private VersionCode.Bitness ExecutableBitness;

    private TraceLogger TL;

    internal PEReader(string FileName, TraceLogger TLogger)
    {
        TL = TLogger; // Save the TraceLogger instance we have been passed

        TL.LogMessage("PEReader", "Running within CLR version: " + RuntimeEnvironment.GetSystemVersion());

        if (Strings.Left(FileName, 5).ToUpperInvariant() == "FILE:")
        {
            // Convert uri to local path if required, uri paths are not supported by FileStream - this method allows file names with # characters to be passed through
            Uri u = new Uri(FileName);
            FileName = u.LocalPath + Uri.UnescapeDataString(u.Fragment).Replace("/", @"\\");
        }
        TL.LogMessage("PEReader", "Filename to check: " + FileName);
        if (!File.Exists(FileName))
            throw new FileNotFoundException("PEReader - File not found: " + FileName);

        // Determine whether this is an assembly by testing whether we can load the file as an assembly, if so then it IS an assembly!
        TL.LogMessage("PEReader", "Determining whether this is an assembly");
        try
        {
            SuppliedAssembly = Assembly.ReflectionOnlyLoadFrom(FileName);
            IsAssembly = true; // We got here without an exception so it must be an assembly
            TL.LogMessage("PEReader.IsAssembly", "Found an assembly because it loaded Ok to the reflection context: " + IsAssembly);
        }
        catch (FileNotFoundException ex)
        {
            TL.LogMessage("PEReader.IsAssembly", "FileNotFoundException: File not found so this is NOT an assembly: " + IsAssembly);
        }
        catch (BadImageFormatException ex1)
        {

            // There are multiple reasons why this can occur so now determine what actually happened by examining the hResult
            int hResult = Marshal.GetHRForException(ex1);

            switch (hResult)
            {
                case COR_E_BADIMAGEFORMAT:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - COR_E_BADIMAGEFORMAT. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case CLDB_E_FILE_OLDVER:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - CLDB_E_FILE_OLDVER. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case CLDB_E_INDEX_NOTFOUND:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - CLDB_E_INDEX_NOTFOUND. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case CLDB_E_FILE_CORRUPT:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - CLDB_E_FILE_CORRUPT. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case COR_E_NEWER_RUNTIME // This is an assembly but it requires a newer runtime than is currently running, so flag it as an assembly even though we can't load it
         :
                    {
                        IsAssembly = true;
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - COR_E_NEWER_RUNTIME. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case COR_E_ASSEMBLYEXPECTED:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - COR_E_ASSEMBLYEXPECTED. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case ERROR_BAD_EXE_FORMAT:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - ERROR_BAD_EXE_FORMAT. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case ERROR_EXE_MARKED_INVALID:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - ERROR_EXE_MARKED_INVALID. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case CORSEC_E_INVALID_IMAGE_FORMAT:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - CORSEC_E_INVALID_IMAGE_FORMAT. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case ERROR_NOACCESS:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - ERROR_NOACCESS. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case ERROR_INVALID_ORDINAL:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - ERROR_INVALID_ORDINAL. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case ERROR_INVALID_DLL:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - ERROR_INVALID_DLL. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case ERROR_FILE_CORRUPT:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - ERROR_FILE_CORRUPT. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case COR_E_LOADING_REFERENCE_ASSEMBLY:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - COR_E_LOADING_REFERENCE_ASSEMBLY. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                case META_E_BAD_SIGNATURE:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - META_E_BAD_SIGNATURE. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }

                default:
                    {
                        TL.LogMessage("PEReader.IsAssembly", "BadImageFormatException. hResult: " + hResult.ToString("X8") + " - Meaning of error code is unknown. Setting IsAssembly to: " + IsAssembly);
                        break;
                    }
            }
        }

        catch (FileLoadException ex2)
        {
            IsAssembly = true;
            TL.LogMessage("PEReader.IsAssembly", "FileLoadException: Assembly already loaded so this is an assembly: " + IsAssembly);
        }

        TL.LogMessage("PEReader", "Determining PE Machine type");
        stream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
        reader = new BinaryReader(stream);

        reader.BaseStream.Seek(0, SeekOrigin.Begin); // Reset reader position, just in case
        dosHeader = MarshalBytesTo<IMAGE_DOS_HEADER>(reader); // Read MS-DOS header section
        if (dosHeader.e_magic != 0x5A4D)
            throw new InvalidOperationException("File is not a portable executable.");

        reader.BaseStream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin); // Skip MS-DOS stub and seek reader to NT Headers
        ntHeaders.Signature = MarshalBytesTo<UInt32>(reader); // Read NT Headers
        if (ntHeaders.Signature != 0x4550)
            throw new InvalidOperationException("Invalid portable executable signature in NT header.");
        ntHeaders.FileHeader = MarshalBytesTo<IMAGE_FILE_HEADER>(reader); // Read the IMAGE_FILE_HEADER which starts 4 bytes on from the start of the signature (already here by reading the signature itself)

        // Determine whether this executable is flagged as a 32bit or 64bit and set OS32BitCompatible accordingly
        switch (ntHeaders.FileHeader.Machine)
        {
            case IMAGE_FILE_MACHINE_I386:
                {
                    OS32BitCompatible = true;
                    TL.LogMessage("PEReader.MachineType", "Machine - found \"Intel 32bit\" executable. Characteristics: " + ntHeaders.FileHeader.Characteristics.ToString("X8") + ", OS32BitCompatible: " + OS32BitCompatible);
                    break;
                }

            case IMAGE_FILE_MACHINE_IA64:
                {
                    OS32BitCompatible = false;
                    TL.LogMessage("PEReader.MachineType", "Machine - found \"Itanium 64bit\" executable. Characteristics: " + ntHeaders.FileHeader.Characteristics.ToString("X8") + ", OS32BitCompatible: " + OS32BitCompatible);
                    break;
                }

            case IMAGE_FILE_MACHINE_AMD64:
                {
                    OS32BitCompatible = false;
                    TL.LogMessage("PEReader.MachineType", "Machine - found \"Intel 64bit\" executable. Characteristics: " + ntHeaders.FileHeader.Characteristics.ToString("X8") + ", OS32BitCompatible: " + OS32BitCompatible);
                    break;
                }

            default:
                {
                    TL.LogMessage("PEReader.MachineType", "Found Unknown machine type: " + ntHeaders.FileHeader.Machine.ToString("X8") + ". Characteristics: " + ntHeaders.FileHeader.Characteristics.ToString("X8") + ", OS32BitCompatible: " + OS32BitCompatible);
                    break;
                }
        }

        if (OS32BitCompatible)
        {
            TL.LogMessage("PEReader.MachineType", "Reading optional 32bit header");
            ntHeaders.OptionalHeader32 = MarshalBytesTo<IMAGE_OPTIONAL_HEADER32>(reader);
        }
        else
        {
            TL.LogMessage("PEReader.MachineType", "Reading optional 64bit header");
            ntHeaders.OptionalHeader64 = MarshalBytesTo<IMAGE_OPTIONAL_HEADER64>(reader);
        }

        if (IsAssembly)
        {
            TL.LogMessage("PEReader", "This is an assembly, determining Bitness through the CLR header");
            // Find the CLR header
            int NumberOfHeadersToCheck = MAX_HEADERS_TO_CHECK;
            if (OS32BitCompatible)
            {
                TL.LogMessage("PEReader.Bitness", "This is a 32 bit assembly, reading the CLR Header");
                if (ntHeaders.OptionalHeader32.NumberOfRvaAndSizes < MAX_HEADERS_TO_CHECK)
                    NumberOfHeadersToCheck = System.Convert.ToInt32(ntHeaders.OptionalHeader32.NumberOfRvaAndSizes);
                TL.LogMessage("PEReader.Bitness", "Checking " + NumberOfHeadersToCheck + " headers");

                for (int i = 0; i <= NumberOfHeadersToCheck - 1; i++)
                {
                    if (ntHeaders.OptionalHeader32.DataDirectory[i].Size > 0)
                        sectionHeaders.Add(MarshalBytesTo<IMAGE_SECTION_HEADER>(reader));
                }

                foreach (IMAGE_SECTION_HEADER SectionHeader in sectionHeaders)
                {
                    if (SectionHeader.Name == ".text")
                        TextBase = SectionHeader.PointerToRawData;
                }

                if (NumberOfHeadersToCheck >= CLR_HEADER + 1)
                {
                    if (ntHeaders.OptionalHeader32.DataDirectory[CLR_HEADER].VirtualAddress > 0)
                    {
                        reader.BaseStream.Seek(ntHeaders.OptionalHeader32.DataDirectory[CLR_HEADER].VirtualAddress - ntHeaders.OptionalHeader32.BaseOfCode + TextBase, SeekOrigin.Begin);
                        CLR = MarshalBytesTo<IMAGE_COR20_HEADER>(reader);
                    }
                }
            }
            else
            {
                TL.LogMessage("PEReader.Bitness", "This is a 64 bit assembly, reading the CLR Header");
                if (ntHeaders.OptionalHeader64.NumberOfRvaAndSizes < MAX_HEADERS_TO_CHECK)
                    NumberOfHeadersToCheck = System.Convert.ToInt32(ntHeaders.OptionalHeader64.NumberOfRvaAndSizes);
                TL.LogMessage("PEReader.Bitness", "Checking " + NumberOfHeadersToCheck + " headers");

                for (int i = 0; i <= NumberOfHeadersToCheck - 1; i++)
                {
                    if (ntHeaders.OptionalHeader64.DataDirectory[i].Size > 0)
                        sectionHeaders.Add(MarshalBytesTo<IMAGE_SECTION_HEADER>(reader));
                }

                foreach (IMAGE_SECTION_HEADER SectionHeader in sectionHeaders)
                {
                    if (SectionHeader.Name == ".text")
                    {
                        TL.LogMessage("PEReader.Bitness", "Found TEXT section");
                        TextBase = SectionHeader.PointerToRawData;
                    }
                }

                if (NumberOfHeadersToCheck >= CLR_HEADER + 1)
                {
                    if (ntHeaders.OptionalHeader64.DataDirectory[CLR_HEADER].VirtualAddress > 0)
                    {
                        reader.BaseStream.Seek(ntHeaders.OptionalHeader64.DataDirectory[CLR_HEADER].VirtualAddress - ntHeaders.OptionalHeader64.BaseOfCode + TextBase, SeekOrigin.Begin);
                        CLR = MarshalBytesTo<IMAGE_COR20_HEADER>(reader);
                        TL.LogMessage("PEReader.Bitness", "Read CLR header successfully");
                    }
                }
            }

            // Determine the bitness from the CLR header
            if (OS32BitCompatible)
            {
                if (((CLR.Flags & CLR_FLAGS.CLR_FLAGS_32BITREQUIRED) > 0))
                {
                    TL.LogMessage("PEReader.Bitness", "Found \"32bit Required\" assembly");
                    ExecutableBitness = VersionCode.Bitness.Bits32;
                }
                else
                {
                    TL.LogMessage("PEReader.Bitness", "Found \"MSIL\" assembly");
                    ExecutableBitness = VersionCode.Bitness.BitsMSIL;
                }
            }
            else
            {
                TL.LogMessage("PEReader.Bitness", "Found \"64bit Required\" assembly");
                ExecutableBitness = VersionCode.Bitness.Bits64;
            }

            TL.LogMessage("PEReader", "Assembly required Runtime version: " + CLR.MajorRuntimeVersion + "." + CLR.MinorRuntimeVersion);
        }
        else
        {
            TL.LogMessage("PEReader", "This is not an assembly, determining Bitness through the executable bitness flag");
            if (OS32BitCompatible)
            {
                TL.LogMessage("PEReader.Bitness", "Found 32bit executable");
                ExecutableBitness = VersionCode.Bitness.Bits32;
            }
            else
            {
                TL.LogMessage("PEReader.Bitness", "Found 64bit executable");
                ExecutableBitness = VersionCode.Bitness.Bits64;
            }
        }
    }

    internal VersionCode.Bitness BitNess
    {
        get
        {
            TL.LogMessage("PE.BitNess", "Returning: " + ExecutableBitness);
            return ExecutableBitness;
        }
    }

    internal bool IsDotNetAssembly()
    {
        TL.LogMessage("PE.IsDotNetAssembly", "Returning: " + IsAssembly);
        return IsAssembly;
    }

    internal SubSystemType SubSystem()
    {
        if (OS32BitCompatible)
        {
            TL.LogMessage("PE.SubSystem", "Returning 32bit value: " + (SubSystemType)ntHeaders.OptionalHeader32.Subsystem.ToString());
            return (SubSystemType)ntHeaders.OptionalHeader32.Subsystem; // Return the 32bit header field
        }
        else
        {
            TL.LogMessage("PE.SubSystem", "Returning 64bit value: " + (SubSystemType)ntHeaders.OptionalHeader64.Subsystem.ToString());
            return (SubSystemType)ntHeaders.OptionalHeader64.Subsystem; // Return the 64bit field
        }
    }

    private static T MarshalBytesTo<T>(BinaryReader reader)
    {
        // Unmanaged data
        byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

        // Create a pointer to the unmanaged data pinned in memory to be accessed by unmanaged code
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        // Use our previously created pointer to unmanaged data and marshal to the specified type
        T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

        // Deallocate pointer
        handle.Free();

        return theStructure;
    }

    private bool disposedValue; // To detect redundant calls

    // IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                try
                {
                    reader.Close();
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
                catch (Exception ex)
                {
                }// Swallow any exceptions here
            }
        }
        this.disposedValue = true;
    }

    // This code added by Visual Basic to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}


}