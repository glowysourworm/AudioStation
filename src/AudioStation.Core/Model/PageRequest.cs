using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Model
{
    /// <summary>
    /// Page request for a specific entity type
    /// </summary>
    public class PageRequest<TEntity, TOrder>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        /// <summary>
        /// Callbacks must be kept in terms of Postgres / EFCore grammar to succeed without performance
        /// delays.
        /// </summary>
        public Func<TEntity, TOrder> OrderByCallback { get; set; }

        /// <summary>
        /// Callbacks must be kept in terms of Postgres / EFCore grammar to succeed without performance
        /// delays.
        /// </summary>
        public Func<TEntity, bool> WhereCallback { get; set; }

        public PageRequest()
        {
            this.OrderByCallback = null;
            this.WhereCallback = null;
        }
    }
}
