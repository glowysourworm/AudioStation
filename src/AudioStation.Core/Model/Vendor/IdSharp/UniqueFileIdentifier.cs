using System.Text;

namespace AudioStation.Core.Model.Vendor.IdSharp
{
    public class UniqueFileIdentifier
    {
        public string OwnerIdentifier { get; set; }
        public byte[] Identifier { get; set; }

        public string GetString()
        {
            if (this.Identifier != null &&
                this.Identifier.Length > 0)
                return Encoding.UTF8.GetString(this.Identifier);

            else
                return string.Empty;
        }

        public override string ToString()
        {
            return string.Format("OwnerIdentifier={0} Identifier={1}", this.OwnerIdentifier, GetString());
        }
    }
}
