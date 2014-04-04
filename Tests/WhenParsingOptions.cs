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

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Only_Help_Option_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option,o");
            sut.Parse(new[] { "--help" });
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Help_Option_Specified_With_Other_Options()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option1,1");
            sut.Setup<int>("option2,2");
            sut.Parse(new[] { "--option1", "--help" });
        }

        [TestMethod]
        public void Default_Value_For_Option_Set_And_Option_Not_Specified()
        {
            var sut = new CmdLineParser();

            sut.Opts.option1 = 3;

            sut.Setup<int>("option1,1", false);
            sut.Setup<int>("option2,2", false);
            sut.Parse(new[] { "--option2", "55" });

            Assert.AreEqual(3, sut.Opts.option1);
            Assert.AreEqual(55, sut.Opts.option2);
        }

        [TestMethod]
        public void Default_Value_For_Option_Set_And_Option_Specified()
        {
            var sut = new CmdLineParser();

            sut.Opts.option1 = "abc";

            sut.Setup<string>("option1,1", false);
            sut.Setup<int>("option2,2");
            sut.Parse(new[] { "--option1", "def", "--option2", "55" });

            Assert.AreEqual("def", sut.Opts.option1);
            Assert.AreEqual(55, sut.Opts.option2);           
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Option_Specified_Twice_Throws_Exception()
        {
            var sut = new CmdLineParser();

            sut.Setup<string>("option1,1");
            sut.Setup<int>("option2,2", false);
            sut.Parse(new[] { "--option1", "def", "--option1", "def" });
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Illegal_Value_For_Option_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<string>("option1,1");
            sut.Setup<int>("option2,2");
            sut.Parse(new[] { "--option1", "def", "--option2", "def" });           
        }

        [TestMethod]
        public void Option_Specified_Boolean()
        {
            var sut = new CmdLineParser();

            sut.Setup<bool>("option", false);

            sut.Parse(new[] { "--option" });

            Assert.AreEqual(true, sut.Opts.option);
        }

        [TestMethod]
        public void Multiple_Boolean_Option_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<bool>("option1", false);
            sut.Setup<bool>("option2", false);
            sut.Setup<bool>("option3", false);

            sut.Parse(new[] { "--option1" });

            Assert.AreEqual(true, sut.Opts.option1);
            Assert.AreEqual(false, sut.Opts.option2);
            Assert.AreEqual(false, sut.Opts.option3);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Single_Option_Specified_But_No_Value()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option", false);

            sut.Parse(new[] { "--option" });
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Multiple_Options_Specified_But_No_Value()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option1");
            sut.Setup<string>("option2");
            sut.Setup<bool>("option3", false);

            sut.Parse(new[] { "--option1", "--option2", "--option3" });
        }

        class Class1
        {
            public int Nothing { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Custom_Type_Cannot_Parse()
        {
            var sut = new CmdLineParser();

            sut.Setup<Class1>("option", false);

            sut.Parse(new[] { "--option", "3" });
        }

        class Class2
        {
            public int MyProperty { get; set; }
            void Parse(string input)
            {
                MyProperty = Int32.Parse(input);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Custom_Type_With_Parse_Method()
        {
            var sut = new CmdLineParser();

            sut.Setup<Class2>("option", false);

            sut.Parse(new[] { "--option", "3" });
        }

        [TestMethod]
        public void Multiple_Options_Valid_CommandLine()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option1");
            sut.Setup<string>("option2");
            sut.Setup<bool>("option3", false);

            sut.Parse(new[] { "--option1", "3", "--option2", "abc", "--option3" });

            Assert.AreEqual(3, sut.Opts.option1);
            Assert.AreEqual("abc", sut.Opts.option2);
            Assert.AreEqual(true, sut.Opts.option3);
        }
    }
}
