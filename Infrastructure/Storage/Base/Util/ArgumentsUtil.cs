using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Runner.Base.Util
{

    /// <summary>
    /// ArgumentsUtil class
    /// example: a.exe -size=100 /height:'400' -param1 "Nice stuff !" --debug
    /// </summary>
    public static class ArgumentsUtil
    {
        private static readonly string[] StringSpaceSeparators = new [] { " ", "\t" };

        public static void RegexParser(string argsLine, IDictionary<string, string> parameters)
        {
            string[] args = argsLine.Split(StringSpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
            RegexParser(args, parameters);
        }

        public static void RegexParser(string[] args, string argsLine, IDictionary<string, string> parameters)
        {
            string[] args1 = null;
            if (!string.IsNullOrEmpty(argsLine))
            {
                args1 = argsLine.Split(StringSpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            var aaa = new List<string>();
            if(args1!=null)
            {
                aaa.AddRange(args1);
            }
            if (args!=null)
            {
                aaa.AddRange(args);
            }
            RegexParser(aaa.ToArray(), parameters);
        }

        /// <summary>
        /// Direct parse args, and assign keypair values to parameters 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parameters"> </param>
        public static void RegexParser(string[] args, IDictionary<string, string> parameters)
        {
            //Parameters = new Dictionary<string, string>();
            var spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string parameter = null;

            if (args == null) return;
            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (var txt in args)
            {
                if(!string.IsNullOrEmpty(parameter))
                {
                    if(txt.Contains(" ")||txt.Contains("="))
                    {
                        parameters[parameter] = txt;
                        parameter = null;
                        continue;
                    }
                }
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                var parts = spliter.Split(txt, 3);
                switch (parts.Length)
                {
                        // Found a value (for the last parameter 
                        // found (space separator))
                    case 1:
                        if (parameter != null)
                        {
                            parts[0] = remover.Replace(parts[0], "$1");
                            if (!parameters.ContainsKey(parameter))
                            {
                                parameters.Add(parameter, parts[0]);
                            }
                            else
                            {
                                parameters[parameter] = parts[0];
                            }
                            parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                        // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                                parameters.Add(parameter, "true");
                            else
                                parameters[parameter] = "true";
                        }
                        parameter = parts[1];
                        break;

                        // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (parameter != null)
                        {
                            if (!parameters.ContainsKey(parameter))
                                parameters.Add(parameter, "true");
                            else
                                parameters[parameter] = "true";
                        }
                        parameter = parts[1];
                        parts[2] = remover.Replace(parts[2], "$1");
                        // Remove possible enclosing characters (",')
                        if (!parameters.ContainsKey(parameter))
                        {
                            parameters.Add(parameter, parts[2]);
                        }else{
                            parameters[parameter] = parts[2];
                        }
                        parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (parameter != null)
            {
                if (!parameters.ContainsKey(parameter))
                    parameters.Add(parameter, "true");
                else
                    parameters[parameter] = "true";
            }
        }
    }
}
