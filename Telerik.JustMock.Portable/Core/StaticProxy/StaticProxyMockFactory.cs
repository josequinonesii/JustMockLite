﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Telerik.JustMock.Core.Castle.DynamicProxy;

namespace Telerik.JustMock.Core.StaticProxy
{
	internal class StaticProxyMockFactory : IMockFactory
	{
		public bool IsAccessible(Type type)
		{
			return type.IsPublic;
		}

		public object Create(Type type, MocksRepository repository, IMockMixin mockMixinImpl, MockCreationSettings settings, bool createTransparentProxy)
		{
			Type proxyType;
			if (!ProxySourceRegistry.ProxyTypes.TryGetValue(type, out proxyType))
			{
				throw new MockException(String.Format("No proxy type found for type '{0}'. Add [assembly: MockedType(typeof({0}))] to your test assembly.", type));
			}

			var interceptor = new DynamicProxyInterceptor(repository);

			return Activator.CreateInstance(proxyType, interceptor, mockMixinImpl);
		}

		public Type CreateDelegateBackend(Type delegateType)
		{
			throw new NotImplementedException();
		}

		public IMockMixin CreateExternalMockMixin(IMockMixin mockMixin, IEnumerable<object> mixins)
		{
			throw new NotImplementedException();
		}

		public ProxyTypeInfo CreateClassProxyType(Type classToProxy, MocksRepository repository, MockCreationSettings settings)
		{
			throw new NotImplementedException();
		}
	}
}