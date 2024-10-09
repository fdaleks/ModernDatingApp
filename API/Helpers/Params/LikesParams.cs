namespace API.Helpers.Params;

public class LikesParams : PaginationParams
{
    public int UserId { get; set; }

    public required string Predicate { get; set; } = "liked";
}
