namespace AudioStation.Core.Model.Vendor.IdSharp
{
    public class UserDefinedField
    {
        public string Description { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("Description={0} Value={1}", this.Description, this.Value);
        }
    }
}
