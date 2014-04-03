using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonicComputing;

namespace Tests
{
    [TestClass]
    public class WhenParsingOptions
    {
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Option_Is_Required_No_Options_Passed_In()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option,o");

            sut.Parse(new string[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Option_Is_Required_But_Not_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option,o");

            sut.Parse(new []{"--some", "1", "--other", "abc"});           
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Option_Required_Is_Not_Set()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("required,r");
            sut.Setup<int>("notrequired,n", false);

            sut.Parse(new[] { "--notrequired", "1"});           
        }

        [TestMethod]
        public void No_Options_Required_And_No_Options_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("notrequired1", false);
            sut.Setup<int>("notrequired2", false);

            sut.Parse(new string[] { });
        }

        [TestMethod]
        public void One_Option_Required_And_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option,o");
            sut.Parse(new[] { "--option", "3" });

            Assert.AreEqual(3, sut.Opts.option);
        }
    }
}
