﻿using System;
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
using System.Runtime.InteropServices;
using ASCOM.Utilities.CS.Interfaces;

namespace ASCOM.Utilities.CS
{


    /// <summary>

    /// ''' Class that returns a key and associated value

    /// ''' </summary>

    /// ''' <remarks>This class is used by some Profile properties and methods and

    /// ''' compensates for the inability of .NET to return Generic classes to COM clients.

    /// ''' <para>The properties and methods are: 

    /// ''' <see cref="Profile.RegisteredDevices">Profile.RegisteredDevices</see>, 

    /// ''' <see cref="Profile.SubKeys">Profile.SubKeys</see> and 

    /// ''' <see cref="Profile.Values">Profile.Values</see>.</para></remarks>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("69CFE7E6-E64F-4045-8D0D-C61F50F31CAC")]
    [ComVisible(true)]
    public class KeyValuePair : IKeyValuePair
    {
        private string m_Key;
        private string m_Value;

        /// <summary>
        ///     ''' COM visible default constructor
        ///     ''' </summary>
        ///     ''' <remarks></remarks>
        public KeyValuePair()
        {
            m_Key = "";
            m_Value = "";
        }
        /// <summary>
        ///     ''' Constructor that can set the key and value simultaneously.
        ///     ''' </summary>
        ///     ''' <param name="Key">The Key element of a key value pair</param>
        ///     ''' <param name="Value">The Value element of a key value pair</param>
        ///     ''' <remarks></remarks>
        public KeyValuePair(string Key, string Value)
        {
            m_Key = Key;
            m_Value = Value;
        }

        /// <summary>
        ///     ''' The Key element of a key value pair
        ///     ''' </summary>
        ///     ''' <value>Key</value>
        ///     ''' <returns>Key as a string</returns>
        ///     ''' <remarks></remarks>
        public string Key
        {
            get
            {
                return m_Key;
            }
            set
            {
                m_Key = value;
            }
        }

        /// <summary>
        ///     ''' The Value element of a key value pair.
        ///     ''' </summary>
        ///     ''' <value>Value</value>
        ///     ''' <returns>Value as a string</returns>
        ///     ''' <remarks></remarks>
        public string Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                m_Value = value;
            }
        }
    }
}