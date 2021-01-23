
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Size
{
    [JsonPropertyName("height")] public int Height { get; set; }

    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("width")] public int Width { get; set; }
}

public class Photo
{
    [JsonPropertyName("album_id")] public int AlbumId { get; set; }

    [JsonPropertyName("date")] public int Date { get; set; }

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("owner_id")] public int OwnerId { get; set; }

    [JsonPropertyName("has_tags")] public bool HasTags { get; set; }

    [JsonPropertyName("sizes")] public List<Size> Sizes { get; set; }

    [JsonPropertyName("text")] public string Text { get; set; }

    [JsonPropertyName("user_id")] public int UserId { get; set; }

    [JsonPropertyName("access_key")] public string AccessKey { get; set; }

    [JsonPropertyName("post_id")] public int PostId { get; set; }
}

public class Link
{
    [JsonPropertyName("url")] public string Url { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("caption")] public string Caption { get; set; }

    [JsonPropertyName("description")] public string Description { get; set; }

    [JsonPropertyName("photo")] public Photo Photo { get; set; }

    [JsonPropertyName("is_favorite")] public bool IsFavorite { get; set; }
}

public class Attachment
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("link")] public Link Link { get; set; }

    [JsonPropertyName("photo")] public Photo Photo { get; set; }
}

public class PostSource
{
    [JsonPropertyName("type")] public string Type { get; set; }
}

public class Comments
{
    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("can_post")] public int CanPost { get; set; }

    [JsonPropertyName("groups_can_post")] public bool? GroupsCanPost { get; set; }
}

public class Likes
{
    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("user_likes")] public int UserLikes { get; set; }

    [JsonPropertyName("can_like")] public int CanLike { get; set; }

    [JsonPropertyName("can_publish")] public int CanPublish { get; set; }
}

public class Reposts
{
    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("user_reposted")] public int UserReposted { get; set; }
}

public class Views
{
    [JsonPropertyName("count")] public int Count { get; set; }
}

public class Donut
{
    [JsonPropertyName("is_donut")] public bool IsDonut { get; set; }
}

public class Item
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("from_id")] public int FromId { get; set; }

    [JsonPropertyName("owner_id")] public int OwnerId { get; set; }

    [JsonPropertyName("date")] public int Date { get; set; }

    [JsonPropertyName("marked_as_ads")] public int MarkedAsAds { get; set; }

    [JsonPropertyName("post_type")] public string PostType { get; set; }

    [JsonPropertyName("text")] public string Text { get; set; }

    [JsonPropertyName("is_pinned")] public int IsPinned { get; set; }

    [JsonPropertyName("attachments")] public List<Attachment> Attachments { get; set; }

    [JsonPropertyName("post_source")] public PostSource PostSource { get; set; }

    [JsonPropertyName("comments")] public Comments Comments { get; set; }

    [JsonPropertyName("likes")] public Likes Likes { get; set; }

    [JsonPropertyName("reposts")] public Reposts Reposts { get; set; }

    [JsonPropertyName("views")] public Views Views { get; set; }

    [JsonPropertyName("is_favorite")] public bool IsFavorite { get; set; }

    [JsonPropertyName("donut")] public Donut Donut { get; set; }

    [JsonPropertyName("short_text_rate")] public double ShortTextRate { get; set; }

    [JsonPropertyName("edited")] public int Edited { get; set; }
}

public class Response
{
    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("items")] public List<Item> Items { get; set; }
}

public class WallResponse
{
    [JsonPropertyName("response")] public Response Response { get; set; }
}