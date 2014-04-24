using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace SonicComputing
{
    public class CmdLineParserException : Exception
    {
        public string HelpMessage { get; set; }

        public CmdLineParserException(string msg) : base(msg)
        {
        }

        public CmdLineParserException(string msg, string helpMsg) : base(msg)
        {
            HelpMessage = helpMsg;
        }
    }

    public class CmdLineParser
    {
        public dynamic Opts = new DynamicDictionary();
        private Dictionary<string, bool> _required = new Dictionary<string, bool>();
        private Dictionary<string, string> _help = new Dictionary<string, string>();
        private List<string> _longOpts = new List<string>();
        private Dictionary<string, string> _shortToLongOpts = new Dictionary<string, string>();
        private Dictionary<string, string> _longToShortOpts = new Dictionary<string, string>();
        private Dictionary<string, Type> _optTypes = new Dictionary<string, Type>();
        private List<string> _longOptsSet = new List<string>();
        /*
        class Option
        {
            public string longSpec;
            public string shortSpec;
            public Type value;
            public string helpMsg;
            public bool required;
        }
        List<Option> _options;
        */

        public void Setup<T>(string spec, bool required = true, string helpMsg = "")
        {
            if (string.IsNullOrEmpty(spec))
                throw new CmdLineParserException("Missing option");

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
                    throw new CmdLineParserException("Missing long or short option");
                }
                if(_shortToLongOpts.ContainsKey(shortOptName))
                {
                    throw new CmdLineParserException(string.Format("Short option {0} already specified", shortOptName));
                }
                _shortToLongOpts[shortOptName] = longOptName;
                _longToShortOpts[longOptName] = shortOptName;
            }
            if( _longOpts.Contains(longOptName))
            {
                throw new CmdLineParserException(string.Format("Long option {0} already specified", longOptName));
            }
            _longOpts.Add(longOptName);
            _optTypes[longOptName] = typeof(T);
            _required[longOptName] = required;
            _help[longOptName] = helpMsg;

            if(!required)
            {
                object result;
                if(!Opts.TryGetMember(new MyGetMemberBinder(longOptName), out result))
                {
                    if (typeof(T).Name == "String")
                    {
                        Opts.TrySetMember(new MySetMemberBinder(longOptName), "");
                    }
                    else if (typeof(T).Name.Contains("[]"))
                    {
                        var t = typeof(T).GetElementType();
                        var y = Array.CreateInstance(t, 0);
                        Opts.TrySetMember(new MySetMemberBinder(longOptName), y);
                    }
                    else
                    {
                        Opts.TrySetMember(new MySetMemberBinder(longOptName), default(T));
                    }
                }
            }
        }

        public void Parse(string[] args)
        {
            CheckForHelpOpt(args);

            ConvertShortOptsToLongOpts(args);

            SetOptions(args);

            SetMissingBoolOptsToFalse();

            CheckForMissingRequiredOpts();
        }

        private void CheckForMissingRequiredOpts()
        {
            var requiredOpts = _required.Where(x => x.Value).Select(x => x.Key);
            foreach (var required in requiredOpts)
            {
                if (!_longOptsSet.Contains(required))
                    throw new CmdLineParserException("Requird option missing: " + required, GenerateHelpMsg());
            }
        }

        private void SetMissingBoolOptsToFalse()
        {
            _optTypes.Where(x => x.Value.Name == "Boolean")
                   .Select(x => x.Key)
                   .Except(_longOptsSet)
                   .ToList()
                   .ForEach(x => Opts.TrySetMember(new MySetMemberBinder(x), false));
        }

        private void SetOptions(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (IsLongOpt(args[i]))
                {
                    var longOptName = GetLongOptName(args[i]);
                    if (!IsValidOpt(longOptName))
                        throw new CmdLineParserException("Invalid Option:", GenerateHelpMsg());
                    if (_longOptsSet.Contains(longOptName))
                        throw new CmdLineParserException("Option specfied twice:" + longOptName);

                    _longOptsSet.Add(longOptName);

                    if (_optTypes[longOptName].Name == "Boolean")
                    {
                        Opts.TrySetMember(new MySetMemberBinder(longOptName), true);
                    }
                    else if(_optTypes[longOptName].Name.Contains("[]"))
                    {
                        if (i + 1 >= args.Length)
                            throw new CmdLineParserException("Missing value for option:" + longOptName);
                        var values = args[i + 1].Split(',');
                        var type = _optTypes[longOptName].GetElementType();
                        var array = Array.CreateInstance(type, values.Length);
                        for (int x = 0; x < values.Length; x++ )
                        {
                            array.SetValue(ParseValue(type,values[x]), x);
                        }
                        Opts.TrySetMember(new MySetMemberBinder(longOptName),  array);
                        i = i + 1;
                    }
                    else
                    {
                        if (i + 1 >= args.Length)
                            throw new CmdLineParserException("Missing value for option:" + longOptName);
                        Opts.TrySetMember(new MySetMemberBinder(longOptName),
                            SetValue(longOptName, args[i + 1]));
                        i = i + 1;
                    }
                }
            }
        }

        private void ConvertShortOptsToLongOpts(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (IsShortOpt(args[i]))
                {
                    var shortOptName = GetShortOptName(args[i]);
                    if (_shortToLongOpts.ContainsKey(shortOptName))
                        args[i] = "--" + _shortToLongOpts[shortOptName];
                    else
                        throw new CmdLineParserException("Invalid option:" + shortOptName);
                    if (_optTypes[_shortToLongOpts[shortOptName]].Name != "Boolean")
                        i = i + 1;
                }
                else if (IsLongOpt(args[i]))
                {
                    if (!IsValidOpt(GetLongOptName(args[i])))
                        throw new CmdLineParserException("Invalid Option:", GenerateHelpMsg());
                    if (_optTypes[GetLongOptName(args[i])].Name != "Boolean")
                        i = i + 1;
                }
                if (i > args.Length)
                    break;
            }
        }

        private void CheckForHelpOpt(string[] args)
        {
            if (args.Contains("--help") || args.Contains("-h"))
                throw new CmdLineParserException("", GenerateHelpMsg());
        }

        private object ParseValue(Type type, string value)
        {
            var method = type.GetMethod("Parse", new[] { typeof(string) });
            try
            {
                return method.Invoke(type, new[] { value });
            }
            catch (Exception)
            {
                throw new CmdLineParserException("Illegal value for array option");
            }
        }

        private object SetValue(string optName, string value)
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
                throw new CmdLineParserException("Illegal value for option:" + optName);
            }
        }

        private bool IsLongOpt(string option)
        {
            return option.StartsWith("--");
        }

        private bool IsShortOpt(string option)
        {
            if (option[0] == '-' && option[1] != '-')
                return true;
            return false;
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
            var nl = Environment.NewLine;
            var result =  "Usage:" + nl;
            var longest = _longOpts.Select(x => x.Length).Max();
            foreach (var h in _help)
            {
                var numSpaces = longest - h.Key.Length;
                if (numSpaces != 0) numSpaces += 1;
                result += "--" + h.Key + GetShortOpt(h.Key) + ":" + GetSpaces(numSpaces) + h.Value + nl;
            }
            return result;
        }

        private string GetShortOpt(string longOpt)
        {
            if (_longToShortOpts.ContainsKey(longOpt))
                return ",-" + _longToShortOpts[longOpt];
            return "   ";
        }

        private string GetSpaces(int n)
        {
            if (n == 0) return " ";
            return Enumerable.Range(0, n).Select(x => " ").Aggregate((a, b) => a + " ");
            //var str = "";
            //for (var i = 0; i < n; i++)
            //{
            //    str += " ";
            //}
            //return str;
        }
    }

    class MySetMemberBinder : SetMemberBinder
    {
        public MySetMemberBinder(string name)
            : base(name, false)
        {
        }
        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }

    class MyGetMemberBinder : GetMemberBinder
    {
        public MyGetMemberBinder(string name) : base(name, false)
        {

        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
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
