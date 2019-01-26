﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PiggyRuntime
{
    public class Tool
    {
        // This class is used by the tool and the templates for communication between the two.
        // It indicates the options used for the tool invocation.

        public static string OutputLocation
        {
            get;
            set;
        }

        public static string[] CommandLineArgs
        {
            get;
            set;
        }

        public static Redirect Redirect
        {
            get;
            set;
        }

        public static List<string> GeneratedFiles
        {
            get;
            set;
        } = new List<string>();
    }
}
