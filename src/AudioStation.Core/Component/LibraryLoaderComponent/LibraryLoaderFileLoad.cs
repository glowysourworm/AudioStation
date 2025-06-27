namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderFileLoad : LibraryLoaderLoadBase
    {
        protected Dictionary<string, bool> FilesProcessed { get; private set; }
        protected Dictionary<string, bool> FilesPending { get; private set; }

        private object _lock = new object();

        public LibraryLoaderFileLoad(IEnumerable<string> files)
        {
            this.FilesProcessed = new Dictionary<string, bool>();
            this.FilesPending = new Dictionary<string, bool>();

            foreach (var file in files)
            {
                if (!this.FilesPending.ContainsKey(file))
                    this.FilesPending.Add(file, true);
            }
        }

        public IEnumerable<string> GetPendingFiles()
        {
            lock(_lock)
            {
                return this.FilesPending.Keys;
            }            
        }

        public void SetComplete(string file, bool success)
        {
            lock(_lock)
            {
                if (!this.FilesProcessed.ContainsKey(file))
                    this.FilesProcessed.Add(file, success);         // Result Status

                this.FilesPending[file] = false;                    // Pending Status
            }
        }

        public override double GetProgress()
        {
            lock(_lock)
            {
                return this.FilesProcessed.Keys.Count / (double)this.FilesPending.Keys.Count;
            }            
        }

        public override bool HasErrors()
        {
            lock (_lock)
            {
                return this.FilesProcessed.Values.Any(x => !x);
            }
        }

        public override int GetFailureCount()
        {
            lock (_lock)
            {
                return this.FilesProcessed.Values.Count(x => !x);
            }
        }

        public override int GetSuccessCount()
        {
            lock (_lock)
            {
                return this.FilesProcessed.Values.Count(x => x);
            }
        }
    }
}
