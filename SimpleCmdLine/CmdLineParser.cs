﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace SonicComputing
{
    public class ParseException : Exception
    {
        public string HelpMessage { get; set; }

        public ParseException(string msg) : base(msg)
        {
        }

        public ParseException(string msg, string helpMsg) : base(msg)
        {
            HelpMessage = helpMsg;
        }
    }

    public class CmdLineParser
    {
        public dynamic Opts = new DynamicDictionary();
        private Dictionary<string, bool> _required = new Dictionary<string, bool>();
        private Dictionary<string, string> _help = new Dictionary<string, string>();
        //private Dictionary<string, string> _longOptValues = new Dictionary<string, string>();
        //private Dictionary<string, string> _shortOptValues = new Dictionary<string, string>();
        private List<string> _longOpts = new List<string>();
        private List<string> _shortOpts = new List<string>();
        private Dictionary<string, Type> _optTypes = new Dictionary<string, Type>();
        private List<string> _longOptsSet = new List<string>();
        private List<string> _shortOptsSet = new List<string>();

        public void Setup<T>(string spec, bool required = true, string helpMsg = "")
        {
            if (string.IsNullOrEmpty(spec))
                throw new ArgumentException();

            string longOptName="", shortOptName="";
            var opts = spec.Split(',');
            if (opts.Length == 1)
            {
                longOptName = opts[0];
            }
            else if (opts.Length == 2)
            {
                longOptName = opts[0];
                shortOptName = opts[1];
                if (string.IsNullOrEmpty(longOptName) ||
                   string.IsNullOrEmpty(shortOptName))
                {
                    throw new ArgumentException();
                }
                if(_shortOpts.Contains(shortOptName))
                {
                    throw new ArgumentException();
                }
                _shortOpts.Add(shortOptName);
            }
            if( _longOpts.Contains(longOptName))
            {
                throw new ArgumentException();
            }
            _longOpts.Add(longOptName);
            _optTypes[longOptName] = typeof(T);
            _required[longOptName] = required;
            _help[longOptName] = helpMsg;

        }

        public void Parse(string[] args)
        {
            if (args.Length == 0 && _required.Any(kv => { return kv.Value; }))
                throw new ParseException("Required option(s) not present:", GenerateHelpMsg());

            if(args.Contains("--help") || args.Contains("-h"))
                throw new ParseException("Usage:", GenerateHelpMsg());

            //convert short opts to long opts?

            for (int i = 0; i < args.Length; i++)
            {
                if(IsLongOpt(args[i]))
                {
                    var longOptName = GetLongOptName(args[i]);
                    if (!IsValidOpt(longOptName))
                        throw new ParseException("Invalid Option:", GenerateHelpMsg());
                    if (_longOptsSet.Contains(longOptName))
                        throw new ParseException("Option specfied twice:" + longOptName);

                    _longOptsSet.Add(longOptName);

                    if (_optTypes[longOptName].Name == "Boolean")
                    {
                        Opts.TrySetMember(new MyMemberBinder(longOptName), true);
                    }
                    else
                    {
                        if (i+1 >= args.Length)
                            throw new ParseException("Missing value for option:" + longOptName);
                        Opts.TrySetMember(new MyMemberBinder(longOptName),
                            GetValue(longOptName, args[i + 1]));
                    }
                }
            }

             _optTypes.Where(x => x.Value.Name == "Boolean")
                    .Select(x => x.Key)
                    .Except(_longOptsSet)
                    .ToList()
                    .ForEach(x => Opts.TrySetMember(new MyMemberBinder(x), false));

            var requiredOpts = _required.Where(x => x.Value).Select(x => x.Key);
            foreach(var required in requiredOpts)
            {
                if (!_longOptsSet.Contains(required))
                    throw new ParseException("Requird option missing: " + required, GenerateHelpMsg());
            }
        }

        private object GetValue(string optName, string value)
        {
            var type = _optTypes[optName];
            if (type.Name == "String")
            {
                return value;
            }
            var method = type.GetMethod("Parse", new []{typeof(string)});
            try
            {
                return method.Invoke(type, new[] { value });
            }
            catch(Exception)
            {
                throw new ParseException("Illegal value for option:" + optName);
            }
        }

        private bool IsLongOpt(string option)
        {
            return option.StartsWith("--");
        }

        private bool IsValidOpt(string option)
        {
            return _longOpts.Contains(option);
        }

        private string GetLongOptName(string option)
        {
            return option.Substring(2);
        }

        private string GetShortOptName(string option)
        {
            return option.Substring(1);
        }

        private string GenerateHelpMsg()
        {
            return string.Empty;
        }
    }

    class MyMemberBinder : SetMemberBinder
    {
        public MyMemberBinder(string name)
            : base(name, false)
        {
        }
        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }

    class DynamicDictionary : DynamicObject
    {
        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        public int Count
        {
            get { return dictionary.Count; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name.ToLower();
            return dictionary.TryGetValue(name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dictionary[binder.Name.ToLower()] = value;
            return true;
        }
    }
}
