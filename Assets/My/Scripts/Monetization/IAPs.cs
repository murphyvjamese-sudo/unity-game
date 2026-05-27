using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

//left off: read block comment below. I also created a new "Indie" folder in my murphyvjamese gdrive for fresh developer notes, including project designs and general learning notes. (I made one specifically for learning the architecture behind asynchronous and event driven programming. I need to learn these better not only so I can properly / easily implement IAPs and leaderboards, but also for job interviews.
/*
My implementation of this script for IAPs is heavily borrowed from the documentation example at https://docs.unity.com/en-us/iap/upgrade-to-iap-v5#code-sample-of-new-initialization-process-using-storecontroller
*/

public class IAPs: MonoBehaviour
{
    private StoreController storeController;
    private List<ProductDefinition> productCatalog;
    private GlobalState gs;

    void Start()
    {
        gs = FindFirstObjectByType<GlobalState>();
        InitializeIAP();
    }

    public async void InitializeIAP()  
    {  
        storeController = UnityIAPServices.StoreController();  
    
        //You might want to uncomment the below line of code if you want to implement a visual indication that your purchase is still processing. It becomes more necessary if you want to prevent duplicate purchases of consumables, but since I only have non-consumables, it should prevent duplicate purchases and be less of an issue.
        //storeController.OnPurchasePending += OnPurchasePending;  
    
        await storeController.Connect();  
    
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
    
        productCatalog = new List<ProductDefinition>  
        {  
            new("GameB", ProductType.NonConsumable),  
        };  
    
        storeController.FetchProducts(productCatalog);  
    }
    public void RestorePurchases()
    {
        Debug.Log("RestorePurchases");
        storeController.FetchProducts(productCatalog);
    }
    void OnProductsFetched(List<Product> products)  
    { //apple/google return a catalog of what is available to purchase, as well as the prices
        storeController.FetchPurchases();  
    }  
    void OnPurchasesFetched(Orders orders) 
    {  //apple/google return which items have already been purchased by the player. Especially important for restoring purchases after someone deletes / reinstalls the app
    
    }
    public void RequestPlatformBillingUI(string productID)
    { //a wrapper that calls PurchaseProduct(), which brings up the apple/google/fakeStore dialogue to confirm buying an in app purchase
        Debug.Log("RequestPlatformBillingUI()");
        Sgs.ButtonHandler(Sgs.SgsButtonHandler.Loading); //pass SgsButtonHandler.Loading to make this create a page that says "loading" so that users can't spam-click your store's ui and double purchase consumables, or tweak out the logic for "double purchasing" a non consumable
        storeController.PurchaseProduct(productID);
    }
    void OnPurchasePending(PendingOrder order)
    { //handles event that is triggered when user clicks YOUR own store's ui button to bring up the platform-specific billing ui. (Necessary because during this connection delay, a user might spam click a button multiple times, and attempt to perform multiple transactions. However, I might not even need this event, as I can just change to a different page that says "loading" in response to the RequestPlatformBillingUI() that gets called when I click on my UI button)
        Debug.Log("OnPurchasePending()");
        ProcessPurchase(order);
        storeController.ConfirmPurchase(order);
        //ProcessPurchase(order);
    }
    void OnPurchaseConfirmed(Order order)
    { //triggered in response to clicking "buy" or the likes on the apple / google / fake store billing ui
        Debug.Log("OnPurchaseConfirmed()");
    }
    void OnPurchaseFailed(FailedOrder failedOrder)
    { //triggered in response to clicking "cancel" or the likes on the apple / google / fake store billing ui
        Debug.Log("Canceled Billing Transaction");
        Sgs.NewMenuPage(Sgs.Pages.Home);
    }
    void ProcessPurchase(Order order)
    { //process each item that was requested to be purchased, and then call GrantProduct() on each item to handle the actual sgs game logic of what should be awarded in response to it.
        foreach (var product in order.CartOrdered.Items())
        {
            // Grant product
            GrantProduct(product);
        }
        // Confirm the order to finalize the transaction
        //storeController.ConfirmPurchase(order);
    }
    private void GrantProduct(CartItem cartItem)
    { //actual sgs game logic of how to implement a product or reward that was purchased (unlocking access to new game mode or consumable or whatever)
        string product = cartItem.Product.definition.id;
        if(gs != null)
        {
            switch(product)
            {
                case "GameB":
                Debug.Log("unlocked game b");
                gs.hasUnlockedGameB = true;
                Sgs.NewMenuPage(Sgs.Pages.Home);
                break;
            }
        }
        else
        {
            Debug.LogWarning("Jim. Shouldn't happen. Need local vars to reward IAPs");
        }
    }
}
