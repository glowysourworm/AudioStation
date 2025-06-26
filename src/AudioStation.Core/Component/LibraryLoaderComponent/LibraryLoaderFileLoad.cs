namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderFileLoad : LibraryLoaderLoadBase
    {
        protected Dictionary<string, bool> FilesProcessed { get; private set; }

        private object _lock = new object();

        public LibraryLoaderFileLoad(IEnumerable<string> files)
        {
            this.FilesProcessed = new Dictionary<string, bool>();

            foreach (var file in files)
            {
                if (!this.FilesProcessed.ContainsKey(file))
                    this.FilesProcessed.Add(file, false);
            }
        }

        public IEnumerable<string> GetFiles()
        {
            lock(_lock)
            {
                return this.FilesProcessed.Keys;
            }            
        }

        public void SetComplete(string file, bool success)
        {
            lock(_lock)
            {
                this.FilesProcessed[file] = success;
            }
        }

        public override double GetProgress()
        {
            lock(_lock)
            {
                return this.FilesProcessed.Values.Count(x => x) / (double)this.FilesProcessed.Count;
            }            
        }
    }
}
