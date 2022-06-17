using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Expressions;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Parslets;
using Wallop.Engine.ECS.ActorQuerying.Parsing.Tokens;
using Wallop.Engine.ECS.ActorQuerying.Queries;

namespace Wallop.Engine.ECS.ActorQuerying.Parsing
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

            // TODO: Register these elsewhere so plugins can alter this.
            BindingPowerLookup.Set<Parslets.Default.BasicCollectionParslet>(9000);
            BindingPowerLookup.Set<Parslets.Default.ComplexCollectionParslet>(9000);

            BindingPowerLookup.Set<Parslets.Default.LiteralParslet>(100);
            BindingPowerLookup.Set<Parslets.Default.SummationParslet>(110);
            BindingPowerLookup.Set<Parslets.Default.ProductParslet>(150);
            BindingPowerLookup.Set<Parslets.Default.PowParslet>(130);
            BindingPowerLookup.Set<Parslets.Default.CallParslet>(150);
            BindingPowerLookup.Set<Parslets.Default.IdentifierParslet>(100);
            BindingPowerLookup.Set<Parslets.Default.PipeParslet>(10000);


            RegisterParslet<Tokens.Default.LParenToken>(new Parslets.Default.CallParslet());
            RegisterParslet<Tokens.Default.IdentifierToken>(new Parslets.Default.IdentifierParslet());

            RegisterParslet<Tokens.Default.AllToken>(new Parslets.Default.BasicCollectionParslet());
            RegisterParslet<Tokens.Default.FirstToken>(new Parslets.Default.BasicCollectionParslet());
            RegisterParslet<Tokens.Default.LastToken>(new Parslets.Default.BasicCollectionParslet());
            RegisterParslet<Tokens.Default.FilterToken>(new Parslets.Default.ComplexCollectionParslet());

            RegisterParslet<Tokens.Default.IntToken>(new Parslets.Default.LiteralParslet());
            RegisterParslet<Tokens.Default.RealToken>(new Parslets.Default.LiteralParslet());
            RegisterParslet<Tokens.Default.BoolToken>(new Parslets.Default.LiteralParslet());
            RegisterParslet<Tokens.Default.StringToken>(new Parslets.Default.LiteralParslet());

            RegisterParslet<Tokens.Default.AdditionToken>(new Parslets.Default.SummationParslet());
            RegisterParslet<Tokens.Default.SubtractionToken>(new Parslets.Default.SummationParslet());
            RegisterParslet<Tokens.Default.ProductToken>(new Parslets.Default.ProductParslet());
            RegisterParslet<Tokens.Default.DivideToken>(new Parslets.Default.ProductParslet());
            RegisterParslet<Tokens.Default.PowToken>(new Parslets.Default.ProductParslet());

            RegisterParslet<Tokens.Default.PipeToken>(new Parslets.Default.PipeParslet());
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
            if(_prefixParsletLookup.TryGetValue(tokenType, out var prefixParslet))
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

            if(!_prefixParsletLookup.TryGetValue(token.GetType(), out var prefixParslet))
            {
                throw new InvalidOperationException($"Failed to parse token '{token.Value}' at position '{token.Index}'.");
            }

            var left = prefixParslet.Parse(this, token);

            while (precedence < PeekBindingPower())
            {
                token = _tokenStream.Dequeue();

                if(!_infixParsletLookup.TryGetValue(token.GetType(), out var infixParslet))
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
            if(Expect<TExpected>())
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
            if(token is TExpected || (allowEndOfStream && token is EndOfStreamToken))
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
