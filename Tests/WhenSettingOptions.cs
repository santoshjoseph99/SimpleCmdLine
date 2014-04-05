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
    }
}
