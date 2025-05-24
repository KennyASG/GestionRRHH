namespace GestionRRHH.Web.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }

        public PaginatedList(List<T> items, int totalItems, int pageNumber, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            HasPrevious = pageNumber > 1;
            HasNext = pageNumber < TotalPages;
        }
    }

    public class EmpleadoFilter
    {
        public string? Estado { get; set; }
        public string? Nombre { get; set; }
        public string? Departamento { get; set; }
        public string? TipoNomina { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}