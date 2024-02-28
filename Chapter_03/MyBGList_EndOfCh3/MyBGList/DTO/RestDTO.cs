namespace MyBGList.DTO
{
    //DTO Class for hosting the actual data
    public class RestDTO<T>
    {
        public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();

        public T Data { get; set; } = default!;
    }
}
