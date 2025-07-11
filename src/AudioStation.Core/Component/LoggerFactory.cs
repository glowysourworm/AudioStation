using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AudioStation.Core.Component.Interface;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    /// <summary>
    /// Component to fit with MSFT's ILogger design. This would not have been required except for a new
    /// AutoMapper specification (see MapperConfiguration constructor)
    /// </summary>
    [IocExport(typeof(ILoggerFactory))]
    public class LoggerFactory : ILoggerFactory, ILoggerProvider
    {
        private readonly IOutputController _outputController;

        [IocImportingConstructor]
        public LoggerFactory(IOutputController outputController)
        {
            _outputController = outputController;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            // Nothing to do. We're also the provider :)
        }

        public ILogger CreateLogger(string categoryName)
        {
            // If we wanted to we could create one for each of our 3rd party components. I'd expect we may have Npgsql/EF DB messages
            // mixed in here along with AutoMapper.
            return _outputController;
        }

        public void Dispose()
        {
            // Nothing to do. We have central disposal.
        }
    }
}
