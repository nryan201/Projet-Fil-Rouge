using Projet_Fil_Rouge.Entities;

namespace Projet_Fil_Rouge.Dto
{
    public class PagedCredentials
    {
        public List<CredentialListItem> Items { get; set; } = new();
        public int TotalItems { get; set; }      
        public int Page { get; set; }           
        public int PageSize { get; set; }      
        public int TotalPages { get; set; }     
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

}
