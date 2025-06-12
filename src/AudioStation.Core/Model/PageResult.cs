using Microsoft.EntityFrameworkCore.Query.Internal;

namespace AudioStation.Core.Model
{
    public class PageResult<TEntity>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int TotalRecordCount { get; set; }
        public int TotalRecordCountFiltered { get; set; }

        public IEnumerable<TEntity> Results { get; set; }

        public PageResult()
        {
            this.Results = Enumerable.Empty<TEntity>();
        }

        public static PageResult<TEntity> GetDefault()
        {
            return new PageResult<TEntity>()
            {
                PageNumber = 0,
                PageSize = 0,   
                PageCount = 0,
                TotalRecordCount = 0,
                TotalRecordCountFiltered = 0,
                Results = Enumerable.Empty<TEntity>()                
            };
        }
    }
}
