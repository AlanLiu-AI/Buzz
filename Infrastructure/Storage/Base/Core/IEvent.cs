namespace Runner.Base.Core
{
    using System;
    using System.Collections.Generic;

    public interface IEvent
    {
        /// <summary>
        /// Gets the command identifier.
        /// </summary>
        Guid Id { get; }
    }

    public interface IEventBus
    {
        void Send(Envelope<IEvent> command);
        void Send(IEnumerable<Envelope<IEvent>> commands);
    }
}
