using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Parslets.Default;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Shared.ECS.ActorQuerying.Parsing.Tokens.Default;
using Wallop.Shared.ECS.ActorQuerying.Queries;

namespace Wallop.Shared.ECS.ActorQuerying.Parsing
{
    public class QueryParser
    {
        public static IEnumerable<IPrefixParslet> PrefixParslets => _prefixParsletLookup.Values;
        public static IEnumerable<IInfixParslet> InfixParslets => _infixParsletLookup.Values;

        private static Dictionary<Type, IPrefixParslet> _prefixParsletLookup;
        private static Dictionary<Type, IInfixParslet> _infixParsletLookup;

        static QueryParser()
        {
            _prefixParsletLookup = new Dictionary<Type, IPrefixParslet>();
            _infixParsletLookup = new Dictionary<Type, IInfixParslet>();

            BindingPowerLookup.Set<SummationParslet>(1500);
            BindingPowerLookup.Set<ProductParslet>(1400);
            BindingPowerLookup.Set<PowParslet>(1300);
            BindingPowerLookup.Set<CallParslet>(1200);
            BindingPowerLookup.Set<ComparisonParslet>(1000);
            BindingPowerLookup.Set<SummationPrefixParslet>(700);
            BindingPowerLookup.Set<LogicalPrefixParslet>(600);
            BindingPowerLookup.Set<LogicalInfixParslet>(500);
            BindingPowerLookup.Set<BasicCollectionParslet>(400);
            BindingPowerLookup.Set<ComplexCollectionParslet>(300);
            BindingPowerLookup.Set<PipeParslet>(200);


            RegisterParslet<AndToken>(new LogicalInfixParslet());
            RegisterParslet<OrToken>(new LogicalInfixParslet());
            RegisterParslet<NotToken>(new LogicalPrefixParslet());

            RegisterParslet<AdditionToken>(new SummationPrefixParslet());
            RegisterParslet<SubtractionToken>(new SummationPrefixParslet());

            RegisterParslet<ComparisonToken>(new ComparisonParslet());

            RegisterParslet<LParenToken>(new CallParslet());
            RegisterParslet<IdentifierToken>(new IdentifierParslet());

            RegisterParslet<AllToken>(new BasicCollectionParslet());
            RegisterParslet<FirstToken>(new BasicCollectionParslet());
            RegisterParslet<LastToken>(new BasicCollectionParslet());
            RegisterParslet<FilterToken>(new ComplexCollectionParslet());
            RegisterParslet<EditToken>(new ComplexCollectionParslet());

            RegisterParslet<IntToken>(new LiteralParslet());
            RegisterParslet<RealToken>(new LiteralParslet());
            RegisterParslet<BoolToken>(new LiteralParslet());
            RegisterParslet<StringToken>(new LiteralParslet());

            RegisterParslet<AdditionToken>(new SummationParslet());
            RegisterParslet<SubtractionToken>(new SummationParslet());
            RegisterParslet<ProductToken>(new ProductParslet());
            RegisterParslet<DivideToken>(new ProductParslet());
            RegisterParslet<PowToken>(new ProductParslet());

            RegisterParslet<PipeToken>(new PipeParslet());
        }

        public static void RegisterParslet<T>(IPrefixParslet parslet) where T : IToken
        {
            _prefixParsletLookup.Add(typeof(T), parslet);
        }

        public static void RegisterParslet<T>(IInfixParslet parslet) where T : IToken
        {
            _infixParsletLookup.Add(typeof(T), parslet);
        }


        public static IPrefixParslet? GetPrefixParslet(Type tokenType)
        {
            if (_prefixParsletLookup.TryGetValue(tokenType, out var prefixParslet))
            {
                return prefixParslet;
            }
            return null;
        }

        public static IInfixParslet? GetInfixParslet(Type tokenType)
        {
            if (_infixParsletLookup.TryGetValue(tokenType, out var infixParslet))
            {
                return infixParslet;
            }
            return null;
        }




        public string ParseResult { get; private set; }
        public bool ParseFailed { get; private set; }


        private Queue<IToken> _tokenStream;

        public QueryParser(string input)
        {
            Tokenizer tokenizer = new Tokenizer();
            _tokenStream = new Queue<IToken>(tokenizer.GetStream(input));

            ParseResult = string.Empty;
            ParseFailed = false;
        }

        public IQuery Parse()
            => (IQuery)ParseNextExpression(0);

        public IExpression ParseNextExpression(int precedence)
        {
            var token = _tokenStream.Dequeue();

            if (!_prefixParsletLookup.TryGetValue(token.GetType(), out var prefixParslet))
            {
                throw new InvalidOperationException($"Failed to parse token '{token.Value}' at position '{token.Index}'.");
            }

            var left = prefixParslet.Parse(this, token);

            while (precedence < PeekBindingPower())
            {
                token = _tokenStream.Dequeue();

                if (!_infixParsletLookup.TryGetValue(token.GetType(), out var infixParslet))
                {
                    throw new InvalidOperationException($"Failed to parse token '{token.Value}' at position '{token.Index}'.");
                }

                left = infixParslet.Parse(this, token, left);
            }

            return left;
        }

        public bool Expect<TExpected>() where TExpected : IToken
            => Peek() is TExpected;

        public bool Match<TExpected>() where TExpected : IToken
        {
            if (Expect<TExpected>())
            {
                Consume();
                return true;
            }
            return false;
        }

        public IToken Peek()
            => _tokenStream.Peek();

        public IToken Consume()
            => _tokenStream.Dequeue();

        public IToken Consume<TExpected>(bool allowEndOfStream = true) where TExpected : IToken
        {
            var token = _tokenStream.Dequeue();
            if (token is TExpected || allowEndOfStream && token is EndOfStreamToken)
            {
                return token;
            }
            throw new InvalidOperationException($"Expected token of type {typeof(TExpected)}. Found {token.GetType()} instead.");
        }


        private int PeekBindingPower()
        {
            var target = Peek();
            var targetType = Peek().GetType();
            if (_infixParsletLookup.TryGetValue(targetType, out var parselet))
            {
                return parselet.BindingPower;
            }
            return 0;
        }
    }
}
