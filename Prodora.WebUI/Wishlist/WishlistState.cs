using System.Security.Claims;
using Prodora.Business.Abstract;

namespace Prodora.WebUI.Wishlist;

/// <summary>Per-request view helper: answers "is this product saved?" for every card with one query at most.</summary>
public sealed class WishlistState(IWishlistServices wishlist, IHttpContextAccessor accessor)
{
    private HashSet<int>? ids;

    public bool Contains(int productId)
    {
        if (ids == null)
        {
            var user = accessor.HttpContext?.User;
            var userId = user?.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            ids = userId == null ? new HashSet<int>() : wishlist.GetProductIds(userId);
        }
        return ids.Contains(productId);
    }
}
