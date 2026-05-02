using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using System.Threading.Tasks;


/*MAKING SENSE OF HOW THIS API WORKS
  - you are using unity's "In-App Purchasing" package from the unity registry. Make sure it is version 5, as version 4 is very different and strongly depracated
  - you also need the game synced up properly with unity dashboard
  - most of your api scripting is done through the StoreController interface, which basically centralizes a bunch of events that can happen related to purchasing, and sends data with them. Your job is to attach handlers to the various events
    - You need to use FetchPurchases() at the beginning of the game to apply stuff that the person has already bought (should persist even after reinstalling the app)
    - You need to use FetchProducts() to understand if the thing you want to buy is still available to buy (person shouldn't be able to buy a non-consumable twice)
    - Note that FetchPurchases() and FetchProducts() are both event-driven asynchronous operations, as opposed to Task-driven async. Only the latter uses the await keyword.
        - Meaning you call these two methods, and then await the event that they have finished to actually get the data.
*/

public class IAPs : MonoBehaviour
{
    private StoreController storeController;
    private GlobalState gs;
    private List<ProductDefinition> products;

    async Task Awake()
    {
        
        storeController = UnityIAPServices.StoreController();
        gs = GameObject.FindFirstObjectByType<GlobalState>();
        try
        {
            await storeController.Connect();
            RestorePurchases();  //if you can connect to the store, use their records as the official "source of truth" regarding which things have been purchased.
        }
        catch
        {
            //if you are offline or otherwise can't connect to the store, use SecurePlayerPrefs to establish which purchases have been attained instead. (Ironically, SecurePlayerPrefs are less secure than the store's backend, so this offline approach should only be used as a fallback. Not risky enough for me to be worried though.)
        }

        //define which products are available in your game
        products = new List<ProductDefinition>();
//BUGS?: double check that "GameB" is the correct id or whatever to be using. Also, unsure if this conflicts or cooperates with the "catalog" I already set up in the editor
        products.Add(new("GameB", ProductType.NonConsumable));
    }

    public void RestorePurchases()
    { //call this every time the game starts up. You will attempt to establish which game modes you have unlocked based on app store's servers, but if offline, you will use SecurePlayerPrefs. (Note that it would be easier for players to hack your SecurePlayerPrefs values offline and play your game for free, but any time RestorePurchases() is called online, it will get overriden by whether or not Apple knows they truly paid or not. And since it is necessary to be online to submit to leaderboards, cheating risk is low enough for me not to worry)
        storeController.FetchPurchases();  //calls OnPurchasesFetched when completed
    }
        void OnPurchasesFetched(Orders orders)
        { //pre-defined event handler for when FetchPurchases() is completed
            //
        }
    public void BuySomething()
    { //call this with a parameter of some kind to buy game b, c and any other things you might sell (perhaps even a remove ads button at some point)
        storeController.FetchProducts(products);
    }
        void OnProductsFetched(List<Product> products)
        {  //pre-defined event handler for when FetchProducts() is completed
            //
        }
}

/*// My first manual attempt
public class IAPs
{
    private StoreController storeController;
    private GlobalState gs;
    
    async void Initialize()
    {
        storeController = UnityIAPServices.StoreController();
        gs = GameObject.FindFirstObjectByType<GlobalState>();

        await storeController.Connect();  
        
        storeController.OnPurchasePending += OnPurchasePending;      
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnPurchasesFetched += OnPurchasesFetched;  
    
        var initialProductsToFetch = new List<ProductDefinition>  
        {  
            new("GameB", ProductType.NonConsumable),  
        };  
    
        storeController.FetchProducts(initialProductsToFetch);  

    }
}*/

/*AI attempt (pure garbage, as it is pulling from both version 4 and 5 and poor documentation overall)

using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;
using System.Threading.Tasks;

public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance;

    private StoreController storeController;
    private GlobalState gs;

    void Awake()
    {
        Instance = this;
        gs = GameObject.FindObjectOfType<GlobalState>();
    }

    async void Start()
    {
        await InitializeIAP();
    }

    async Task InitializeIAP()
    {
        storeController = UnityIAPServices.StoreController();

        // Attach event handlers BEFORE calling anything important
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnProductsFetched += OnProductsFetched;

        await storeController.Connect();

        var products = new List<ProductDefinition>
        {
            new ProductDefinition("GameB", ProductType.NonConsumable)
        };

        storeController.FetchProducts(products);
    }

    void OnProductsFetched(List<Product> products)
    {
        Debug.Log("Products fetched");

        // After products, fetch purchases (important for restore)
        storeController.FetchPurchases();
    }

    void OnPurchasesFetched(Orders orders)
    {
        Debug.Log("Purchases fetched (restore check)");

        foreach (var order in orders)
        {
            if (order.Product.definition.id == "GameB")
            {
                gs.hasUnlockedGameB = true;
            }
        }
    }

    void OnPurchasePending(PendingOrder order)
    {
        Debug.Log("Purchase pending: " + order.Product.definition.id);

        if (order.Info.PurchasedProductInfo == "gameB_unlock")
        {
            gs.hasUnlockedGameB = true;
        }

        storeController.ConfirmPurchase(order);
    }

    public void BuyGameB()
    {
        storeController?.Purchase("GameB");
    }
}*/