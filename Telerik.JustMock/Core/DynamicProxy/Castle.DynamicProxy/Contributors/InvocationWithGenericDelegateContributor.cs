﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Telerik.JustMock.Core.Castle.DynamicProxy.Contributors
{
	using System;
	using System.Diagnostics;
	using System.Reflection;

	using Telerik.JustMock.Core.Castle.DynamicProxy.Generators;
	using Telerik.JustMock.Core.Castle.DynamicProxy.Generators.Emitters;
	using Telerik.JustMock.Core.Castle.DynamicProxy.Generators.Emitters.SimpleAST;
	using Telerik.JustMock.Core.Castle.DynamicProxy.Tokens;

	internal class InvocationWithGenericDelegateContributor : IInvocationCreationContributor
	{
		private readonly Type delegateType;
		private readonly MetaMethod method;
		private readonly Reference targetReference;

		public InvocationWithGenericDelegateContributor(Type delegateType, MetaMethod method, Reference targetReference)
		{
			Debug.Assert(delegateType.IsGenericType, "delegateType.IsGenericType");
			this.delegateType = delegateType;
			this.method = method;
			this.targetReference = targetReference;
		}

		public ConstructorEmitter CreateConstructor(ArgumentReference[] baseCtorArguments, AbstractTypeEmitter invocation)
		{
			return invocation.CreateConstructor(baseCtorArguments);
		}

		public MethodInfo GetCallbackMethod()
		{
			return delegateType.GetMethod("Invoke");
		}

		public MethodInvocationExpression GetCallbackMethodInvocation(AbstractTypeEmitter invocation, Expression[] args,
		                                                              Reference targetField,
		                                                              MethodEmitter invokeMethodOnTarget)
		{
			var @delegate = GetDelegate(invocation, invokeMethodOnTarget);
			return new MethodInvocationExpression(@delegate, GetCallbackMethod(), args);
		}

		public Expression[] GetConstructorInvocationArguments(Expression[] arguments, ClassEmitter proxy)
		{
			return arguments;
		}

		private Reference GetDelegate(AbstractTypeEmitter invocation, MethodEmitter invokeMethodOnTarget)
		{
			var closedDelegateType = delegateType.MakeGenericType(invocation.GenericTypeParams);
			var localReference = invokeMethodOnTarget.CodeBuilder.DeclareLocal(closedDelegateType);
			var closedMethodOnTarget = method.MethodOnTarget.MakeGenericMethod(invocation.GenericTypeParams);
			var localTarget = new ReferenceExpression(targetReference);
			invokeMethodOnTarget.CodeBuilder.AddStatement(
				SetDelegate(localReference, localTarget, closedDelegateType, closedMethodOnTarget));
			return localReference;
		}

		private AssignStatement SetDelegate(LocalReference localDelegate, ReferenceExpression localTarget,
		                                    Type closedDelegateType, MethodInfo closedMethodOnTarget)
		{
			var delegateCreateDelegate = new MethodInvocationExpression(
				null,
				DelegateMethods.CreateDelegate,
				new TypeTokenExpression(closedDelegateType),
				localTarget,
				new MethodTokenExpression(closedMethodOnTarget));
			return new AssignStatement(localDelegate, new ConvertExpression(closedDelegateType, delegateCreateDelegate));
		}
	}
}