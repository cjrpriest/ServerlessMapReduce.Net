using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureFromTheTrenches.Commanding.Abstractions;
using AzureFromTheTrenches.Commanding.Abstractions.Model;
using NSubstitute;
using NSubstitute.Core;

namespace ServerlessMapReduceDotNet.Tests.Extensions.CommandDispatcherMock
{
    public class RegisterCommandHandlerExpressionBuilder
    {
        private const int CommandHandlerCommandTypeGenericArgPosition = 0;
        private const int CommandHandlerResultTypeGenericArgPosition = 1;
        
        public Expression<Action<ICommandDispatcher, Func<TCommandHandler>>> Build<TCommandHandler>()
        {
            var commandDispatcherParameter = Expression.Parameter(typeof(ICommandDispatcher));
            var commandHandlerFactoryParameter = Expression.Parameter(typeof(Func<TCommandHandler>));
            
            var commandHandlerWithResultInterfaceTypeGenericArguments = typeof(TCommandHandler)
                .GetInterfaces()
                .First(t => typeof(ICommandHandler).IsAssignableFrom(t) && t.IsGenericType)
                .GetGenericArguments();

            var commandHandlerHasResult = commandHandlerWithResultInterfaceTypeGenericArguments.Length == 2;

            var commandType = commandHandlerWithResultInterfaceTypeGenericArguments[CommandHandlerCommandTypeGenericArgPosition];
            var resultType = commandHandlerHasResult
                    ? commandHandlerWithResultInterfaceTypeGenericArguments[CommandHandlerResultTypeGenericArgPosition]
                    : null;
            
            var returnsExpression = BuildReturnsExpression<TCommandHandler>(resultType, commandType, commandDispatcherParameter, commandHandlerFactoryParameter);

            var lambdaExpression = Expression.Lambda<Action<ICommandDispatcher, Func<TCommandHandler>>>(
                returnsExpression,
                commandDispatcherParameter,
                commandHandlerFactoryParameter);

            return lambdaExpression;
        }

        private static MethodCallExpression BuildReturnsExpression<TCommandHandler>(Type resultType, Type commandType, ParameterExpression commandDispatcherParameter, ParameterExpression commandHandlerFactoryParameter)
        {
            var commandResultWithResultType = resultType == null ? typeof(CommandResult) : typeof(CommandResult<>)
                .MakeGenericType(resultType);
            
            var dispatchAsyncExpression = BuildDispatchAsyncExpression(resultType, commandType, commandDispatcherParameter);

            var returnThisLambaExpression = BuildReturnThisLambaExpression<TCommandHandler>(commandHandlerFactoryParameter, commandType, commandResultWithResultType, resultType);

            var returnsMethodGeneric =
                typeof(SubstituteExtensions)
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .First(x => x.Name == nameof(SubstituteExtensions.Returns)
                            && IsParameterType(x.GetParameters()[0], typeof(Task<>))
                            && IsParameterType(x.GetParameters()[1], typeof(Func<,>)))
                    .MakeGenericMethod(commandResultWithResultType);

            var returnThisLambdaType =
                typeof(Func<,>)
                    .MakeGenericType(typeof(CallInfo), commandResultWithResultType);

            var returnsExpression =
                Expression.Call(
                    null,
                    returnsMethodGeneric,
                    dispatchAsyncExpression,
                    returnThisLambaExpression,
                    Expression.NewArrayInit(returnThisLambdaType));
            return returnsExpression;
        }

        private static LambdaExpression BuildReturnThisLambaExpression<TCommandHandler>(
            ParameterExpression commandHandlerFactoryParameter, Type commandType, Type commandResultWithResultType,
            Type resultType)
        {
            var commandHandlerExpression =
                Expression.Invoke(
                    Expression.Lambda<Func<TCommandHandler>>(
                        Expression.Invoke(commandHandlerFactoryParameter)
                    )
                );

            var callInfoParameter = Expression.Parameter(typeof(CallInfo));
            var commandArgumentFromCallInfoExpression = Expression.Call(callInfoParameter, nameof(Arg), new[] {commandType});

            var commandHandlerArguments = resultType == null
                ? new Expression[] {commandArgumentFromCallInfoExpression}
                : new Expression[] {commandArgumentFromCallInfoExpression, Expression.Default(resultType)};

            var commandHandlerCallInTaskExpression = BuildCommandHandlerCallInTaskExpression(resultType, commandHandlerExpression, commandHandlerArguments);

            var returnThisLambaBodyExpression = BuildReturnThisLambaBodyExpression(commandResultWithResultType, resultType, commandHandlerCallInTaskExpression);

            return Expression.Lambda(returnThisLambaBodyExpression, callInfoParameter);
        }

        private static Expression BuildCommandHandlerCallInTaskExpression(Type resultType,
            InvocationExpression commandHandlerExpression, Expression[] commandHandlerArguments)
        {
            var commandHandlerCallExpression = Expression.Call(
                commandHandlerExpression,
                nameof(ICommandHandler<ICommand>.ExecuteAsync),
                new Type[] { },
                commandHandlerArguments
            );

            var commandHandlerCallInTaskExpression = resultType == null
                ? Expression.Call(
                    commandHandlerCallExpression,
                    nameof(Task.Wait),
                    new Type[] { })
                : (Expression) Expression.Property(
                    commandHandlerCallExpression,
                    nameof(Task<object>.Result)
                );
            return commandHandlerCallInTaskExpression;
        }

        private static Expression BuildReturnThisLambaBodyExpression(Type commandResultWithResultType,
            Type resultType, Expression commandHandlerCallInTaskExpression)
        {
            Expression returnThisLambaBodyExpression;

            if (resultType != null)
            {
                returnThisLambaBodyExpression =
                    Expression.New(
                        commandResultWithResultType.GetConstructor(new[] {resultType, typeof(bool)}),
                        commandHandlerCallInTaskExpression,
                        Expression.Constant(false));
            }
            else
            {
                returnThisLambaBodyExpression =
                    Expression.Block(
                        commandHandlerCallInTaskExpression,
                        Expression.New(
                            commandResultWithResultType.GetConstructor(new[] {typeof(bool)}),
                            Expression.Constant(false)
                        )
                    );
            }

            return returnThisLambaBodyExpression;
        }

        public static MethodCallExpression BuildDispatchAsyncExpression(Type resultType, Type commandType,
            ParameterExpression commandDispatcherParameter)
        {
            var dispatchAsyncMethod = typeof(IFrameworkCommandDispatcher)
                .GetMethods()
                .First(x => x.Name == nameof(IFrameworkCommandDispatcher.DispatchAsync)
                        && IsParameterType(x.GetParameters()[0], resultType == null ? typeof(ICommand) : typeof(ICommand<>))
                        && IsParameterType(x.GetParameters()[1], typeof(CancellationToken)));
            
            if (resultType != null)
                dispatchAsyncMethod = dispatchAsyncMethod.MakeGenericMethod(resultType);

            var argAnyCommandExpression = Expression.Call(typeof(Arg), nameof(Arg.Any), new[] {commandType});

            var dispatchAsyncExpression = Expression.Call(commandDispatcherParameter, dispatchAsyncMethod,
                argAnyCommandExpression, Expression.Default(typeof(CancellationToken)));
            return dispatchAsyncExpression;
        }
        
        private static bool IsParameterType (ParameterInfo parameter, Type type)
        {
            if (parameter.ParameterType.IsGenericType)
            {
                if (!type.IsGenericType) return false;
                return parameter.ParameterType.GetGenericTypeDefinition() == type;
            }

            return parameter.ParameterType == type;
        }
    }
}