using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZhanTools.ConsoleArgumentReader
{
    /// <summary>
    /// 
    /// </summary>
    public class CmdLineArgumentReader
    {
        private List<KeyValuePair<string, string>> _argumentsList;
        private List<string> _valueArguments; //this will receive values that does not follow a known argument value.
        private List<ArgumentInformation> _argInfo = new List<ArgumentInformation>();
        private PropertyInfo[] _argProperties;
        public bool SupportPartialName { get; set; }

        public string[] ValueArguments
        {
            get { return _valueArguments.ToArray(); }
        }

        // The arguents that are resolved in sequence.
        public KeyValuePair<string, string>[] ArgumentsList
        {
            get { return _argumentsList.ToArray(); }
        }


        // Ctor.
        public CmdLineArgumentReader(Type argType, bool supportPartialName = false)
        {
            SupportPartialName = supportPartialName;

            _argProperties = argType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttributes(typeof(CmdLineArgumentAttribute), false) != null).ToArray();

            foreach (var p in _argProperties)
            {
                var attrs = p.GetCustomAttributes(typeof(CmdLineArgumentAttribute), false);
                if (attrs.Length == 0)
                    continue;

                var attr = (CmdLineArgumentAttribute)attrs[0];

                var m = new ArgumentInformation
                {
                    Name = attr.Name ?? "-" + p.Name,
                    Description = attr.Description,
                    IsRequired = attr.IsRequired,
                    IsFlag = p.PropertyType == typeof(bool),
                    PropertyName = p.Name,
                    Default = attr.Default
                };
                //m.ValuesCount = m.IsFlag ? 0 : attr.ValueCount;

                _argInfo.Add(m);
            }
        }

        private void SetPropertyValue(object o, string propertyName, object value)
        {
            var property = _argProperties.First(x => x.Name == propertyName);

            //only string and flag argument supported now
            if (property.PropertyType == typeof(string) || property.PropertyType == typeof(bool))
                property.SetValue(o, value, null);
            if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
            {
                property.SetValue(o, int.Parse(value.ToString()), null);

            }
            if (property.PropertyType.IsEnum)
            {
                var names = Enum.GetNames(property.PropertyType);
                var name = names.FirstOrDefault(x => x.Equals(value == null ? "" : value.ToString(), StringComparison.InvariantCultureIgnoreCase));
                if (name == null)
                    throw new Exception(string.Format("Enum value not recogonized: {0}, should be one of these values: {1}", propertyName, string.Join(",", names)));
                property.SetValue(o, Enum.Parse(property.PropertyType, name), null);

            }
        }

        /// Contains the last error from ReadArguments.
        /// If the argument is read with no errors, Error will be null
        public string Error { get; private set; }


        public TArg ReadArguments<TArg>(string[] tokens)
        {
            return (TArg)ReadArguments(typeof(TArg), tokens);
        }

        public object ReadArguments(Type argType, string[] tokens)
        {
            Error = null;

            object t = Activator.CreateInstance(argType);
            _valueArguments = new List<string>();
            _argumentsList = new List<KeyValuePair<string, string>>();
            int index = 0;
            while (index < tokens.Length)
            {
                var token = tokens[index++];

                try
                {

                    ArgumentInformation[] matches = null;
                    if (!token.StartsWith("-"))
                    {
                        _valueArguments.Add(token);
                        continue;
                    }

                    if (SupportPartialName)
                        matches = _argInfo.Where(x => x.Name.ToLower().Contains(token.Replace("-", "").ToLower())).ToArray();
                    else
                    {

                        matches = _argInfo.Where(x => x.Name.Equals(token, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                    }

                    if (matches.Length == 0)
                    {
                        Error = "Unsupported argument:" + token;
                        return null;

                    }


                    ArgumentInformation match = matches[0];
                    if (matches.Length > 1 && SupportPartialName)
                    {
                        //if there is a excat match, then use it.
                        //else it will be duplicate match
                        match = matches.FirstOrDefault(x => x.Name.ToLower() == token.ToLower());
                        if (match == null)
                        {
                            Error = "More than one argument mathch " + token + "\r\n";
                            foreach (var m in matches)

                                Error += m.Name + "\r\n";
                            return null;
                        }
                    }




                    match.Visited = true;

                    if (match.IsFlag)// a flag argument
                    {
                        SetPropertyValue(t, match.PropertyName, true);
                        _argumentsList.Add(new KeyValuePair<string, string>(token, null));
                    }
                    else// a value arguemnt
                    {
                        if (index >= tokens.Length)
                        {
                            Error = "Arguemnt " + token + " does not have a value";
                            return null;
                        }
                        var value = tokens[index++];
                        _argumentsList.Add(new KeyValuePair<string, string>(token, value));
                        SetPropertyValue(t, match.PropertyName, value);
                    }
                }
                catch (Exception err)
                {
                    Error = "Error when reading arguement " + token + " : " + err.Message;
                    return null;
                }

            }

            foreach (var arg in _argInfo)
            {
                if (arg.IsRequired && !arg.Visited)
                {
                    Error += "Argument required:" + arg.Name;
                    return null;
                }

                if (!arg.Visited && arg.Default != null)
                {
                    try
                    {
                        SetPropertyValue(t, arg.PropertyName, arg.Default);
                    }
                    catch (Exception err)
                    {
                        Error = err.Message;
                    }
                }

            }

            return t;
        }

    }
}