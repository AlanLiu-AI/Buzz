using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;
#if !COMPACTUTIL
using System.Web.Hosting;
#endif

namespace Runner.Base.Util
{
    public static class ClassUtil
    {
#if !NOLOG4NET
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ClassUtil).Name);
        private static readonly Logger Logger = Logger.GetLogger(typeof(ClassUtil).Name);
#else 
        private static readonly Logger log = Logger.GetLogger(typeof(ClassUtil).Name);
        private static readonly Logger logger = log;
#endif

        static ClassUtil()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ins">class instance </param>
        /// <param name="clazz">full name of a class, ex: System.IComparable</param>
        /// <returns></returns>
        public static bool ClassInstanceof(object ins, String clazz)
        {
            var myType = ins.GetType();
            var myFilter = new TypeFilter(MyInterfaceFilter);
            var myInterfaces = myType.FindInterfaces(myFilter, clazz);
            if (myInterfaces.Length > 0)
            {
                if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
                {
                    Log.DebugFormat("classInstanceof::{0} implements the interface {1}.", myType, clazz);
                }
                return true;
            }
            Log.ErrorFormat(
                "classInstanceof::{0} does not implement the interface {1}.",
                myType, clazz);
            return false;
        }

        public static bool MyInterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
        }

        public static object InvokeByInstance(object instance, string method, params object[] args)
        {
            return InvokeCallAssembly(instance, instance.GetType(), method, args);
        }

        public static object InvokeByTypename(string clazz, string method, params object[] args)
        {
            return InvokeAssembly("", clazz, method, args);
        }

        public static object InvokeAssembly(string assembly, string clazz, string method, params object[] args)
        {
            var type = string.IsNullOrEmpty(assembly) ? LoadAssamblyType(clazz) : LoadAssamblyType(assembly, clazz);
            return InvokeCallAssembly(null, type, method, args);
        }

        public static object GetPropertySimply(object instance, string propertyName)
        {
            if (instance == null) return null;
            return GetPropertySimply(instance, instance.GetType(), propertyName);
        }
        public static object GetPropertySimply(object instance, Type type, string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName);
            if(propertyInfo!=null)
            {
                return propertyInfo.GetValue(instance, null);
            }
            return null; 
        }

        public static bool SetPropertySimply(object instance, string propertyName, object newValue)
        {
            return instance != null && SetPropertySimply(instance, instance.GetType(), propertyName, newValue);
        }

        public static bool SetPropertySimply(object instance, Type type, string propertyName, object newValue)
        {
            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(instance, newValue, null);
                return true;
            }
            return false;
        }

        public static object GetFieldSimply(object instance, string fieldName)
        {
            if (instance == null) return null;
            return GetFieldSimply(instance, instance.GetType(), fieldName);
        }
        public static object GetFieldSimply(object instance, Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName);
            return fieldInfo != null ? fieldInfo.GetValue(instance) : null;
        }

        public static bool SetFieldSimply(object instance, string fieldName, object newValue)
        {
            return instance != null && SetFieldSimply(instance, instance.GetType(), fieldName, newValue);
        }

        public static bool SetFieldSimply(object instance, Type type, string fieldName, object newValue)
        {
            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo == null) return false;
            fieldInfo.SetValue(instance, newValue);
            return true;
        }

        public static object GetItemSimply(object instance, string fieldName)
        {
            return instance == null ? null : GetItemSimply(instance, instance.GetType(), fieldName);
        }

        public static object GetItemSimply(object instance, Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName);
            return fieldInfo != null ? fieldInfo.GetValue(instance) : GetPropertySimply(instance, type, fieldName);
        }

        public static bool SetItemSimply(object instance, string fieldName, object newValue)
        {
            return instance != null && SetItemSimply(instance, instance.GetType(), fieldName, newValue);
        }

        public static bool SetItemSimply(object instance, Type type, string fieldName, object newValue)
        {
            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo == null) return SetPropertySimply(instance, type, fieldName, newValue);
            fieldInfo.SetValue(instance, newValue);
            return true;
        }

        public static object InvokeCallSimple(object instance, Type type, string method, params object[] args)
        {
            var methodInfo = type.GetMethod(method);
            if (methodInfo != null)
            {
                try
                {
                    var returnResult = methodInfo.Invoke(instance, args);
                    return returnResult;
                }
                catch (Exception e)
                {
                    string errorMsg = String.Format("Could not invoke method \"{0}\" from type \"{1}\"", method, type.Name);
                    Log.Error(errorMsg, e);
                    throw new ClassUtilInvokeException(errorMsg, e);
                }
            }
            Log.WarnFormat("Can't find reflect method '{0}' from type '{1}'!", method,
                type.FullName);
            return null;
        }
        public static object InvokeCallAssembly(Type type, string method, params object[] args)
        {
            return InvokeCallAssembly(null, type, method, args);
        }
        public static object InvokeCallAssembly(object instance, Type type, string method, params object[] args)
        {
            // first try to direct call the method
            MethodInfo methodInfo;
            try
            {
                methodInfo = type.GetMethod(method);
            }
            catch (Exception ex)
            {
                Log.WarnFormat("Get default method '{0}.{1}(argc={2})' error: {3}", type.Name, method, 
                    args==null ? 0 : args.Length, ex.Message);
                methodInfo = null;
            }
            
            if (methodInfo != null)
            {
                try
                {
                    object returnResult = methodInfo.Invoke(instance, args);
                    return returnResult;
                }
                catch (Exception e)
                {
                    var errorMsg = String.Format("Could not direct invoke method \"{0}\" from type \"{1}\"", method, type.Name);
                    Log.Warn(errorMsg, e);
                }
            }
            if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
            {
                Log.DebugFormat("Try to iterator method to invoke method \"{0}\" from type \"{1}\"", method, type.Name);
            }
            // Get the methods from the type
            var methods = type.GetMethods();

            // Try each method for a match
            var failureExcuses = new StringBuilder();
            foreach (var m in methods)
            {
                Object obj;
                if (String.CompareOrdinal(m.Name, method) != 0)
                {
                    continue;
                }
                try
                {
                    //TODO ClassUtil.invokeCallAssembly maybe need optimizer
                    obj = AttemptMethod(instance, type, m, method, args);
                }
                catch (ClassUtilInvokeException e)
                {
                    failureExcuses.Append(e.Message + "\n");
                    continue;
                }

                // If we make it this far without a throw, our job is done!
                if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
                {
                    Log.Debug("invoke success!");
                }
                return obj;
            }
            Log.Error(failureExcuses.ToString());
            throw new ClassUtilInvokeException(String.Format("Could not invoke method \"{0}\" from type \"{1}\" at \"{2}\"", method, type.Name, Assembly.GetAssembly(type).GetName() + ":" + Assembly.GetAssembly(type).CodeBase));
        }

        public static string LoadedAssamblies()
        {
            var assembles = Thread.GetDomain().GetAssemblies();
            var sb = new StringBuilder();
            sb.Append("loaded Assemblies: ");
            foreach(Assembly assembly in assembles)
            {
                sb.AppendLine(assembly.CodeBase);
            }
            return sb.ToString();
        }

        private static readonly Object MyAssembliesCacheLock = new Object();
        private static readonly IDictionary<string, Assembly> MyAssembliesCache = new Dictionary<string, Assembly>();
        public static Assembly LoadAssambly(string assambly)
        {
            lock (MyAssembliesCacheLock)
            {
                if(MyAssembliesCache.ContainsKey(assambly))
                {
                    return MyAssembliesCache[assambly];
                }
                var assembly = LoadAssambly(AppDomain.CurrentDomain, assambly);
                if(assembly != null)
                {
                    MyAssembliesCache[assambly] = assembly;
                    return assembly;
                }
            }
            return null;
        }

        private static Assembly LoadAssambly(AppDomain domain, string assambly)
        {
            Assembly assemble;
            try
            {
                if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
                {
                    Log.DebugFormat("loadAssambly('{0}') called...", assambly);
                }                
                string assambly1 = assambly;
                if(assambly1.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                    assambly1.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    assambly1 = assambly1.Substring(0, assambly1.Length - 4);
                }
                var assembles = domain.GetAssemblies();
                foreach (var assembleLoaded in assembles)
                {
                    var an = assembleLoaded.GetName();
                    if (String.Compare(an.Name, assambly1, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return assembleLoaded;
                    }
                }
                // Load the requested assembly from file
                if (string.IsNullOrEmpty(domain.SetupInformation.CachePath))
                {
                    /*if (logger.IsDebugEnabled)
                    {
                        logger.DebugFormat(@"App ApplicationBase '{0}', CachePath is {1}, PrivateBinPath is {2} ShadowCopyDirectories: {3} ",
                            domain.SetupInformation.ApplicationBase,
                            domain.SetupInformation.CachePath,
                            domain.SetupInformation.PrivateBinPath,
                            domain.SetupInformation.ShadowCopyDirectories);
                    }*/
                }
                if(File.Exists(assambly))
                {
                    //assambly = IOUtil.GetFullPath(assambly);
                    //assemble = Assembly.LoadFile(assambly);
                    assemble = Assembly.LoadFrom(assambly);
                    //byte[] buf = IOUtil.binReadFile(assambly);
                    //assemble = domain.Load(buf);
                }
                else
                {
                    if (!Path.IsPathRooted(assambly))
                    {
                        if (AssemblyPathes.Select(t => t + Path.DirectorySeparatorChar + assambly).Any(File.Exists))
                        {
                            var buf = IOUtil.BinReadFile(assambly);
                            assemble = domain.Load(buf);
                            return assemble;
                        }
                    }
                    assemble = domain.Load(assambly);
                }
            }
            catch (FileNotFoundException e)
            {
                if (Logger.IsTraceEnabled) Log.ErrorFormat("Could not load Assembly '{0}', error: {1}", assambly, e.Message);
                throw;
            }
            return assemble;
        }

        public static Type LoadAssamblyType(string clazz)
        {
            return LoadAssamblyType(clazz, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clazz"></param>
        /// <param name="quiet">IF quiet is true and if the clazz is not found it will reutn null.</param>
        /// <returns></returns>
        public static Type LoadAssamblyType(string clazz, bool quiet)
        {
            if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
            {
                Log.DebugFormat("loadAssamblyType('{0}') called...", clazz);
            }
            var assembles = Thread.GetDomain().GetAssemblies();
            foreach (var assamble in assembles)
            {
                var type = LoadAssamblyType(assamble, clazz);
                if (type != null) return type;
            }
            if (quiet) return null;
            throw new ClassUtilInvokeException(String.Format("Could not load type \"{0}\" ", clazz));
        }
        
        
        private static string[] _assemblyPathes;
        public static string[] AssemblyPathes {
            get 
            {
                if(_assemblyPathes!=null)return _assemblyPathes;
                var pathes = new List<string>();
                var path = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar;
                pathes.Add(path);
                path = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
                if(Directory.Exists(path) && !pathes.Contains(path)) pathes.Add(path);
                path = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +  "bin" + Path.DirectorySeparatorChar;
                if(Directory.Exists(path) && !pathes.Contains(path)) pathes.Add(path);
                path = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar +  "bin" + Path.DirectorySeparatorChar;
                if(Directory.Exists(path) && !pathes.Contains(path)) pathes.Add(path);
#if !COMPACTUTIL
                if (!string.IsNullOrEmpty(HostingEnvironment.ApplicationPhysicalPath))
                {
                    path = HostingEnvironment.ApplicationPhysicalPath + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
                    if(Directory.Exists(path) && !pathes.Contains(path)) pathes.Add(path);
                    path = HostingEnvironment.ApplicationPhysicalPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar;
                    if(Directory.Exists(path) && !pathes.Contains(path)) pathes.Add(path);
                }
#endif
                _assemblyPathes = pathes.ToArray();
                return _assemblyPathes;
            }
        }

        public static Type LoadAssamblyType(string assembly, string clazz)
        {
            Type type;
            try
            {
                var assemble = LoadAssambly(assembly);
                if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Load assembly '{0}' succeeded.", assembly);
#if !NOLOG4NET
                    Logger.Log(string.Format("Load assembly '{0}' succeeded.'", assembly));
#endif
                }
                type = LoadAssamblyType(assemble, clazz);
                if (type != null)
                {
                    return type;
                }
            }
            catch (Exception ex)
            {
                if (Logger.IsTraceEnabled) Log.WarnFormat("Try to load clazz '{0}' in assembly '{1}' failed: {2}", clazz, assembly, ex.StackTrace);
            }
            if(!Path.IsPathRooted(assembly))
            {
                if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Try to load clazz '{0}' in assembly '{1}' from several paths.", clazz, assembly);
                }
                for (int i = 0; i < AssemblyPathes.Length; i++)
                {
                    if (!Directory.Exists(AssemblyPathes[i])) Directory.CreateDirectory(AssemblyPathes[i]);
                    var assemblyFile = AssemblyPathes[i] + Path.DirectorySeparatorChar + assembly;
                    if (!File.Exists(assemblyFile)) continue;
                    type = LoadAssamblyType(assemblyFile, clazz);
                    if (type == null) continue;
                    if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Load assembly '{0}' in assembly file '{1}'", clazz, assemblyFile);
                    }
                    return type;
                }
            }
            var error = string.Format("Can't load class '{0}' from assembly file '{1}", clazz, assembly);
            Log.Error(error);
            throw new Exception(error);
        }

        public static Type ForName(string className, Assembly[] assemblys)
        {
            Type type = Type.GetType(className);
            if (type != null) return type;
            foreach (Assembly assembly in assemblys)
            {
                type = assembly.GetType(className);
                if (type != null) return type;
            }
            return null;
        }

        public static object NewInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static object NewInstance(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        public static object NewInstanceUrge(string className, string assemblyName, params object[] args)
        {
            //Assembly[] asms = new Assembly[1] { Assembly.LoadFrom(assemblyName) };
            var asms = new [] { LoadAssambly(assemblyName) };
            return NewInstance(ForName(className, asms), args);
        }

        public static object NewInstance(string className, string assemblyName, params object[] args)
        {
            Type type = LoadAssamblyType(assemblyName, className);
            if (type!=null)
            {
                return NewInstance(type, args);
            }
            throw new Exception(string.Format("Class '{0}' not found in '{1}'.", className, assemblyName));
        }

        private static Type LoadAssamblyType(Assembly assemble, string clazz)
        {
            Type type;
            try
            {
                // get the requested type
                type = assemble.GetType(clazz, true);
                if (Logger.IsTraceEnabled && Log.IsDebugEnabled)
                {
                    Log.DebugFormat("load Type '{0}' from assembly '{1}' '{2}'.", 
                        clazz, assemble.GetName(), assemble.CodeBase);
                }
            }
            catch (TypeLoadException e)
            {
                Log.WarnFormat("Could not load Type '{0}' from assembly '{1}' '{2}', error: {3} ", 
                    clazz, assemble.GetName(), assemble.CodeBase, e.Message);
                throw;
            }
            return type;
        }

        /// <summary>
        /// Checks a method for a signature match, and invokes it if there is one 
        /// </summary>
        /// <param name="instance"> </param>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Object AttemptMethod(object instance, Type type, MethodInfo method, String name, object[] args)
        {
            if (method == null) throw new ArgumentNullException("method");
            // Name does not match?
            if (String.Compare(method.Name, name, false, CultureInfo.InvariantCulture) != 0)
            {
                throw new ClassUtilInvokeException(method.DeclaringType + "." + method.Name + ": Method Name Doesn't Match!");
            }

            // Wrong number of parameters?
            var param = method.GetParameters();
            if (param.Length != args.Length)
            {
                throw new ClassUtilInvokeException(method.DeclaringType + "." + method.Name + ": Method Signatures Don't Match!");
            }

            // Ok, can we convert the strings to the right types?
            var newArgs = new Object[args.Length];
            for (int index = 0; index < args.Length; index++)
            {
                try
                {
                    newArgs[index] = Convert.ChangeType(args[index], param[index].ParameterType, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    throw new ClassUtilInvokeException(method.DeclaringType + "." + method.Name + ": Argument Conversion Failed", e);
                }
            }

            // We made it this far, lets see if we need an instance of this type
            if (instance == null && !method.IsStatic)
            {
                instance = Activator.CreateInstance(type);
            }

            // ok, let's invoke this one!
            return method.Invoke(instance, newArgs);
        }

        public static object Clone(object instance)
        {
            return Clone(instance, false);
        }
        //no consider generic class
        public static object Clone(object instance, bool all)
        {
            if (instance == null)
            {
                return null;
            }
            if(instance is ICloneable)
            {
                return ((ICloneable)instance).Clone();
            }
            //First we create an instance of this specific type.
            var newObject = Activator.CreateInstance(instance.GetType());
            //We get the array of fields for the new type instance.
            var fields = newObject.GetType().GetFields();
            var i = 0;
            foreach (var fi in instance.GetType().GetFields())
            {
                //We query if the fields support the ICloneable interface.
                var cloneType = fi.FieldType.
                            GetInterface("ICloneable", true);

                if (cloneType != null)
                {
                    var val = fi.GetValue(instance);
                    //Getting the ICloneable interface from the object.
                    var clone = (ICloneable)val;
                    if(clone!=null)
                    {
                        //We use the clone method to set the new value to the field.
                        fields[i].SetValue(newObject, clone.Clone());
                    }
                }
                else
                {
                    var val = fi.GetValue(instance);
                    if(val!=null)
                    {
                        // If the field doesn't support the ICloneable 
                        // interface then just set it.
                        //TODO ClassUtils.Clone maybe need to process Array 
                        fields[i].SetValue(newObject, val);
                    }
                }
                //Now we check if the object support the 
                //IEnumerable interface, so if it does
                //we need to enumerate all its items and check if 
                //they support the ICloneable interface.
                var enumerableType = fi.FieldType.GetInterface
                                ("IEnumerable", true);
                if (all && enumerableType != null)
                {
                    //Get the IEnumerable interface from the field.
                    var iEnum = (IEnumerable)fi.GetValue(instance);
                    //This version support the IList and the 
                    //IDictionary interfaces to iterate on collections.
                    var listType = fields[i].FieldType.GetInterface
                                        ("IList", true);
                    var dictType = fields[i].FieldType.GetInterface
                                        ("IDictionary", true);

                    int j = 0;
                    if (listType != null)
                    {
                        //Getting the IList interface.
                        var list = (IList)fields[i].GetValue(newObject);
                        foreach (var obj in iEnum)
                        {
                            //Checking to see if the current item 
                            //support the ICloneable interface.
                            cloneType = obj.GetType().
                                GetInterface("ICloneable", true);

                            if (cloneType != null)
                            {
                                //If it does support the ICloneable interface, 
                                //we use it to set the clone of
                                //the object in the list.
                                var clone = (ICloneable)obj;
                                list[j] = clone.Clone();
                            }

                            //NOTE: If the item in the list is not 
                            //support the ICloneable interface then in the 
                            //cloned list this item will be the same 
                            //item as in the original list
                            //(as long as this type is a reference type).
                            j++;
                        }
                    }
                    else if (dictType != null)
                    {
                        //Getting the dictionary interface.
                        var dic = (IDictionary)fields[i].
                                            GetValue(newObject);
                        j = 0;

                        foreach (DictionaryEntry de in iEnum)
                        {
                            //Checking to see if the item 
                            //support the ICloneable interface.
                            cloneType = de.Value.GetType().
                                GetInterface("ICloneable", true);

                            if (cloneType != null)
                            {
                                var clone = (ICloneable)de.Value;

                                dic[de.Key] = clone.Clone();
                            }
                            j++;
                        }
                    }
                }
                i++;
            }
            return newObject;
        }
    }

    public class ClassUtilInvokeException : Exception
    {
        public ClassUtilInvokeException(String m) : base(m) { }

        public ClassUtilInvokeException(String m, Exception n) : base(m, n) { }
    }
}
