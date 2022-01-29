using System;

namespace AutoInority.Command
{
    internal interface ICommand : IComparable<ICommand>, IEquatable<ICommand>
    {
        bool IsCompleted { get; }

        bool IsApplicable { get; }

        bool Execute();

        int RepeatAfter();

        PriorityEnum Priority();
    }
}