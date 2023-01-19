using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Order2VPos.Core.Common;
using Order2VPos.Core.IoneApi;
using Order2VPos.Core.IoneApi.ItemCategories;
using Order2VPos.Core.IoneApi.Items;
using Order2VPos.Core.IoneApi.Orders;
using Order2VPos.Core.Models;
using Order2VPos.Core.VPosClient;
using Order2VPos.Core.VPosClient.MasterData;

namespace OrderToVectronPosition.IOneApi
{
    public class IoneClient :  IIoneClient
    {

        private readonly HttpClient _httpClient;
        
        DateTime allFromDate = new DateTime(1970, 1, 1);
        DateTime allToDate = DateTime.Now.AddYears(1);

        public IoneClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //UI Triggerd
        public async Task SendPlus(bool allItems)
        {
            ItemCategoryListResponse categoryListResponse = await GetAllCategoriesAsync();
            ItemLinkLayerListResponse itemLinkLayersListResponse = await GetAllItemLinkLayersAsync();
            ItemListResponse itemListResponse = await GetAllItemsAsync();
            MasterDataResponse vposMasterData = VPosCom.GetMasterData();

            if (itemLinkLayersListResponse.StatusCode != 200 && itemLinkLayersListResponse.StatusCode != 0)
                throw new Exception($"Fehler beim Abruf der IONE Artikelauswahl {itemLinkLayersListResponse.Message}");
            if (itemListResponse.StatusCode != 200 && itemListResponse.StatusCode != 0)
                throw new Exception($"Fehler beim Abruf der IONE Artikel {itemListResponse.Message}");
            if (categoryListResponse.StatusCode != 200 && categoryListResponse.StatusCode != 0)
                throw new Exception($"Fehler beim Abruf der IONE Kategorien {categoryListResponse.Message}");

            CoreDbContext dbContext = CoreDbContext.GetContext();

            // Hauptkategorie ermitteln bzw. übertragen

            string mainCategoryName = $"Main [#{AppSettings.Default.BranchAddressId}]";
            ItemCategory mainCategory = categoryListResponse.Data
                .FirstOrDefault(x => x.Name == mainCategoryName && x.LevelId == 1 && x.APIObjectId == "-1");
            int mainCategoryIoneRefId;
            if (mainCategory == null)
            {
                string jsonText = JsonConvert.SerializeObject(
                    new ItemCategory { LevelId = 1, APIObjectId = "-1", Name = mainCategoryName });
                var response = await _httpClient.PostAsync(
                    new Uri("SaveItemCategory", UriKind.Relative),
                    new StringContent(jsonText));
                response.EnsureSuccessStatusCode();
                string responseContentText = await response.Content.ReadAsStringAsync();
                if (ApiResponse.IsValid(responseContentText, out string errorMessage1))
                {
                    var responseResult = JsonConvert.DeserializeObject<ItemCategoryResponse>(responseContentText);
                    mainCategoryIoneRefId = responseResult.Data.Id;
                }
                else
                    throw new Exception($"Fehler beim Übertragen der Haupt-Kategorie\r\n{errorMessage1}");
            }
            else
                mainCategoryIoneRefId = mainCategory.Id;

            // Kategorien verarbeiten

            var currentCategories = categoryListResponse.Data
                .Where(
                    x => x.BranchAddressIdList.Contains(AppSettings.Default.BranchAddressId) &&
                        x.ParentId == mainCategoryIoneRefId)
                .ToArray();
            foreach (var currentCategory in currentCategories)
            {
                if (int.TryParse(currentCategory.APIObjectId, out int vectronNo))
                {
                    var dbCategory = dbContext.Categories.FirstOrDefault(x => x.VectronNo == vectronNo);
                    if (dbCategory == null)
                    {
                        dbCategory = new Category
                        {
                            Name = currentCategory.Name,
                            IoneRefId = currentCategory.Id,
                            VectronNo = vectronNo
                        };
                        dbContext.Categories.Add(dbCategory);
                    }
                    else
                        dbCategory.IoneRefId = currentCategory.Id;
                }
            }
            await dbContext.SaveChangesAsync();

            foreach (var dbCategory in dbContext.Categories)
            {
                ItemCategory itemCategory = currentCategories.FirstOrDefault(x => x.Id == dbCategory.IoneRefId);
                if (dbCategory.IoneRefId == 0 ||
                    !(itemCategory != null &&
                        dbCategory.Name == itemCategory.Name &&
                        itemCategory.ParentId == mainCategoryIoneRefId))
                {
                    var response = await _httpClient.PostAsync(
                        new Uri("SaveItemCategory", UriKind.Relative),
                        new StringContent(
                            JsonConvert.SerializeObject(
                                new ItemCategory
                                {
                                    Name = dbCategory.Name?.Trim(),
                                    APIObjectId = $"{dbCategory.VectronNo}",
                                    Id = dbCategory.IoneRefId,
                                    ParentId = mainCategoryIoneRefId
                                })));
                    response.EnsureSuccessStatusCode();
                    string responseContentText = await response.Content.ReadAsStringAsync();

                    if (ApiResponse.IsValid(responseContentText, out string errorMessage2))
                    {
                        var responseResult = JsonConvert.DeserializeObject<ItemCategoryResponse>(responseContentText);
                        dbCategory.IoneRefId = responseResult.Data.Id;
                        dbCategory.Name = responseResult.Data.Name;
                    }
                    else
                        throw new Exception(
                            $"Fehler beim Übertragen der Kategorie [{dbCategory.VectronNo}] {dbCategory.Name}\r\n{errorMessage2}");
                }
            }
            await dbContext.SaveChangesAsync();

            // Artikel für Webshop aus Kasse ermitteln

            var vposMainPlusForWebShop = vposMasterData.PLUs.Where(x => x.IsForWebShop).ToArray();
            var vposCondimentPlusForWebShop = vposMasterData.PLUs
                .Where(
                    x => vposMainPlusForWebShop.Any(
                        y => y.SelectWin
                            .Join(vposMasterData.SelWins, z => z, a => a.Number, (b, c) => c.PLUNos)
                            .Any(selPlus => selPlus.Contains(x.PLUno))))
                .ToArray();
            var vposPlusForWebShop = vposMainPlusForWebShop.Union(vposCondimentPlusForWebShop).ToArray();

            List<MappingArticle> mappingArticles = new List<MappingArticle>();
            List<Item> orphandItems = new List<Item>();
            if (itemListResponse.Data != null)
            {
                // Liste mit Artikeln füllen und zu deaktivierende Artikel für Webshop ermitteln

                List<Item> apiItemsWithPluNo = new List<Item>();
                foreach (var currentApiItem in itemListResponse.Data)
                {
                    if (int.TryParse(currentApiItem.APIObjectId, out int vectronPluNo) &&
                        vposPlusForWebShop.Any(x => x.PLUno == vectronPluNo))
                        apiItemsWithPluNo.Add(currentApiItem);
                    else if (currentApiItem.ItemWebshopLink)
                        orphandItems.Add(currentApiItem);
                }

                foreach (var apiItemGroup in apiItemsWithPluNo.GroupBy(x => Convert.ToInt32(x.APIObjectId)))
                {
                    var currentMappingArticle = mappingArticles.FirstOrDefault(x => x.VectronPluNo == apiItemGroup.Key);
                    if (currentMappingArticle == null)
                    {
                        currentMappingArticle = new MappingArticle { VectronPluNo = apiItemGroup.Key };
                        mappingArticles.Add(currentMappingArticle);
                    }

                    Item mainArticle = apiItemGroup.FirstOrDefault(x => x.ItemWebshopLink && x.ItemCategoryId.HasValue);
                    if (mainArticle == null)
                        mainArticle = apiItemGroup.FirstOrDefault(x => x.ItemCategoryId.HasValue);

                    var toRemoveItems = apiItemGroup.ToList();
                    if (mainArticle != null)
                    {
                        currentMappingArticle.IoneRefIdMain = mainArticle.Id;
                        currentMappingArticle.ItemCategoryIdMain = mainArticle.ItemCategoryId;
                        toRemoveItems.Remove(mainArticle);
                    }

                    Item condimentArticle = apiItemGroup.FirstOrDefault(
                        x => x.ItemWebshopLink &&
                            !x.ItemCategoryId.HasValue &&
                            itemLinkLayersListResponse.Data.Any(y => y.ItemLinkLayerList.Any(z => z.ItemID == x.Id)));
                    if (condimentArticle == null)
                        condimentArticle = apiItemGroup.FirstOrDefault(
                            x => !x.ItemCategoryId.HasValue &&
                                itemLinkLayersListResponse.Data.Any(y => y.ItemLinkLayerList.Any(z => z.ItemID == x.Id)));

                    if (condimentArticle != null)
                    {
                        currentMappingArticle.IoneRefIdCondiment = condimentArticle.Id;
                        toRemoveItems.Remove(condimentArticle);
                    }

                    orphandItems.AddRange(toRemoveItems.Where(x => x.ItemWebshopLink));
                }
            }

            // Artikel zum Webshop übertragen

            foreach (var vposPlu in vposPlusForWebShop)
            {
                MappingArticle mappingArticle = mappingArticles.FirstOrDefault(y => y.VectronPluNo == vposPlu.PLUno);
                bool isMain = vposMainPlusForWebShop.Contains(vposPlu);
                bool isCondiment = vposCondimentPlusForWebShop.Contains(vposPlu);
                int count = 1;
                if (isMain && isCondiment)
                    count = 2;

                for (int i = 0; i < count; i++)
                {
                    int id;
                    int itemCategoryId;
                    if (i == 0 && isMain)
                    {
                        id = mappingArticle?.IoneRefIdMain ?? 0;
                        itemCategoryId = dbContext.Categories.FirstOrDefault(x => x.VectronNo == vposPlu.MainGroupB)?.IoneRefId ??
                            0;
                    }
                    else
                    {
                        id = mappingArticle?.IoneRefIdCondiment ?? 0;
                        itemCategoryId = 0;
                    }


                    List<ItemPrice> itemPrices = new List<ItemPrice>();

                    foreach (var priceListAssigment in AppSettings.Default.PriceListAssignmentList)
                    {
                        ItemPrice itemPrice = new ItemPrice
                        {
                            BasePriceWithTax = vposPlu.Prices.FirstOrDefault(x => x.Level == priceListAssigment.VectronPriceLevel)?.Price.GetDecimalString() ?? "0",
                            PriceListId = priceListAssigment.PriceListId,
                            PriceListType = 1,
                            TaxPercentage = vposMasterData.Taxes.FirstOrDefault(x => x.TaxNo == vposPlu.TaxNo)?.Rate.GetDecimalString().Trim() ?? "0"
                        };

                        itemPrices.Add(itemPrice);
                    }

                    Item newOrChangedItem = new Item
                    {
                        APIObjectId = vposPlu.PLUno.ToString(),
                        ItemWebshopLink = true,
                        BasePriceWithTax = itemPrices.First().BasePriceWithTax,
                        Id = id,
                        Name = GetName(vposPlu),
                        ItemCategoryId = itemCategoryId,
                        TaxPercentage = vposMasterData.Taxes.FirstOrDefault(x => x.TaxNo == vposPlu.TaxNo)?.Rate.GetDecimalString().Trim() ?? "0",
                        BranchAddressIdList = new int[] { AppSettings.Default.BranchAddressId },
                        ItemPriceList = itemPrices
                    };

                    var currentItem = itemListResponse.Data?.FirstOrDefault(x => x.Id == id);
                    if (allItems || IsChanged(currentItem, newOrChangedItem))
                    {
                        string jsonText = JsonConvert.SerializeObject(newOrChangedItem);
                        var response = await _httpClient.PostAsync(
                            new Uri("SaveItem", UriKind.Relative),
                            new StringContent(jsonText));
                        response.EnsureSuccessStatusCode();
                        string responseContentText = await response.Content.ReadAsStringAsync();
                        if (ApiResponse.IsValid(responseContentText, out string errorMessage3))
                        {
                            var responseResult = JsonConvert.DeserializeObject<ItemResponse>(responseContentText);

                            if (mappingArticle == null)
                            {
                                mappingArticle = new MappingArticle { VectronPluNo = vposPlu.PLUno };
                                mappingArticles.Add(mappingArticle);
                            }

                            if (i == 0 && isMain)
                            {
                                mappingArticle.IoneRefIdMain = responseResult.Data.Id;
                                mappingArticle.ItemCategoryIdMain = responseResult.Data.ItemCategoryId;
                            }
                            else
                                mappingArticle.IoneRefIdCondiment = responseResult.Data.Id;
                        }
                        else
                            throw new Exception(
                                $"Fehler beim Übertragen eines Artikels [{vposPlu.PLUno}] {GetName(vposPlu)}\r\n{errorMessage3}");
                    }
                }
            }

            // Artikelauswahlen zum Webshop übertragen

            List<ItemLinkLayer> newBaseItemLinkLayers = new List<ItemLinkLayer>();
            foreach (var vposPlu in vposMainPlusForWebShop.Where(x => x.SelectWin?.Length > 0))
            {
                var selWin = vposMasterData.SelWins.First(x => x.Number == vposPlu.SelectWin.First());

                List<ItemLinkLayer> itemLinkLayerList = new List<ItemLinkLayer> { };
                foreach (var selPluNo in selWin.PLUNos)
                {
                    itemLinkLayerList.Add(
                        new ItemLinkLayer
                        {
                            APIObjectId = $"{selPluNo}",
                            ItemID = mappingArticles.First(x => x.VectronPluNo == selPluNo).IoneRefIdCondiment,
                            BranchAddressId = AppSettings.Default.BranchAddressId
                        });
                }

                ItemLinkLayer linkLayer = new ItemLinkLayer
                {
                    APIObjectId = $"{vposPlu.PLUno}",
                    BranchAddressId = AppSettings.Default.BranchAddressId,
                    ItemID = mappingArticles.First(x => x.VectronPluNo == vposPlu.PLUno).IoneRefIdMain,
                    Name = selWin.Name,
                    ItemLinkLayerList = itemLinkLayerList.ToArray(),
                    SelectionCounter = selWin.SelectCountIone,
                    SelectionConstraint = selWin.SelectCompulsion > 0,
                    Nullprice = selWin.ZeroPriceAllowed
                };

                newBaseItemLinkLayers.Add(linkLayer);
            }

            if (newBaseItemLinkLayers.Count > 0)
            {
                string linkLayersJsonText = JsonConvert.SerializeObject(newBaseItemLinkLayers);
                var addLinkLayersResponse = await _httpClient.PostAsync(
                    new Uri("SaveItemLinkLayer", UriKind.Relative),
                    new StringContent(linkLayersJsonText));
                addLinkLayersResponse.EnsureSuccessStatusCode();
                string addLinkLayersResponseText = await addLinkLayersResponse.Content.ReadAsStringAsync();

                if (ApiResponse.IsValid(addLinkLayersResponseText, out string errorMessage5))
                {
                    var addLinkLayersResponseResult = JsonConvert.DeserializeObject<ItemLinkLayerResponse>(
                        addLinkLayersResponseText);
                }
                else
                    throw new Exception($"Fehler beim Übertragen der Artikelauswahlen\r\n{errorMessage5}");
            }

            // Artikel im Webshop deaktivieren

            if (itemListResponse.Data != null)
            {
                var items = itemListResponse.Data.Where(x => orphandItems.Contains(x));
                foreach (var item in items)
                {
                    item.ItemWebshopLink = false;
                    string jsonText = JsonConvert.SerializeObject(item);
                    var response = await _httpClient.PostAsync(
                        new Uri("SaveItem", UriKind.Relative),
                        new StringContent(jsonText));
                    response.EnsureSuccessStatusCode();
                    string responseContentText = await response.Content.ReadAsStringAsync();
                    if (!ApiResponse.IsValid(responseContentText, out string errorMessage4))
                    {
                        throw new Exception($"Fehler beim Deaktivieren eines Artikels\r\n{errorMessage4}");
                    }
                }
            }

            new LogWriter().WriteEntry($"Artikelstammdaten wurden erfolgreich zum Webshop übertragen!", System.Diagnostics.EventLogEntryType.Information, 200);
        }

        private static bool IsChanged(Item currentItem, Item newItem)
        {
            if (currentItem == null)
                return true;

            bool priceEqual = true;

            foreach (var newPrice in newItem.ItemPriceList)
            {
                if (newPrice.BasePriceWithTax?.GetDecimal() != currentItem.ItemPriceList.FirstOrDefault(x => x.PriceListId == newPrice.PriceListId)?.BasePriceWithTax?.GetDecimal())
                {
                    priceEqual = false;
                    break;
                }
            }

            return !(newItem.Name == currentItem.Name &&
                priceEqual &&
                newItem.ItemCategoryId == (currentItem.ItemCategoryId ?? 0) &&
                newItem.ItemWebshopLink == currentItem.ItemWebshopLink &&
                newItem.TaxPercentage == currentItem.TaxPercentage);
        }

        private static string GetName(PLU vposPlu)
        {
            string result = string.Empty;
            switch (AppSettings.Default.NameNr)
            {
                case NameNr.Name1:
                    result = vposPlu.Name1;
                    break;
                case NameNr.Name2:
                    result = vposPlu.Name2;
                    break;
                case NameNr.Name3:
                    result = vposPlu.Name3;
                    break;
                default:
                    result = vposPlu.Name1;
                    break;
            }

            if (string.IsNullOrEmpty(result))
                result = vposPlu.Name1;

            return result?.Trim();
        }

        public async Task<RefundResponse> ProcessRefund(int ioneId)
        {
            string data = $"{{\"OrderId\":{ioneId}}}";

            HttpResponseMessage responseMessage = await _httpClient.PostAsync(
                new Uri("ProcessRefund", UriKind.Relative),
                new StringContent(data, Encoding.UTF8, "application/json"));

            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception($"HTTP-Error {responseMessage.StatusCode}");

            string jsonText = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RefundResponse>(jsonText);
        }

        public async Task<OrderListResponse> GetOrdersAsync(DateTime from, DateTime to)
        {
            string data = $"{{\"CreatedDateFrom\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", from)}\",\"CreatedDateTo\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", to)}\",\"BranchAddressId\": {AppSettings.Default.BranchAddressId}}}";

            HttpResponseMessage responseMessage = await _httpClient.PostAsync(
                new Uri("GetOrderList", UriKind.Relative),
                new StringContent(data, Encoding.UTF8, "application/json"));

            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception($"HTTP-Error {responseMessage.StatusCode}");

            string jsonText = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OrderListResponse>(jsonText);
        }

        public async Task<ItemLinkLayerListResponse> GetAllItemLinkLayersAsync()
        {
            string data = $"{{\"CreatedDateFrom\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allFromDate)}\",\"CreatedDateTo\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allToDate)}\"}}";

            HttpResponseMessage responseMessage = await _httpClient.PostAsync(
                new Uri("GetItemLinkLayerList", UriKind.Relative),
                new StringContent(data, Encoding.UTF8, "application/json"));

            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception($"HTTP-Error {responseMessage.StatusCode}");

            string jsonText = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ItemLinkLayerListResponse>(jsonText);
        }

        public async Task<ItemCategoryListResponse> GetAllCategoriesAsync()
        {
            //string data = $"{{\"CreatedDateFrom\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allFromDate)}\",\"CreatedDateTo\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allToDate)}\",\"BranchAddressId\": {AppSettings.Default.BranchAddressId}}}";

            string data = $"{{\"CreatedDateFrom\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allFromDate)}\",\"CreatedDateTo\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allToDate)}\"}}";


            HttpResponseMessage responseMessage = await _httpClient.PostAsync(
                new Uri("GetItemCategoryList", UriKind.Relative),
                new StringContent(data, Encoding.UTF8, "application/json"));

            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception($"HTTP-Error {responseMessage.StatusCode}");

            string jsonText = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ItemCategoryListResponse>(jsonText);
        }

        public async Task<ItemListResponse> GetAllItemsAsync()
        {
            string data = $"{{\"CreatedDateFrom\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allFromDate)}\",\"CreatedDateTo\":\"{string.Format(System.Globalization.CultureInfo.GetCultureInfo("de-DE"), "{0:dd.MM.yyyy HH:mm}", allToDate)}\",\"BranchAddressId\": {AppSettings.Default.BranchAddressId}}}";

            HttpResponseMessage responseMessage = await _httpClient.PostAsync(
                new Uri("GetItemList", UriKind.Relative),
                new StringContent(data, Encoding.UTF8, "application/json"));

            if (!responseMessage.IsSuccessStatusCode)
                throw new Exception($"HTTP-Error {responseMessage.StatusCode}");

            string jsonText = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ItemListResponse>(jsonText);
        }
    }
}
