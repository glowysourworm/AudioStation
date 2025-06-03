using SimpleWpf.Extensions;
using SimpleWpf.RecursiveSerializer.Component.Interface;
using SimpleWpf.RecursiveSerializer.Interface;

namespace AudioStation.Model
{
    public class Artist : ViewModelBase, IRecursiveSerializable
    {
        string _name;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }

        public Artist()
        {
            this.Name = string.Empty;
        }
        public Artist(IPropertyReader reader)
        {
            this.Name = reader.Read<string>("Name");
        }
        public void GetProperties(IPropertyWriter writer)
        {
            writer.Write("Name", this.Name);
        }
    }
}
