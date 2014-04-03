using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonicComputing;

namespace Tests
{
    [TestClass]
    public class WhenSettingOptions
    {
        //public void func<T>()
        //{
        //    Type type = typeof(T);
        //    Debug.WriteLine(type.Name);
        //    Debug.WriteLine(type.IsValueType); 
        //    var ctors = type.GetConstructors();
        //    //var instance1 = Activator.CreateInstance<T>();
        //    if(type.Name == "String")
        //    {
        //        var instance = Activator.CreateInstance(type, "");
        //    }
        //    else
        //    {
        //        var instance = Activator.CreateInstance(type);
        //    }
        //}

        //[TestMethod]
        //public void MyTestMethod()
        //{
        //    func<int>();
        //    func<string>();
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyOptionName_Throws_Exception()
        {
            var sut = new CmdLineParser();
            sut.Setup<int>("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MalformedSpec_NoShortOption_Throws_Exception()
        {
            var sut = new CmdLineParser();
            sut.Setup<int>("option,");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void MalformedSpec_NoLongOption_Throws_Exception()
        {
            var sut = new CmdLineParser();
            sut.Setup<int>(",o");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Setting_The_Same_LongOption_Twice_Throws_Exception()
        {
            var sut = new CmdLineParser();
            sut.Setup<int>("option");
            sut.Setup<int>("option");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Setting_The_Same_ShortOption_Twice_Throws_Exception()
        {
            var sut = new CmdLineParser();
            sut.Setup<int>("option1,o");
            sut.Setup<int>("option2,o");
        }
        
        //[TestMethod]
        //public void TestMethod1()
        //{
        //    var options = new CmdLineParser();

        //    options.Setup<string>("option", true, "help");

        //    Assert.AreEqual(options.Opts.option, null);
        //}
    }
}
