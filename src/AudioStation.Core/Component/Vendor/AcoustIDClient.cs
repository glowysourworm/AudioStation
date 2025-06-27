using AcoustID.Web;

using AudioStation.Core.Model;

namespace AudioStation.Core.Component.Vendor
{
    public static class AcoustIDClient
    {
        /// <summary>
        /// Calculates library entry by audio fingerprint using an online api.
        /// </summary>
        public static async Task IdentifyFingerprint(LibraryEntry entry)
        {
            var context = new AcoustID.ChromaContext();

            var service = new LookupService();

            var response = await service.GetAsync(context.GetFingerprint(), 30);

            //response.Results.First().
        }
    }
}
