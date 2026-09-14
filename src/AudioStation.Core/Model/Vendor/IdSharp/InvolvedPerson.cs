namespace AudioStation.Core.Model.Vendor.IdSharp
{
    public class InvolvedPerson
    {
        public string Name { get; set; }
        public string Involvement { get; set; }

        public override string ToString()
        {
            return string.Format("Name={0} Involvement={1}", this.Name, this.Involvement);
        }
    }
}
