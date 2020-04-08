using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wallop.Cmd.Parsing;
using Wallop.Cmd.Parsing.Tokens;
using System.Linq;


namespace Cmd.Tests
{
    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public void ExplicitArgNamesTest_NoSelector()
        {
            var source = "cmd --arg1 --arg2 -3 -arg4";
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.GetTokens(source).ToArray();

            Assert.IsInstanceOfType(tokens[0], typeof(CommandToken));
            Assert.AreEqual("cmd", (tokens[0] as CommandToken).Name);

            Assert.IsInstanceOfType(tokens[1], typeof(ArgNameToken));
            Assert.AreEqual("arg1", (tokens[1] as ArgNameToken).Name);
            Assert.IsInstanceOfType(tokens[2], typeof(ArgNameToken));
            Assert.AreEqual("arg2", (tokens[2] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[3], typeof(ArgNameToken));
            Assert.IsTrue((tokens[3] as ArgNameToken).IsShort);
            Assert.AreEqual("3", (tokens[3] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[4], typeof(ArgNameToken));
            Assert.IsFalse((tokens[4] as ArgNameToken).IsShort);
            Assert.AreEqual("arg4", (tokens[4] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[5], typeof(EOCToken));
        }

        [TestMethod]
        public void ExplicitArgNamesTest_Selector()
        {
            var source = "cmd selector --arg1 --arg2 -3 -arg4";
            var tokenizer = new Tokenizer();
            tokenizer.Selectors.Add("selector");
            var tokens = tokenizer.GetTokens(source).ToArray();


            Assert.IsInstanceOfType(tokens[0], typeof(CommandToken));
            Assert.AreEqual("cmd", (tokens[0] as CommandToken).Name);

            Assert.IsInstanceOfType(tokens[1], typeof(SelectorToken));
            Assert.AreEqual("selector", (tokens[1] as SelectorToken).Name);

            Assert.IsInstanceOfType(tokens[2], typeof(ArgNameToken));
            Assert.AreEqual("arg1", (tokens[2] as ArgNameToken).Name);
            Assert.IsInstanceOfType(tokens[3], typeof(ArgNameToken));
            Assert.AreEqual("arg2", (tokens[3] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[4], typeof(ArgNameToken));
            Assert.IsTrue((tokens[4] as ArgNameToken).IsShort);
            Assert.AreEqual("3", (tokens[4] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[5], typeof(ArgNameToken));
            Assert.IsFalse((tokens[5] as ArgNameToken).IsShort);
            Assert.AreEqual("arg4", (tokens[5] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[6], typeof(EOCToken));
        }

        [TestMethod]
        public void ExplicitArgNamesWithValuesTest()
        {
            var source = "cmd selector --arg1 value1 --arg2 -3 3.14 -arg4 True --arg5 2020";
            var tokenizer = new Tokenizer();
            tokenizer.Selectors.Add("selector");
            var tokens = tokenizer.GetTokens(source).ToArray();


            Assert.IsInstanceOfType(tokens[0], typeof(CommandToken));
            Assert.AreEqual("cmd", (tokens[0] as CommandToken).Name);

            Assert.IsInstanceOfType(tokens[1], typeof(SelectorToken));
            Assert.AreEqual("selector", (tokens[1] as SelectorToken).Name);


            Assert.IsInstanceOfType(tokens[2], typeof(ArgNameToken));
            Assert.AreEqual("arg1", (tokens[2] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[3], typeof(ArgValueToken<string>));
            Assert.AreEqual("value1", (tokens[3] as ArgValueToken<string>).ActualValue);


            Assert.IsInstanceOfType(tokens[4], typeof(ArgNameToken));
            Assert.AreEqual("arg2", (tokens[4] as ArgNameToken).Name);


            Assert.IsInstanceOfType(tokens[5], typeof(ArgNameToken));
            Assert.IsTrue((tokens[5] as ArgNameToken).IsShort);
            Assert.AreEqual("3", (tokens[5] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[6], typeof(ArgValueToken<double>));
            Assert.AreEqual(3.14, (tokens[6] as ArgValueToken<double>).ActualValue);


            Assert.IsInstanceOfType(tokens[7], typeof(ArgNameToken));
            Assert.IsFalse((tokens[7] as ArgNameToken).IsShort);
            Assert.AreEqual("arg4", (tokens[7] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[8], typeof(ArgValueToken<bool>));
            Assert.AreEqual(true, (tokens[8] as ArgValueToken<bool>).ActualValue);


            Assert.IsInstanceOfType(tokens[9], typeof(ArgNameToken));
            Assert.IsFalse((tokens[9] as ArgNameToken).IsShort);
            Assert.AreEqual("arg5", (tokens[9] as ArgNameToken).Name);

            Assert.IsInstanceOfType(tokens[10], typeof(ArgValueToken<long>));
            Assert.AreEqual(2020, (tokens[10] as ArgValueToken<long>).ActualValue);


            Assert.IsInstanceOfType(tokens[11], typeof(EOCToken));
        }


        [TestMethod]
        public void ImplicitArgNamesTest()
        {
            var source = "cmd selector value1 3.141";
            var tokenizer = new Tokenizer();
            tokenizer.Selectors.Add("selector");
            var tokens = tokenizer.GetTokens(source).ToArray();

            Assert.IsInstanceOfType(tokens[0], typeof(CommandToken));
            Assert.AreEqual("cmd", (tokens[0] as CommandToken).Name);


            Assert.IsInstanceOfType(tokens[1], typeof(SelectorToken));
            Assert.AreEqual("selector", (tokens[1] as SelectorToken).Name);


            Assert.IsInstanceOfType(tokens[2], typeof(ArgValueToken<string>));
            Assert.AreEqual("value1", (tokens[2] as ArgValueToken<string>).ActualValue);


            Assert.IsInstanceOfType(tokens[3], typeof(ArgValueToken<double>));
            Assert.AreEqual(3.141, (tokens[3] as ArgValueToken<double>).ActualValue);

            Assert.IsInstanceOfType(tokens[4], typeof(EOCToken));
        }

        [TestMethod]
        public void QuoteValueTest()
        {
            var source = "cmd selector \"value 1\" 3.141";
            var tokenizer = new Tokenizer();
            tokenizer.Selectors.Add("selector");
            var tokens = tokenizer.GetTokens(source).ToArray();

            Assert.IsInstanceOfType(tokens[0], typeof(CommandToken));
            Assert.AreEqual("cmd", (tokens[0] as CommandToken).Name);


            Assert.IsInstanceOfType(tokens[1], typeof(SelectorToken));
            Assert.AreEqual("selector", (tokens[1] as SelectorToken).Name);


            Assert.IsInstanceOfType(tokens[2], typeof(ArgValueToken<string>));
            Assert.AreEqual("value 1", (tokens[2] as ArgValueToken<string>).ActualValue);


            Assert.IsInstanceOfType(tokens[3], typeof(ArgValueToken<double>));
            Assert.AreEqual(3.141, (tokens[3] as ArgValueToken<double>).ActualValue);

            Assert.IsInstanceOfType(tokens[4], typeof(EOCToken));
        }

        [TestMethod]
        public void EscapedQuoteValueTest()
        {
            var source = "cmd selector \"value \\\"1\\\"\" 3.141";
            var tokenizer = new Tokenizer();
            tokenizer.Selectors.Add("selector");
            var tokens = tokenizer.GetTokens(source).ToArray();

            Assert.IsInstanceOfType(tokens[0], typeof(CommandToken));
            Assert.AreEqual("cmd", (tokens[0] as CommandToken).Name);


            Assert.IsInstanceOfType(tokens[1], typeof(SelectorToken));
            Assert.AreEqual("selector", (tokens[1] as SelectorToken).Name);


            Assert.IsInstanceOfType(tokens[2], typeof(ArgValueToken<string>));
            Assert.AreEqual("value \"1\"", (tokens[2] as ArgValueToken<string>).ActualValue);


            Assert.IsInstanceOfType(tokens[3], typeof(ArgValueToken<double>));
            Assert.AreEqual(3.141, (tokens[3] as ArgValueToken<double>).ActualValue);

            Assert.IsInstanceOfType(tokens[4], typeof(EOCToken));
        }

        [TestMethod]
        public void ChainedCommandsTest()
        {
            var source = "cmd selector \"value \\\"1\\\"\" 3.141; cmd selector \"value \\\"1\\\"\" 3.141";
            var tokenizer = new Tokenizer();
            tokenizer.AllowMultipleCommands = true;
            tokenizer.Selectors.Add("selector");
            var tokens = tokenizer.GetTokens(source).ToArray();

            for (int i = 0; i < 6; i += 5)
            {
                Assert.IsInstanceOfType(tokens[i], typeof(CommandToken));
                Assert.AreEqual("cmd", (tokens[i] as CommandToken).Name);


                Assert.IsInstanceOfType(tokens[i + 1], typeof(SelectorToken));
                Assert.AreEqual("selector", (tokens[i + 1] as SelectorToken).Name);


                Assert.IsInstanceOfType(tokens[i + 2], typeof(ArgValueToken<string>));
                Assert.AreEqual("value \"1\"", (tokens[i + 2] as ArgValueToken<string>).ActualValue);


                Assert.IsInstanceOfType(tokens[i + 3], typeof(ArgValueToken<double>));
                Assert.AreEqual(3.141, (tokens[i + 3] as ArgValueToken<double>).ActualValue);

                Assert.IsInstanceOfType(tokens[i + 4], typeof(EOCToken));
            }
        }
    }
}
