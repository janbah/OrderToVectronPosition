using Order2VPos.Core.IoneApi.ItemCategories;
using Order2VPos.Core.IoneApi.Items;
using Order2VPos.Core.IoneApi.Orders;

namespace OrderToVectronPosition.IOneApi;

public interface IIoneClient
{
    Task SendPlus(bool allItems);
    Task<RefundResponse> ProcessRefund(int ioneId);
    Task<OrderListResponse> GetOrdersAsync(DateTime from, DateTime to);
    Task<ItemLinkLayerListResponse> GetAllItemLinkLayersAsync();
    Task<ItemCategoryListResponse> GetAllCategoriesAsync();
    Task<ItemListResponse> GetAllItemsAsync();
}