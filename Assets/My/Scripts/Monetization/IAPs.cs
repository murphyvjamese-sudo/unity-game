using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    }

    public async void InitializeIAP()  
    {  
        storeController = UnityIAPServices.StoreController();  

        if(gs != null)
        { //set offline values for prices of IAPs. Later in the flow of InitializeIAP() (OnPurchasesFetched() specifically), these values will be overwritten to the correct localized price.
            gs.gameBLocalizedPriceString = "[connect to internet]";
            gs.gameCLocalizedPriceString = "[connect to internet]";
        }
        
        await storeController.Connect();  
    
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnProductsFetchFailed += OnProductsFetchedFailed;
    
        productCatalog = new List<ProductDefinition>  
        {  
            new("com.neatstreet.cosmicchase.gameb", ProductType.NonConsumable),  
        };  
    
        storeController.FetchProducts(productCatalog);  
    }
    
    public void RestorePurchases()
    {
        Debug.LogWarning("RestorePurchases()");

        if (storeController == null)
        {
            Debug.LogWarning("Restore failed: StoreController is not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer || 
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // Lock UI to prevent spam-clicking
            Sgs.ButtonHandler(Sgs.SgsButtonHandler.Loading);

            Debug.LogWarning("Triggering Unity IAP v5 native Apple transaction restoration...");
            
            // In IAP v5, RestoreTransactions is built directly into StoreController!
            // It safely handles the native Apple ID password/FaceID authentication prompt.
            storeController.RestoreTransactions((success, error) => 
            {
                if (success)
                {
                    Debug.LogWarning("Apple Account verified successfully. Restoring items...");
                    // Once verified, pull the transactions down. This fires OnPurchasesFetched.
                    storeController.FetchPurchases();
                }
                else
                {
                    Debug.LogWarning($"Apple Restore Failed or Canceled by user. Error: {error}");
                    // Safe error recovery: Return to home and unlock the UI
                    Sgs.NewMenuPage(Sgs.Pages.Home); 
                }
            });
        }
        else
        {
            Debug.LogWarning("Skipping manual restore: Non-Apple platform.");
        }
    }
    
    void OnProductsFetched(List<Product> products)  
    { //apple/google return a catalog of what is available to purchase, as well as the prices
        foreach (var product in products)
        {
            switch (product.definition.id)
            {
                case "com.neatstreet.cosmicchase.gameb":
                    gs.gameBLocalizedPriceString = product.metadata.localizedPriceString;
                    break;
                case "com.neatstreet.cosmicchase.gamec":
                    gs.gameCLocalizedPriceString = product.metadata.localizedPriceString;
                    break;
            }
        }
        storeController.FetchPurchases();  
    }  
    void OnProductsFetchedFailed(ProductFetchFailed failure)
    { //this should happen if you attempt to fetch purchases (either by opening the app, or attempting to restore purchases) without being connected to wifi
        int gameB = SecurePlayerPrefs.GetInt("GameB", 0);
        Debug.LogWarning("Get Game B Offline: " + gameB);
        if(gameB == 1)
        {
            Debug.LogWarning("Grab game b from local memory");
            gs.hasUnlockedGameB = true;
        }
    }
    void OnPurchasesFetched(Orders orders) 
    {  //apple/google return which items have already been purchased by the player. Especially important for restoring purchases after someone deletes / reinstalls the app
        Debug.LogWarning("Apple/Google returned historical purchases.");
        
        bool foundGameB = false;

        // 1. Loop through already confirmed/paid historical orders
        foreach (var order in orders.ConfirmedOrders)
        {
            // 2. Scan the items inside this specific order to see if GameB is present
            foreach (var item in order.CartOrdered.Items())
            {
                if (item.Product.definition.id == "com.neatstreet.cosmicchase.gameb")
                {
                    foundGameB = true;
                }
            }

            // 3. Pass the verified historical order to your existing pipeline
            ProcessPurchase(order);
        }

        // 4. Clean up your "Loading" UI page based on the results
        if (foundGameB)
        {
            Debug.LogWarning("Restore complete! GameB unlocked and UI redirected via GrantProduct.");
            // Note: ProcessPurchase calls GrantProduct, which handles:
            // gs.hasUnlockedGameB = true and Sgs.NewMenuPage(Sgs.Pages.Home)
        }
        else
        {
            Debug.LogWarning("Restore complete: GameB was not found in this user's history.");
        }
    }
    public void RequestPlatformBillingUI(string productID)
    { //a wrapper that calls PurchaseProduct(), which brings up the apple/google/fakeStore dialogue to confirm buying an in app purchase
        //De\bug.Log("RequestPlatformBillingUI()");
        Sgs.ButtonHandler(Sgs.SgsButtonHandler.Loading); //pass SgsButtonHandler.Loading to make this create a page that says "loading" so that users can't spam-click your store's ui and double purchase consumables, or tweak out the logic for "double purchasing" a non consumable
        storeController.PurchaseProduct(productID);
    }
    
    void OnPurchasePending(PendingOrder order)
    { //handles event that is triggered when user clicks YOUR own store's ui button to bring up the platform-specific billing ui. (Necessary because during this connection delay, a user might spam click a button multiple times, and attempt to perform multiple transactions. However, I might not even need this event, as I can just change to a different page that says "loading" in response to the RequestPlatformBillingUI() that gets called when I click on my UI button)
        //De\bug.Log("OnPurchasePending()");
        ProcessPurchase(order);
        storeController.ConfirmPurchase(order);
        //ProcessPurchase(order);
    }
    void OnPurchaseConfirmed(Order order)
    { //triggered in response to clicking "buy" or the likes on the apple / google / fake store billing ui
        //De\bug.Log("OnPurchaseConfirmed()");
        Debug.LogWarning("Remember GameB Locally for offline use");
        SecurePlayerPrefs.SetInt("GameB", 1); //have secure player prefs remember locally that you have unlocked game b. (SecurePlayerPrefs will encrypt the data so it is hard for people to tamper with this locally, and even if they do succeed, as soon as they log back into the app when connected to wifi, it will get overwritten by the official app store records.)
    }
    void OnPurchaseFailed(FailedOrder failedOrder)
    { //triggered in response to clicking "cancel" or the likes on the apple / google / fake store billing ui
        //De\bug.Log("Canceled Billing Transaction");
        Sgs.NewMenuPage(Sgs.Pages.Home);
    }
    void ProcessPurchase(Order order)
    { //process each item that was requested to be purchased, and then call GrantProduct() on each item to handle the actual sgs game logic of what should be awarded in response to it.
        Debug.LogWarning("ProcessPurchase()");
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
        //gets called both when requesting to purchase something, when restoring purchases (after delete then re-install), and when fetching purchases (opening the app again and it will remember what you have purchased)
        Debug.LogWarning("GrantProduct()");
        string product = cartItem.Product.definition.id;
        if(gs != null)
        {
            switch(product)
            {
                case "com.neatstreet.cosmicchase.gameb":
                //De\bug.Log("unlocked game b");
                gs.hasUnlockedGameB = true;
                Debug.LogWarning("IAP BINGO");
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
