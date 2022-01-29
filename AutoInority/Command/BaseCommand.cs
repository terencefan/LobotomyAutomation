namespace AutoInority.Command
{
    internal abstract class BaseCommand : ICommand
    {
        public abstract bool IsCompleted { get; }

        public abstract bool IsApplicable { get; }

        public int CompareTo(ICommand other) => other.Priority().CompareTo(Priority());

        public abstract bool Equals(ICommand other);

        public abstract bool Execute();

        public abstract PriorityEnum Priority();

        public int RepeatAfter() => 60;
    }
}
