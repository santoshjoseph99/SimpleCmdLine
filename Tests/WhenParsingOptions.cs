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
        [ExpectedException(typeof(CmdLineParserException))]
        public void Option_Is_Required_No_Options_Passed_In()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option,o");

            sut.Parse(new string[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(CmdLineParserException))]
        public void Option_Is_Required_But_Not_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option,o");

            sut.Parse(new []{"--some", "1", "--other", "abc"});           
        }

        [TestMethod]
        [ExpectedException(typeof(CmdLineParserException))]
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
        [ExpectedException(typeof(CmdLineParserException))]
        public void Only_Help_Option_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option,o", true, "Help for option1");
            sut.Parse(new[] { "--help" });
        }

        [TestMethod]
        [ExpectedException(typeof(CmdLineParserException))]
        public void Help_Option_Specified_With_Other_Options()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option1,1", true, "Help for option1");
            sut.Setup<int>("option2,2", true, "Help for option2");
            sut.Parse(new[] { "--option1", "--help" });
        }

        [TestMethod]
        [ExpectedException(typeof(CmdLineParserException))]
        public void Help_Option_With_Various_Help_Messages()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option1,1", true, "Help for option1");
            sut.Setup<int>("opt,2", true, "Help");
            sut.Setup<int>("reallylongoption1,3", true, "Short help msg");
            sut.Setup<int>("oooo,4", true, "Really long help message that means nothing");
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
        [ExpectedException(typeof(CmdLineParserException))]
        public void Option_Specified_Twice_Throws_Exception()
        {
            var sut = new CmdLineParser();

            sut.Setup<string>("option1,1");
            sut.Setup<int>("option2,2", false);
            sut.Parse(new[] { "--option1", "def", "--option1", "def" });
        }

        [TestMethod]
        [ExpectedException(typeof(CmdLineParserException))]
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
        [ExpectedException(typeof(CmdLineParserException))]
        public void Single_Option_Specified_But_No_Value()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option", false);

            sut.Parse(new[] { "--option" });
        }

        [TestMethod]
        [ExpectedException(typeof(CmdLineParserException))]
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
        [ExpectedException(typeof(CmdLineParserException))]
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
        [ExpectedException(typeof(CmdLineParserException))]
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

        [TestMethod]
        public void All_Possible_Options()
        {
            var sut = new CmdLineParser();

            sut.Setup<string>("xstring");
            sut.Setup<bool>("xbool", false);
            sut.Setup<byte>("xbyte");
            sut.Setup<char>("xchar");
            sut.Setup<decimal>("xdecmial");
            sut.Setup<double>("xdouble");
            sut.Setup<float>("xfloat");
            sut.Setup<int>("xint");
            sut.Setup<long>("xlong");
            sut.Setup<sbyte>("xsbyte");
            sut.Setup<short>("xshort");
            sut.Setup<uint>("xuint");
            sut.Setup<ulong>("xulong");
            sut.Setup<ushort>("xushort");

            sut.Parse(new[] {   "--xstring", "abc", 
                                "--xbool", 
                                "--xbyte", "64",
                                "--xchar", "a",
                                "--xdecmial", "123.4455",
                                "--xdouble", "234234234.324234",
                                "--xfloat", "343.33",
                                "--xint", "-1111222",
                                "--xlong", "-999999999",
                                "--xsbyte", "-22",
                                "--xshort", "22222",
                                "--xuint", "99999999",
                                "--xulong", "1111122222",
                                "--xushort", "6400",
            });

            Assert.AreEqual("abc", sut.Opts.xstring);
            Assert.AreEqual(true, sut.Opts.xbool);
            Assert.AreEqual(64, sut.Opts.xbyte);
            Assert.AreEqual('a', sut.Opts.xchar);
            Assert.AreEqual(123.4455M, sut.Opts.xdecmial);
            Assert.AreEqual(234234234.324234D, sut.Opts.xdouble);
            Assert.AreEqual(343.33F, sut.Opts.xfloat);
            Assert.AreEqual(-1111222, sut.Opts.xint);
            Assert.AreEqual(-999999999L, sut.Opts.xlong);
            Assert.AreEqual(-22, sut.Opts.xsbyte);
            Assert.AreEqual(22222, sut.Opts.xshort);
            Assert.AreEqual(99999999U, sut.Opts.xuint);
            Assert.AreEqual(1111122222UL, sut.Opts.xulong);
            Assert.AreEqual(6400, sut.Opts.xushort);
        }

        [TestMethod]
        public void All_Possible_Options_ShortOnly()
        {
            var sut = new CmdLineParser();

            sut.Setup<string>("xstring,a");
            sut.Setup<bool>("xbool,b", false);
            sut.Setup<byte>("xbyte,c");
            sut.Setup<char>("xchar,d");
            sut.Setup<decimal>("xdecmial,e");
            sut.Setup<double>("xdouble,f");
            sut.Setup<float>("xfloat,g");
            sut.Setup<int>("xint,x");
            sut.Setup<long>("xlong,i");
            sut.Setup<sbyte>("xsbyte,j");
            sut.Setup<short>("xshort,k");
            sut.Setup<uint>("xuint,l");
            sut.Setup<ulong>("xulong,m");
            sut.Setup<ushort>("xushort,n");

            sut.Parse(new[] {   "-a", "abc", 
                                "-b", 
                                "-c", "64",
                                "-d", "a",
                                "-e", "123.4455",
                                "-f", "234234234.324234",
                                "-g", "343.33",
                                "-x", "-1111222",
                                "-i", "-999999999",
                                "-j", "-22",
                                "-k", "22222",
                                "-l", "99999999",
                                "-m", "1111122222",
                                "-n", "6400",
            });

            Assert.AreEqual("abc", sut.Opts.xstring);
            Assert.AreEqual(true, sut.Opts.xbool);
            Assert.AreEqual(64, sut.Opts.xbyte);
            Assert.AreEqual('a', sut.Opts.xchar);
            Assert.AreEqual(123.4455M, sut.Opts.xdecmial);
            Assert.AreEqual(234234234.324234D, sut.Opts.xdouble);
            Assert.AreEqual(343.33F, sut.Opts.xfloat);
            Assert.AreEqual(-1111222, sut.Opts.xint);
            Assert.AreEqual(-999999999L, sut.Opts.xlong);
            Assert.AreEqual(-22, sut.Opts.xsbyte);
            Assert.AreEqual(22222, sut.Opts.xshort);
            Assert.AreEqual(99999999U, sut.Opts.xuint);
            Assert.AreEqual(1111122222UL, sut.Opts.xulong);
            Assert.AreEqual(6400, sut.Opts.xushort);
        }

        [TestMethod]
        public void All_Short_Options_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option1,a");
            sut.Setup<string>("option2,b");
            sut.Setup<bool>("option3,c", false);

            sut.Parse(new[] { "-a", "3", "-b", "abc", "-c" });

            Assert.AreEqual(3, sut.Opts.option1);
            Assert.AreEqual("abc", sut.Opts.option2);
            Assert.AreEqual(true, sut.Opts.option3);
        }

        [TestMethod]
        public void Accessing_Unrequired_Option_Which_Is_Not_Specified()
        {
            var sut = new CmdLineParser();

            sut.Setup<int>("option1", false);
            sut.Setup<int>("option2");
            sut.Parse(new[] { "--option2", "33" });

            Assert.AreEqual(33, sut.Opts.option2);
            Assert.AreEqual(0, sut.Opts.option1);
        }
    }
}
